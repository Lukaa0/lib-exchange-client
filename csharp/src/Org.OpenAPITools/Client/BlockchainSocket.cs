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

public class BlockchainSocket
{
	public List<Channel> SubscribedEvents;

	/// <summary>
	/// </summary>
	/// <param name="configuration"></param>
	public BlockchainSocket(Configuration configuration)
	{
		Configuration = configuration;
		configuration.ApiKey = new Dictionary<string, string> { { "API_SECRET", "secret here" } };
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

	// ReSharper disable once TooManyArguments
	// ReSharper disable once MethodTooLong
	/// <summary>
	///   Connects and subscribes to given channels, if all channels are anonymous, it will just subscribe without
	///   authentication. If there's a mixture of anonymous and authenticated, it will authenticate first and then will
	///   subscribe to all the channels passed in the function.
	/// </summary>
	/// <param name="events"></param>
	/// <param name="symbol"></param>
	/// <param name="onL2Message"></param>
	/// <param name="onL3Message"></param>
	/// <param name="onPriceUpdate"></param>
	/// <param name="onSymbolUpdate"></param>
	/// <param name="onTicketUpdate"></param>
	/// <param name="onTradeUpdate"></param>
	/// <exception cref="InvalidOperationException"></exception>
	/// TODO: Maybe its better to have separate functions for each events?
	public async Task ConnectAndSubscribe(List<Channel> events, Arguments arguments,
		Action<List<Bids>> onL2Message = null, Action<List<Bids>> onL3Message = null,
		Action<Price> onPriceUpdate = null, Action<List<SymbolStatus>> onSymbolUpdate = null,
		Action<Ticker> onTicketUpdate = null, Action<Trade> onTradeUpdate = null,
		Action<List<Balance>> onBalanceUpdate = null)
	{
		WebSocket.MessageReceived?.Subscribe(async message =>
		{
			var subscriptionMessage =
				JsonConvert.DeserializeObject<SubscriptionMessage>(message.Text);
			if (subscriptionMessage.Channel == Channel.auth &&
				subscriptionMessage.Event != "rejected")
			{
				await SubscribeToEvents(events, arguments);
			}
			else if (subscriptionMessage.Event == "subscribed")
			{
				//receipt
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
					var priceValues = JObject.Parse(message.Text)["price"].Select(token => (int)token).
						ToArray();
					onPriceUpdate(new Price(priceValues[0], priceValues[1], priceValues[2],
						priceValues[3], priceValues[4], priceValues[5]));
					break;
				case Channel.symbols:
					onSymbolUpdate(DeserializeResponse<List<SymbolStatus>>(message.Text, "symbols"));
					break;
				case Channel.ticker:
					onTicketUpdate(DeserializeResponse<Ticker>(message.Text));
					break;
				case Channel.trades:
					onTradeUpdate(DeserializeResponse<Trade>(message.Text));
					break;
				//Authenticated Channels
				case Channel.trading:
					break;
				case Channel.NewOrderSingle:
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
		});
		WebSocket.DisconnectionHappened.Subscribe(error =>
		{
			WebSocket.Dispose();
			if (error?.Exception != null)
				throw error.Exception;
			throw new WebException(error.CloseStatusDescription);
		});
		await WebSocket.Start();
		if (events.Any(CheckIfEventRequiresAuthentication))
		{
			var authMessage = new
			{
				token = Configuration.ApiKey["API_SECRET"], action = "subscribe", channel = "auth"
			};
			await Task.Run(() => WebSocket.Send(JsonConvert.SerializeObject(authMessage)));
		}
		else
		{
			await SubscribeToEvents(events, arguments);
		}
	}

	private bool CheckIfEventRequiresAuthentication(Channel channel) =>
		channel is Channel.trading or Channel.NewOrderSingle or Channel.CancelOrderRequest
			or Channel.OrderMassCancelRequest or Channel.OrderMassStatusRequest or Channel.balances;

	private async Task SubscribeToEvents(List<Channel> channels, Arguments arguments)
	{
		foreach (var channel in channels)
		{
			var subscriptionMessage =
				new SubscriptionMessage("subscribe", Symbol: arguments.Symbol);
			subscriptionMessage.Channel = channel;
			if (channel == Channel.prices)
				subscriptionMessage.Granularity = arguments.Granularity.ToString();
			await Task.Run(() => WebSocket.Send(JsonConvert.SerializeObject(subscriptionMessage)));
		}
	}

	private T DeserializeResponse<T>(string json, string propertyName = null) =>
		JsonConvert.DeserializeObject<T>(propertyName == null
			? json
			: JObject.Parse(json)[propertyName].ToString());

	public bool IsConnected() => WebSocket.IsRunning;
}