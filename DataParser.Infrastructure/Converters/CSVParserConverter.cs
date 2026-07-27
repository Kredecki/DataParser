using DataParser.Infrastructure.Abstractions.Converters;
using System.Globalization;

namespace DataParser.Infrastructure.Converters;

public class CSVParserConverter : ICSVParserConverter
{
	public object? ConvertValue(string? raw)
	{
		if (string.IsNullOrEmpty(raw))
			return null;

		var trimmed = raw.Trim();

		if (bool.TryParse(trimmed, out var boolValue))
			return boolValue;

		if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
			return intValue;

		if (double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
			return doubleValue;

		return trimmed;
	}
}
