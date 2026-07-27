using DataParser.Infrastructure.Abstractions.Strategies.Parse;
using DataParser.Infrastructure.Strategies.ParseStrategy;
using DataParser.Shared.Enums;
using Moq;

namespace DataParser.Infrastructure.Tests.Strategies.ParseStrategy;

public class ParseStrategyResolverTests
{
	[Fact]
	public void Resolve_MatchingStrategyExists_ReturnsIt()
	{
		// Arrange
		var csvStrategy = new Mock<IParseStrategy>();
		csvStrategy.Setup(s => s.Type).Returns(ParserType.CSV);

		var jsonStrategy = new Mock<IParseStrategy>();
		jsonStrategy.Setup(s => s.Type).Returns(ParserType.INTERNAL_JSON);

		var sut = new ParseStrategyResolver(new[] { csvStrategy.Object, jsonStrategy.Object });

		// Act
		var result = sut.Resolve(ParserType.INTERNAL_JSON);

		// Assert
		Assert.Same(jsonStrategy.Object, result);
	}

	[Fact]
	public void Resolve_NoMatchingStrategy_ThrowsNotSupportedException()
	{
		// Arrange
		var csvStrategy = new Mock<IParseStrategy>();
		csvStrategy.Setup(s => s.Type).Returns(ParserType.CSV);

		var sut = new ParseStrategyResolver(new[] { csvStrategy.Object });

		// Act & Assert
		var ex = Assert.Throws<NotSupportedException>(() => sut.Resolve(ParserType.INTERNAL_JSON));
		Assert.Contains(ParserType.INTERNAL_JSON.ToString(), ex.Message);
	}

	[Fact]
	public void Resolve_EmptyStrategyCollection_ThrowsNotSupportedException()
	{
		var sut = new ParseStrategyResolver(Enumerable.Empty<IParseStrategy>());

		Assert.Throws<NotSupportedException>(() => sut.Resolve(ParserType.CSV));
	}

	[Fact]
	public void Resolve_MultipleStrategiesWithSameType_ReturnsFirstMatch()
	{
		// Arrange
		var first = new Mock<IParseStrategy>();
		first.Setup(s => s.Type).Returns(ParserType.CSV);

		var second = new Mock<IParseStrategy>();
		second.Setup(s => s.Type).Returns(ParserType.CSV);

		var sut = new ParseStrategyResolver(new[] { first.Object, second.Object });

		// Act
		var result = sut.Resolve(ParserType.CSV);

		// Assert
		Assert.Same(first.Object, result);
	}
}
