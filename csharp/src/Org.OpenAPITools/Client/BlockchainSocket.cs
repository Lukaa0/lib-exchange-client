using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.OpenAPITools.com.blockchain.exchange.rest.model;
using Websocket.Client;

namespace Org.OpenAPITools.Client;

public sealed class BlockchainSocket
{
	public BlockchainSocket(Configuration configuration)
	{
		Configuration = configuration;
		configuration.ApiKey = new Dictionary<string, string> { { "API_SECRET", "secret" } };
		var nativeSocket = new ClientWebSocket
		{
			Options = { KeepAliveInterval = TimeSpan.FromSeconds(5) }
		};
		nativeSocket.Options.SetRequestHeader("Origin", configuration.Origin);
		Factory = () => nativeSocket;
		WebSocket = new WebsocketClient(new Uri(Configuration.WebSocketUrl), Factory);
	}

	private Configuration Configuration { get; }
	private Func<ClientWebSocket> Factory { get; }
	private WebsocketClient WebSocket { get; }

	public async Task SubscribeToPricesAsync(string symbol, int granularity,
		Action<Price> onMessageReceived)
	{
		using var websocket = new WebsocketClient(new Uri(Configuration.WebSocketUrl), Factory);
		websocket.MessageReceived.Subscribe(message =>
		{
			PriceUpdateCallback(onMessageReceived, message);
		});
		await SendToPrices(new Arguments(symbol, granularity),
			Enum.GetName(typeof(Channel), Channel.prices), websocket);
	}

	public async Task SubscribeToL2OrderBookAsync(string symbol,
		Action<List<Bids>> onMessageReceived) =>
		await SubscribeToAnonymousChannel(symbol, Channel.l2, onMessageReceived, "bids");

	public async Task
		SubscribeToCreateOrderChannel(Order order, Action<List<Bids>> onMessageReceived) =>
		await SubscribeToAuthenticatedChannel(
			new Arguments(order.Symbol, clOrdId: order.ClOrdID, ordType: order.OrdType,
				side: order.Side, orderQty: order.OrderQty, timeInForce: order.TimeInForce,
				price: order.Price), Channel.l2, onMessageReceived);

	public async Task SubscribeToL3OrderBookAsync(string symbol,
		Action<List<Bids>> onMessageReceived) =>
		await SubscribeToAnonymousChannel(symbol, Channel.l3, onMessageReceived, "bids");

	public async Task SubscribeToSymbolsAsync(string symbol,
		Action<SymbolStatus> onMessageReceived) =>
		await SubscribeToAnonymousChannel(symbol, Channel.symbols, onMessageReceived, "symbols");

	public async Task SubscribeToTickerAsync(string symbol,
		Action<PriceEvent> onMessageReceived) =>
		await SubscribeToAnonymousChannel(symbol, Channel.ticker, onMessageReceived);

	public async Task SubscribeToTradesAsync(string symbol, Action<Trade> onMessageReceived) =>
		await SubscribeToAnonymousChannel(symbol, Channel.trades, onMessageReceived);

	private async Task SubscribeToAnonymousChannel<T>(string symbol, Channel channel,
		Action<T> onMessageReceived, string property = null)
	{
		var websocket = new WebsocketClient(new Uri(Configuration.WebSocketUrl), Factory);
		websocket.MessageReceived.Subscribe(message =>
		{
			var subscriptionMessage =
				JsonConvert.DeserializeObject<SubscriptionMessage>(message.Text);
			if (subscriptionMessage.Event == "subscribed")
				//receipt
				return;
			if (subscriptionMessage.Event == "rejected")
				throw new Exception("Connection rejected: " + subscriptionMessage.Text +
					" Channel: " + subscriptionMessage.Channel);
			onMessageReceived(DeserializeResponse<T>(message.Text, property));
		});
		websocket.DisconnectionHappened.Subscribe(error =>
		{
			if (error?.Exception != null)
				throw new Exception($"{error.Exception.Message} At channel: {channel}",
					error.Exception);
			throw new WebException($"{error.CloseStatusDescription} At channel: {channel}");
		});
		await websocket.Start();
		var arguments = new Arguments(symbol);
		arguments.SetCommand("subscribe", channel);
		await SendMessageToChannelAsync(arguments, websocket);
	}

