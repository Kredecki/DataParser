using System.Text.Json;

namespace DataParser.Infrastructure.Abstractions.Converters;

public interface IInternalJsonParserConverter
{
	public Dictionary<string, object?> ConvertObject(JsonElement element);
	public object? ConvertValue(JsonElement value);
}
