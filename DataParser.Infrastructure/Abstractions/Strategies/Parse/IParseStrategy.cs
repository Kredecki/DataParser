using DataParser.Shared.Enums;

namespace DataParser.Infrastructure.Abstractions.Strategies.Parse;

public interface IParseStrategy
{
	ParserType Type { get; }
	IEnumerable<Dictionary<string, object?>> Parse(string content);
}