	private async Task SubscribeToAuthenticatedChannel<T>(Arguments arguments, Channel channel,
		Action<T> onMessageReceived)
	{
		var websocket = new WebsocketClient(new Uri(Configuration.WebSocketUrl), Factory);

		async void OnAuthenticate(ResponseMessage message)
		{
			var subscriptionMessage =
				JsonConvert.DeserializeObject<SubscriptionMessage>(message.Text);
			if (subscriptionMessage.Channel == Channel.auth &&
				subscriptionMessage.Event != "rejected")
			{
				arguments.SetCommand("subscribe", channel);
				await SendMessageToChannelAsync(arguments, websocket);
			}
			else if (subscriptionMessage.Event == "subscribed")
				//receipt
			{ }
			else if (subscriptionMessage.Event == "rejected")
			{
				throw new Exception("Connection rejected: " + subscriptionMessage.Text +
					" Channel: " + subscriptionMessage.Channel);
			}
			else
			{
				onMessageReceived(DeserializeResponse<T>(message.Text));
			}
		}

		websocket.MessageReceived.Subscribe(OnAuthenticate);
		websocket.DisconnectionHappened.Subscribe(error =>
		{
			if (error?.Exception != null)
				throw new Exception($"{error.Exception.Message} At channel: {channel}",
					error.Exception);
			throw new WebException($"{error.CloseStatusDescription} At channel: {channel}");
		});
		await websocket.Start();
		await AuthenticateAsync(websocket);
	}

	// ReSharper disable once TooManyArguments
	// ReSharper disable once MethodTooLong
	/// <summary>
	///   Connects and subscribes to given channels, if all channels are anonymous, it will just subscribe without
	///   authentication. If there's a mixture of anonymous and authenticated, it will authenticate first and then will
	///   subscribe to all the channels passed in the function.
	/// </summary>
	/// <param name="channels"></param>
	/// <param name="arguments"></param>
	/// <param name="onL2Message"></param>
	/// <param name="onL3Message"></param>
	/// <param name="onPriceUpdate"></param>
	/// <param name="onSymbolUpdate"></param>
	/// <param name="onTickerUpdate"></param>
	/// <param name="onTradesUpdate"></param>
	/// <param name="onBalanceUpdate"></param>
	/// <param name="onTradingUpdate"></param>
	/// <exception cref="InvalidOperationException"></exception>
	/// NOTE: I will eventually separate this method into multiple ones.
	public async Task ConnectAndSubscribe(List<Channel> channels, Arguments arguments,
		Action<List<Bids>> onL2Message = null, Action<List<Bids>> onL3Message = null,
		Action<Price> onPriceUpdate = null, Action<List<SymbolStatus>> onSymbolUpdate = null,
		Action<PriceEvent> onTickerUpdate = null, Action<Trade> onTradesUpdate = null,
		Action<List<Balance>> onBalanceUpdate = null, Action<OrderList> onTradingUpdate = null,
		Action<OrderList> onCreateOrder = null)
	{
		async void OnMessageReceived(ResponseMessage message)
		{
			var subscriptionMessage =
				JsonConvert.DeserializeObject<SubscriptionMessage>(message.Text);
			if (subscriptionMessage.Channel == Channel.auth &&
				subscriptionMessage.Event != "rejected")
			{
				await SubscribeToChannelsAsync(channels, arguments, WebSocket);
			}
			else if (subscriptionMessage.Event == "subscribed")
			{
				//receipt 
			}
			else if (subscriptionMessage.Event == "rejected")
			{
				throw new Exception("Connection rejected: " + subscriptionMessage.Text +
					" Channel: " + subscriptionMessage.Channel);
			}
			else
			{
				switch (subscriptionMessage.Channel)
				{
				case Channel.l2:
					onL2Message(DeserializeResponse<List<Bids>>(message.Text, "bids"));
					break;
				case Channel.l3:
					onL3Message(DeserializeResponse<List<Bids>>(message.Text, "bids"));
					break;
				case Channel.prices:
					PriceUpdateCallback(onPriceUpdate, message);
					break;
				case Channel.symbols:
					onSymbolUpdate(DeserializeResponse<List<SymbolStatus>>(message.Text, "symbols"));
					break;
				case Channel.ticker:
					onTickerUpdate(DeserializeResponse<PriceEvent>(message.Text));
					break;
				case Channel.trades:
					onTradesUpdate(DeserializeResponse<Trade>(message.Text));
					break;
				//Authenticated Channels
				case Channel.trading:
					onTradingUpdate(DeserializeResponse<OrderList>(message.Text));
					break;
				case Channel.NewOrderSingle:
					onCreateOrder(DeserializeResponse<OrderList>(message.Text));
					break;
				case Channel.CancelOrderRequest:
					break;
				case Channel.OrderMassCancelRequest:
					break;
				case Channel.OrderMassStatusRequest:
					break;
				case Channel.balances:
					onBalanceUpdate(DeserializeResponse<List<Balance>>(message.Text, "balances"));
					break;
				case Channel.auth:
					//Authentication receipt
					break;
				default:
					throw new ArgumentException("Invalid channel" + subscriptionMessage);
				}
			}
		}

		WebSocket?.MessageReceived?.Subscribe(OnMessageReceived);
		WebSocket?.DisconnectionHappened.Subscribe(error =>
		{
			if (error?.Exception != null)
				throw error.Exception;
			throw new WebException(error.CloseStatusDescription);
		});
		await WebSocket.Start();
		if (channels.Any(CheckIfEventRequiresAuthentication))
			await AuthenticateAsync(WebSocket);
		else
			await SubscribeToChannelsAsync(channels, arguments, WebSocket);
	}

