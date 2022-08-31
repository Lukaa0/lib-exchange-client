using Newtonsoft.Json;

namespace Org.OpenAPITools.Client;

public sealed record Bids
{
	[JsonProperty("px")]
	public double Price { get; set; }
	[JsonProperty("qty")]
	public double Quantity { get; set; }
	[JsonProperty("num")]
	public int NumberOfOrders { get; set; }
}