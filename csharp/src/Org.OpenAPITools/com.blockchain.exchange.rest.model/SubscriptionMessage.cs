using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Org.OpenAPITools.Client;

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class SubscriptionMessage
{
	/// <summary>
	///   A model for sending/receiving subscription requests
	/// </summary>
	/// <param name="action">Can be either subscribe or unsubscribe</param>
	/// <param name="channel">Name of the channel</param>
	public SubscriptionMessage(string action, Channel channel)
	{
		Action = action;
		Channel = channel;
	}

	public SubscriptionMessage() { }
	[JsonProperty("action")]
	public string Action { get; set; }
	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("channel")]
	public Channel Channel { get; set; }
	[JsonProperty("event")]
	public string Event { get; set; }
	[JsonProperty("text")]
	public string Text { get; set; }
}