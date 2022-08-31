using Org.OpenAPITools.Client;

var client = new BlockchainSocket(new Configuration());
await client.ConnectAndSubscribe(new List<Channel> { Channel.balances },
	new Arguments("BTC-USDT", 60), onBalanceUpdate: message =>
	{
		var balances = message;
	});
Console.ReadKey();