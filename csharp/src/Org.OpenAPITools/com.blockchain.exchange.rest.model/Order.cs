using Newtonsoft.Json;

namespace Org.OpenAPITools.com.blockchain.exchange.rest.model;

public class Order
{
	[JsonProperty("clOrdID")]
	public string ClOrdID { get; set; }
	[JsonProperty("symbol")]
	public string Symbol { get; set; }
	[JsonProperty("ordType")]
	public OrdType OrdType { get; set; }
	[JsonProperty("timeInForce")]
	public TimeInForce TimeInForce { get; set; }
	[JsonProperty("side")]
	public Side Side { get; set; }
	[JsonProperty("orderQty")]
	public double OrderQty { get; set; }
	[JsonProperty("price")]
	public double Price { get; set; }
	[JsonProperty("execInst")]
	public string ExecInst { get; set; }
}