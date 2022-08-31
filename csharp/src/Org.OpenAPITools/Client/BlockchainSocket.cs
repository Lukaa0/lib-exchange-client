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
	/// <summary>
	/// </summary>
	/// <param name="configuration"></param>
	public BlockchainSocket(Configuration configuration)
	{
		Configuration = configuration;
		configuration.ApiKey = new Dictionary<string, string>
		{
			{
				"API_SECRET",
				"secret"
			}
		};
		var nativeSocket = new ClientWebSocket
		{
			Options = { KeepAliveInterval = TimeSpan.FromSeconds(5) }
		};
		nativeSocket.Options.SetRequestHeader("Origin", configuration.Origin);
		Factory = () => nativeSocket;
		WebSocket = new WebsocketClient(new Uri(Configuration.WebSocketUrl), Factory);
	}

	public bool IsAuthenticated { get; set; }
	private Configuration Configuration { get; }
	private Func<ClientWebSocket> Factory { get; }
	private WebsocketClient WebSocket { get; }

	private bool CheckIfEventRequiresAuthentication(Event @event) =>
		@event is Event.trading or Event.NewOrderSingle or Event.CancelOrderRequest
			or Event.OrderMassCancelRequest or Event.OrderMassStatusRequest or Event.balances;

	// ReSharper disable once TooManyArguments
	// ReSharper disable once MethodTooLong
	/// <summary>
	///   Connects and subscribes to given events
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
	public async Task ConnectAndSubscribe(List<Event> events, Arguments arguments, Action<List<Bids>> onL2Message = null,
		Action<List<Bids>> onL3Message = null, Action<Price> onPriceUpdate = null,
		Action<List<SymbolStatus>> onSymbolUpdate = null, Action<Ticker> onTicketUpdate = null,
		Action<Trade> onTradeUpdate = null)
	{
		WebSocket.MessageReceived?.Subscribe(message =>
		{
			var subscriptionMessage =
				JsonConvert.DeserializeObject<SubscriptionMessage>(message.Text);
			if (subscriptionMessage.Event == "subscribed")
			{
				//receipt
			}
			else
			{
				switch (subscriptionMessage.Channel)
				{
				case Event.l2:
					onL2Message(DeserializeResponse<List<Bids>>(message.Text, "bids"));
					break;
				case Event.l3:
					onL3Message(DeserializeResponse<List<Bids>>(message.Text, "bids"));
					break;
				case Event.prices:
					var priceValues = JObject.Parse(message.Text)["price"].Select(token => (int)token).
						ToArray();
					onPriceUpdate(new Price(priceValues[0], priceValues[1], priceValues[2],
						priceValues[3], priceValues[4], priceValues[5]));
					break;
				case Event.symbols:
					onSymbolUpdate(DeserializeResponse<List<SymbolStatus>>(message.Text, "symbols"));
					break;
				case Event.ticker:
					onTicketUpdate(DeserializeResponse<Ticker>(message.Text));
					break;
				case Event.trades:
					onTradeUpdate(DeserializeResponse<Trade>(message.Text));
					break;
				//Authenticated Channels
				case Event.trading:
					break;
				case Event.NewOrderSingle:
					break;
				case Event.CancelOrderRequest:
					break;
				case Event.OrderMassCancelRequest:
					break;
				case Event.OrderMassStatusRequest:
					break;
				case Event.balances:
					break;
				case Event.auth:
					//Authentication receipt
					IsAuthenticated = true;
					break;
				default:
					throw new ArgumentException("Invalid event");
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
		var subscriptionMessage = new SubscriptionMessage("subscribe", Symbol: arguments.Symbol);
		if (events.Any(@event => CheckIfEventRequiresAuthentication(@event)))
		{
			var authMessage = new[]
			{
				new
				{
					token = Configuration.ApiKey["API_SECRET"],
					action = "subscribe",
					channel = Event.auth
				}
			};
			await Task.Run(() => WebSocket.Send(JsonConvert.SerializeObject(authMessage)));
		}
		foreach (var @event in events)
		{
			if (CheckIfEventRequiresAuthentication(@event) && !IsAuthenticated)
				continue;
			subscriptionMessage.Channel = @event;
			if (@event == Event.prices)
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