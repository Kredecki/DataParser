using DataParser.Infrastructure.Abstractions.Converters;
using DataParser.Infrastructure.Exceptions;
using System.Text.Json;

namespace DataParser.Infrastructure.Converters;

public class InternalJsonParserConverter : IInternalJsonParserConverter
{
	public Dictionary<string, object?> ConvertObject(JsonElement element)
	{
		var dict = new Dictionary<string, object?>();

		foreach (var property in element.EnumerateObject())
			dict[property.Name] = ConvertValue(property.Value);

		return dict;
	}

	public object? ConvertValue(JsonElement value)
	{
		switch (value.ValueKind)
		{
			case JsonValueKind.String:
				return value.GetString();

			case JsonValueKind.Number:
				if (value.TryGetInt64(out var longValue))
					return longValue;
				return value.GetDouble();

			case JsonValueKind.True:
				return true;

			case JsonValueKind.False:
				return false;

			case JsonValueKind.Null:
			case JsonValueKind.Undefined:
				return null;

			case JsonValueKind.Array:
				return value.EnumerateArray()
					.Select(ConvertValue)
					.ToList();

			case JsonValueKind.Object:
				return ConvertObject(value);

			default:
				throw new ParsingException($"Nieobsługiwany typ wartości JSON: {value.ValueKind}.");
		}
	}
}
