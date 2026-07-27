using DataParser.Infrastructure.Abstractions.Strategies.Parse;
using DataParser.Infrastructure.Commands.DataParser;
using DataParser.Shared.DTOs.DataParser;
using DataParser.Shared.Enums;
using Moq;
using System.Text;
using System.Timers;

namespace DataParser.Infrastructure.Tests.Commands.DataParser;

public class ParseContentCommandHandlerTests
{
	private readonly Mock<IParseStrategyResolver> _resolverMock = new();
	private readonly Mock<IParseStrategy> _strategyMock = new();

	private ParseContentCommandHandler CreateSut() => new(_resolverMock.Object);

	private static string ToBase64(string content) => Convert.ToBase64String(Encoding.UTF8.GetBytes(content));

	[Fact]
	public async Task Handle_DecodesBase64AndParsesWithResolvedStrategy_ReturnsSuccessResponse()
	{
		// Arrange
		const string rawContent = "col1,col2\nval1,val2";
		var request = new DataParserRequestDto
		{
			Content = ToBase64(rawContent),
			Type = ParserType.CSV
		};
		var command = new ParseContentCommand(request);

		var parsedData = new List<Dictionary<string, object?>>
		{
			new() { ["col1"] = "val1", ["col2"] = "val2" }
		};

		_resolverMock.Setup(r => r.Resolve(ParserType.CSV)).Returns(_strategyMock.Object);
		_strategyMock.Setup(s => s.Parse(rawContent)).Returns(parsedData);

		var sut = CreateSut();

		// Act
		var result = await sut.Handle(command, CancellationToken.None);

		// Assert
		Assert.Equal("Success", result.Status);
		Assert.Equal(1, result.Count);
		Assert.Equal(parsedData, result.Data);
		_resolverMock.Verify(r => r.Resolve(ParserType.CSV), Times.Once);
		_strategyMock.Verify(s => s.Parse(rawContent), Times.Once);
	}

	[Fact]
	public async Task Handle_EmptyParsedData_ReturnsCountZero()
	{
		// Arrange
		var request = new DataParserRequestDto
		{
			Content = ToBase64("header\n"),
			Type = ParserType.CSV
		};
		var command = new ParseContentCommand(request);

		_resolverMock.Setup(r => r.Resolve(It.IsAny<ParserType>())).Returns(_strategyMock.Object);
		_strategyMock.Setup(s => s.Parse(It.IsAny<string>())).Returns(Enumerable.Empty<Dictionary<string, object?>>());

		var sut = CreateSut();

		// Act
		var result = await sut.Handle(command, CancellationToken.None);

		// Assert
		Assert.Equal("Success", result.Status);
		Assert.Equal(0, result.Count);
		Assert.Empty(result.Data);
	}

	[Fact]
	public async Task Handle_InvalidBase64Content_ThrowsFormatException()
	{
		// Arrange
		var request = new DataParserRequestDto
		{
			Content = "not-valid-base64!!!",
			Type = ParserType.CSV
		};
		var command = new ParseContentCommand(request);
		var sut = CreateSut();

		// Act & Assert
		await Assert.ThrowsAsync<FormatException>(() => sut.Handle(command, CancellationToken.None));
		_resolverMock.Verify(r => r.Resolve(It.IsAny<ParserType>()), Times.Never);
	}

	[Fact]
	public async Task Handle_UnsupportedParserType_PropagatesExceptionFromResolver()
	{
		// Arrange
		var request = new DataParserRequestDto
		{
			Content = ToBase64("irrelevant"),
			Type = (ParserType)999
		};
		var command = new ParseContentCommand(request);

		_resolverMock.Setup(r => r.Resolve((ParserType)999))
			.Throws(new NotSupportedException("Parser 999 is not supported"));

		var sut = CreateSut();

		// Act & Assert
		var ex = await Assert.ThrowsAsync<NotSupportedException>(() => sut.Handle(command, CancellationToken.None));
		Assert.Contains("999", ex.Message);
	}

	[Fact]
	public async Task Handle_DecodesUtf8ContentCorrectly_BeforePassingToStrategy()
	{
		// Arrange - content containing non-ASCII characters (Polish diacritics)
		const string rawContent = "imię,wiek\nZażółć,30";
		var request = new DataParserRequestDto
		{
			Content = ToBase64(rawContent),
			Type = ParserType.CSV
		};
		var command = new ParseContentCommand(request);

		string? capturedContent = null;
		_resolverMock.Setup(r => r.Resolve(It.IsAny<ParserType>())).Returns(_strategyMock.Object);
		_strategyMock
			.Setup(s => s.Parse(It.IsAny<string>()))
			.Callback<string>(c => capturedContent = c)
			.Returns(Enumerable.Empty<Dictionary<string, object?>>());

		var sut = CreateSut();

		// Act
		await sut.Handle(command, CancellationToken.None);

		// Assert
		Assert.Equal(rawContent, capturedContent);
	}
}
