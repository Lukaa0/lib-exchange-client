using Org.OpenAPITools.Client;
using Org.OpenAPITools.com.blockchain.exchange.rest.model;

var client = new BlockchainSocket(new Configuration());
var arguments = new Arguments("BTC-USDT", 60, "someId", OrdType.MARKET, Side.BUY);
await client.ConnectAndSubscribe(
	new List<Channel> { Channel.prices, Channel.l2, Channel.balances }, arguments,
	onPriceUpdate: message =>
	{
		var prices = message;
	}, onL2Message: message =>
	{
		var order = message;
	}, onBalanceUpdate: message =>
	{
		var balance = message;
	});
await client.SubscribeToL2OrderBookAsync("BTC-USDT", message =>
{
	var orders = message;
});
await client.SubscribeToL3OrderBookAsync("BTC-USDT", message =>
{
	var orders = message;
});
await client.SubscribeToSymbolsAsync("BTC-USDT", message =>
{
	var symbols = message;
});
await client.SubscribeToTickerAsync("BTC-USDT", message =>
{
	var ticker = message;
});
await client.SubscribeToTradesAsync("BTC-USDT", message =>
{
	var trades = message;
});
await client.SubscribeToPricesAsync("BTC-USDT", 60, message =>
{
	var trades = message;
});
Console.ReadKey();