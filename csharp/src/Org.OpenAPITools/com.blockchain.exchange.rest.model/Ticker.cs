using Newtonsoft.Json;

namespace Org.OpenAPITools.Client;

public sealed record Ticker
{
	[JsonProperty("seqnum")]
	public int Seqnum { get; set; }
	[JsonProperty("event")]
	public string Event { get; set; }
	[JsonProperty("channel")]
	public string Channel { get; set; }
	[JsonProperty("symbol")]
	public string Symbol { get; set; }
	[JsonProperty("price_24h")]
	public double Price24h { get; set; }
	[JsonProperty("volume_24h")]
	public double Volume24h { get; set; }
	[JsonProperty("last_trade_price")]
	public double LastTradePrice { get; set; }
}