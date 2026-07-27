using DataParser.Infrastructure.Abstractions.Converters;
using DataParser.Infrastructure.Abstractions.Strategies.Parse;
using DataParser.Shared.Enums;
using System.Globalization;
using System.Text;

namespace DataParser.Infrastructure.Strategies.ParseStrategy;

public class CSVParser(ICSVParserConverter converter) : IParseStrategy
{
	public ParserType Type => ParserType.CSV;

	private const char Delimiter = ',';
	private const char Quote = '"';

	public IEnumerable<Dictionary<string, object?>> Parse(string content)
	{
		if (string.IsNullOrWhiteSpace(content))
			yield break;

		using var reader = new StringReader(content);

		var headers = ReadRecord(reader);
		if (headers is null)
			yield break;

		List<string> record;
		while ((record = ReadRecord(reader)) is not null)
		{
			if (record.Count == 1 && string.IsNullOrEmpty(record[0]))
				continue;

			var row = new Dictionary<string, object?>();

			for (var i = 0; i < headers.Count; i++)
			{
				var header = headers[i];
				var rawValue = i < record.Count ? record[i] : null;

				row[header] = converter.ConvertValue(rawValue);
			}

			yield return row;
		}
	}

	private static List<string>? ReadRecord(TextReader reader)
	{
		var fields = new List<string>();
		var field = new StringBuilder();
		var inQuotes = false;
		var hasContent = false;

		int current;
		while ((current = reader.Read()) != -1)
		{
			hasContent = true;
			var c = (char)current;

			if (inQuotes)
			{
				if (c == Quote)
				{
					var next = reader.Peek();
					if (next == Quote)
					{
						field.Append(Quote);
						reader.Read();
					}
					else
					{
						inQuotes = false;
					}
				}
				else
				{
					field.Append(c);
				}

				continue;
			}

			switch (c)
			{
				case Quote:
					inQuotes = true;
					break;

				case Delimiter:
					fields.Add(field.ToString());
					field.Clear();
					break;

				case '\r':
					if (reader.Peek() == '\n')
						reader.Read();

					fields.Add(field.ToString());
					return fields;

				case '\n':
					fields.Add(field.ToString());
					return fields;

				default:
					field.Append(c);
					break;
			}
		}

		if (!hasContent)
			return null;

		fields.Add(field.ToString());
		return fields;
	}
}
