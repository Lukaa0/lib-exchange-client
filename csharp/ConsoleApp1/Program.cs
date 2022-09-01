using Org.OpenAPITools.Client;
using Org.OpenAPITools.com.blockchain.exchange.rest.model;

var client = new BlockchainSocket(new Configuration());
var arguments = new Arguments("BTC-USDT", 60, "someId", OrdType.MARKET, Side.BUY);
await client.ConnectAndSubscribe(new List<Channel> { Channel.prices, Channel.l2 }, arguments,
	onPriceUpdate: message =>
	{
		var prices = message;
	}, onL2Message: message =>
	{
		var bids = message;
	});
Console.ReadKey();