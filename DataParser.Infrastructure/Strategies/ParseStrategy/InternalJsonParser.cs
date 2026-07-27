using DataParser.Infrastructure.Abstractions.Converters;
using DataParser.Infrastructure.Abstractions.Strategies.Parse;
using DataParser.Infrastructure.Abstractions.Validators;
using DataParser.Infrastructure.Exceptions;
using DataParser.Shared.Enums;
using System.Text.Json;

namespace DataParser.Infrastructure.Strategies.ParseStrategy;

public class InternalJsonParser(IInternalJsonParserValidator validator, IInternalJsonParserConverter converter) : IParseStrategy
{
	public ParserType Type => ParserType.INTERNAL_JSON;

	public IEnumerable<Dictionary<string, object?>> Parse(string content)
	{
		if (string.IsNullOrWhiteSpace(content))
			return [];

		JsonDocument document;

		try
		{
			document = JsonDocument.Parse(content);
		}
		catch (JsonException ex)
		{
			throw new ParsingException($"Nieprawidłowy format JSON: {ex.Message}", ex);
		}

		using (document)
		{
			var root = document.RootElement;

			validator.ValidateRoot(root);

			var result = new List<Dictionary<string, object?>>(root.GetArrayLength());

			foreach (var element in root.EnumerateArray())
			{
				validator.ValidateRecord(element);
				result.Add(converter.ConvertObject(element));
			}

			return result;
		}
	}
}
