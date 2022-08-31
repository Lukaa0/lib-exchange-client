namespace Org.OpenAPITools.Client;

public enum Channel
{
	//Anonymous
	l2,
	l3,
	prices,
	symbols,
	ticker,
	trades,
	//Authenticated
	trading,
	NewOrderSingle,
	CancelOrderRequest,
	OrderMassCancelRequest,
	OrderMassStatusRequest,
	balances,
	auth
}