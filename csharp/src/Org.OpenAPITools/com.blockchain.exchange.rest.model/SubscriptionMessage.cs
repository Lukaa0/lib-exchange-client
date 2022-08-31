using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Org.OpenAPITools.Client;

/// <summary>
/// </summary>
/// <param name="Action"></param>
/// <param name="Channel"></param>
/// <param name="Symbol"></param>
[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public record SubscriptionMessage(string Action = "subscribe", Channel Channel = Channel.l2,
	string Symbol = null, string Event = null)
{
	[JsonProperty("action")]
	public string Action { get; set; } = Action;
	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("channel")]
	public Channel Channel { get; set; } = Channel;
	[JsonProperty("symbol")]
	public string Symbol { get; set; } = Symbol;
	[JsonProperty("event")]
	public string Event { get; set; } = Event;
	[JsonProperty("granularity")]
	public string Granularity { get; set; } = Event;
}