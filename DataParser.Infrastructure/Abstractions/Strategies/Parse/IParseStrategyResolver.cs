using DataParser.Shared.Enums;

namespace DataParser.Infrastructure.Abstractions.Strategies.Parse;

public interface IParseStrategyResolver
{
	IParseStrategy Resolve(ParserType type);
}
