using Newtonsoft.Json;
using Org.OpenAPITools.Client;

namespace Org.OpenAPITools.com.blockchain.exchange.rest.model;

public class Arguments : SubscriptionMessage
{
	public Arguments(string symbol = null, int? granularity = null, string clOrdId = null,
		OrdType? ordType = null, Side? side = null, double? orderQty = null,
		TimeInForce? timeInForce = null, double? price = null)
	{
		Symbol = symbol;
		ClOrdId = clOrdId;
		OrdType = ordType;
		Side = side;
		OrderQty = orderQty;
		TimeInForce = timeInForce;
		Price = price;
		Granularity = granularity;
	}

	[JsonProperty("granularity")]
	public int? Granularity { get; set; }
	[JsonProperty("price")]
	public double? Price { get; set; }
	[JsonProperty("timeInForce")]
	public TimeInForce? TimeInForce { get; set; }
	[JsonProperty("orderQty")]
	public double? OrderQty { get; set; }
	[JsonProperty("side")]
	public Side? Side { get; set; }
	[JsonProperty("ordType")]
	public OrdType? OrdType { get; set; }
	[JsonProperty("clOrdId")]
	public string ClOrdId { get; set; }
	[JsonProperty("symbol")]
	public string Symbol { get; set; }

	public void SetCommand(string action, Channel channel)
	{
		Action = action;
		Channel = channel;
	}
}