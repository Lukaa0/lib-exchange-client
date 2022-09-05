using System.Collections.Generic;
using Newtonsoft.Json;

namespace Org.OpenAPITools.com.blockchain.exchange.rest.model;

public class OrderList
{
	[JsonProperty("seqnum")]
	public int Seqnum { get; set; }
	[JsonProperty("event")]
	public string Event { get; set; }
	[JsonProperty("channel")]
	public string Channel { get; set; }
	[JsonProperty("orders")]
	public List<Order> Orders { get; set; }
}