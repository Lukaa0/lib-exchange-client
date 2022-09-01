using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Org.OpenAPITools.com.blockchain.exchange.rest.model
{
	public class OrderSummaryResponse
	{
		[JsonProperty("seqnum")]
		public int Seqnum { get; set; }

		[JsonProperty("event")]
		public string Event { get; set; }

		[JsonProperty("channel")]
		public string Channel { get; set; }

		[JsonProperty("orders")]
		public List<OrderSummary> Orders { get; set; }
	}
}
