using System.Text.Json;

namespace DataParser.Infrastructure.Abstractions.Validators;

public interface IInternalJsonParserValidator
{
	public void ValidateRoot(JsonElement root);
	public void ValidateRecord(JsonElement element);
}
