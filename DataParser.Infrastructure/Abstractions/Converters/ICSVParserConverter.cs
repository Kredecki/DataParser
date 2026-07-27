namespace DataParser.Infrastructure.Abstractions.Converters;

public interface ICSVParserConverter
{
	public object? ConvertValue(string? raw);
}
