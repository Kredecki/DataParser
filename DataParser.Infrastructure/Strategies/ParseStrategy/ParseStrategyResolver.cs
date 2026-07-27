using DataParser.Infrastructure.Abstractions.Strategies.Parse;
using DataParser.Shared.Enums;

namespace DataParser.Infrastructure.Strategies.ParseStrategy;

public class ParseStrategyResolver(IEnumerable<IParseStrategy> strategies) : IParseStrategyResolver
{
	public IParseStrategy Resolve(ParserType type)
	{
		var strategy = strategies.FirstOrDefault(x => x.Type == type);

		return strategy
			?? throw new NotSupportedException(
				$"Parser {type} is not supported");
	}
}
