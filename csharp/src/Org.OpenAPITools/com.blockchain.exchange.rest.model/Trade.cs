using System;
using Newtonsoft.Json;

namespace Org.OpenAPITools.Client;

public sealed record Trade
{
	[JsonProperty("seqnum")]
	public int Seqnum { get; set; }
	[JsonProperty("event")]
	public string Event { get; set; }
	[JsonProperty("channel")]
	public string Channel { get; set; }
	[JsonProperty("symbol")]
	public string Symbol { get; set; }
	[JsonProperty("timestamp")]
	public DateTime Timestamp { get; set; }
	[JsonProperty("side")]
	public string Side { get; set; }
	[JsonProperty("qty")]
	public double Quantity { get; set; }
	[JsonProperty("price")]
	public double Price { get; set; }
	[JsonProperty("trade_id")]
	public string TradeId { get; set; }
}