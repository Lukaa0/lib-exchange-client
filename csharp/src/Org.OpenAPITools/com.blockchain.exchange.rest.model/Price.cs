namespace Org.OpenAPITools.Client;

public sealed record Price(double Timestamp, double Open, double High, double Low,
	double Close, double Volume)
{
	public double Timestamp { get; set; } = Timestamp;
	public double Open { get; set; } = Open;
	public double High { get; set; } = High;
	public double Low { get; set; } = Low;
	public double Close { get; set; } = Close;
	public double Volume { get; set; } = Volume;
}