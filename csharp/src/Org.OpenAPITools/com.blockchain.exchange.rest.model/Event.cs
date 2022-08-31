namespace Org.OpenAPITools.Client;

public enum Event
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