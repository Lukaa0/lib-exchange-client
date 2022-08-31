// See https://aka.ms/new-console-template for more information
using Org.OpenAPITools.Client;

var client = new BlockchainSocket(new Configuration());
await client.ConnectAndSubscribe(
	new List<Event>
	{
		Event.ticker,
		Event.prices,
		Event.trading,
		Event.l2,
		Event.l3
	}, new Arguments("BTC-USDT", 60), onTicketUpdate: message =>
	{
		var ticker = message;
	}, onPriceUpdate: message =>
	{
		var price = message;
	}, onL2Message: message =>
	{
		var l2 = message;
	}, onL3Message: message =>
	{
		var l3 = message;
	}, onTradeUpdate: message =>
	{
		var trade = message;
	});
Console.ReadKey();