	private static void PriceUpdateCallback(Action<Price> onPriceUpdate,
		ResponseMessage message)
	{
		var priceValues = JObject.Parse(message.Text)["price"].Select(token => (double)token).
			ToArray();
		onPriceUpdate(new Price(priceValues[0], priceValues[1], priceValues[2], priceValues[3],
			priceValues[4], priceValues[5]));
	}

	private async Task AuthenticateAsync(WebsocketClient websocket)
	{
		var authMessage = new
		{
			token = Configuration.ApiKey["API_SECRET"], action = "subscribe", channel = "auth"
		};
		await Task.Run(() => websocket.Send(JsonConvert.SerializeObject(authMessage)));
	}

	private bool CheckIfEventRequiresAuthentication(Channel channel) =>
		channel is Channel.trading or Channel.NewOrderSingle or Channel.CancelOrderRequest
			or Channel.OrderMassCancelRequest or Channel.OrderMassStatusRequest or Channel.balances;

	private async Task SubscribeToChannelsAsync(List<Channel> channels, Arguments arguments,
		WebsocketClient websocket)
	{
		foreach (var channel in channels)
		{
			arguments.SetCommand("subscribe", channel);
			await SendMessageToChannelAsync(arguments, websocket);
		}
	}

	private async Task SendMessageToChannelAsync(Arguments arguments, WebsocketClient websocket)
	{
		var channelName = Enum.GetName(typeof(Channel), arguments.Channel);
		switch (arguments.Channel)
		{
		case Channel.prices:
		{
			await SendToPrices(arguments, channelName, WebSocket);
			break;
		}
		case Channel.NewOrderSingle:
		{
			await SubscribeToNewOrderSingle(arguments, channelName, websocket);
			break;
		}
		//Channels that only require symbol as an argument
		default:
			await Task.Run(() => websocket?.Send(JsonConvert.SerializeObject(new
			{
				channel = channelName, action = arguments.Action, symbol = arguments.Symbol
			})));
			break;
		}
	}

	private async Task
		SubscribeToNewOrderSingle(Arguments arguments, string channelName,
			WebsocketClient websocket) =>
		await Task.Run(() => websocket?.Send(JsonConvert.SerializeObject(new
		{
			channel = channelName,
			action = arguments.Action,
			clOrdID = arguments.ClOrdId,
			symbol = arguments.Symbol,
			ordType = arguments.OrdType,
			timeInForce = arguments.TimeInForce,
			side = arguments.Side,
			orderQty = arguments.OrderQty,
			price = arguments.Price
		})));

	private async Task SendToPrices(Arguments arguments, string channelName,
		WebsocketClient websocket) =>
		await Task.Run(() => websocket?.Send(JsonConvert.SerializeObject(new
		{
			channel = channelName,
			action = arguments.Action,
			symbol = arguments.Symbol,
			granularity = arguments.Granularity
		})));

	private T DeserializeResponse<T>(string json, string propertyName = null) =>
		JsonConvert.DeserializeObject<T>(propertyName == null
			? json
			: JObject.Parse(json)[propertyName].ToString());

	public bool IsConnected() => WebSocket.IsRunning;
}