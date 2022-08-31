namespace Org.OpenAPITools.Client;

public sealed record Arguments(string Symbol = null, int? Granularity = null)
{
	public int? Granularity = Granularity;
	public string Symbol = Symbol;
}