using DataParser.Infrastructure.Abstractions.Validators;
using DataParser.Infrastructure.Exceptions;
using System.Text.Json;

namespace DataParser.Infrastructure.Validators;

public class InternalJsonParserValidator : IInternalJsonParserValidator
{
	public void ValidateRoot(JsonElement root)
	{
		if (root.ValueKind != JsonValueKind.Array)
		{
			throw new ParsingException(
				$"Oczekiwano tablicy jako elementu głównego dokumentu JSON, otrzymano: {root.ValueKind}.");
		}
	}

	public void ValidateRecord(JsonElement element)
	{
		if (element.ValueKind != JsonValueKind.Object)
		{
			throw new ParsingException(
				$"Oczekiwano obiektu JSON jako rekordu, otrzymano: {element.ValueKind}.");
		}
	}
}
