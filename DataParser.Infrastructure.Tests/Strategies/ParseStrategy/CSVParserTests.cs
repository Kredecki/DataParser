using DataParser.Infrastructure.Abstractions.Converters;
using DataParser.Infrastructure.Converters;
using DataParser.Infrastructure.Strategies.ParseStrategy;
using DataParser.Shared.Enums;
using Moq;

namespace DataParser.Infrastructure.Tests.Strategies.ParseStrategy;

public class CSVParserTests
{
	private static CSVParser CreateSutWithRealConverter() => new(new CSVParserConverter());

	[Fact]
	public void Type_ReturnsCSV()
	{
		var sut = CreateSutWithRealConverter();

		Assert.Equal(ParserType.CSV, sut.Type);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("\n\n")]
	public void Parse_NullOrWhitespaceContent_ReturnsEmpty(string? content)
	{
		var sut = CreateSutWithRealConverter();

		var result = sut.Parse(content!).ToList();

		Assert.Empty(result);
	}

	[Fact]
	public void Parse_HeaderOnly_ReturnsNoRecords()
	{
		var sut = CreateSutWithRealConverter();
		const string content = "col1,col2,col3";

		var result = sut.Parse(content).ToList();

		Assert.Empty(result);
	}

	[Fact]
	public void Parse_SimpleCsv_ReturnsExpectedRows()
	{
		var sut = CreateSutWithRealConverter();
		const string content = "name,age\nJohn,30\nJane,25";

		var result = sut.Parse(content).ToList();

		Assert.Equal(2, result.Count);

		Assert.Equal("John", result[0]["name"]);
		Assert.Equal(30, result[0]["age"]);

		Assert.Equal("Jane", result[1]["name"]);
		Assert.Equal(25, result[1]["age"]);
	}

	[Fact]
	public void Parse_CrLfLineEndings_AreHandledCorrectly()
	{
		var sut = CreateSutWithRealConverter();
		const string content = "a,b\r\n1,2\r\n3,4";

		var result = sut.Parse(content).ToList();

		Assert.Equal(2, result.Count);
		Assert.Equal(1, result[0]["a"]);
		Assert.Equal(2, result[0]["b"]);
		Assert.Equal(3, result[1]["a"]);
		Assert.Equal(4, result[1]["b"]);
	}

	[Fact]
	public void Parse_LfOnlyLineEndings_AreHandledCorrectly()
	{
		var sut = CreateSutWithRealConverter();
		const string content = "a,b\n1,2\n3,4";

		var result = sut.Parse(content).ToList();

		Assert.Equal(2, result.Count);
		Assert.Equal(1, result[0]["a"]);
		Assert.Equal(3, result[1]["a"]);
	}

	[Fact]
	public void Parse_MixedLineEndings_AreHandledCorrectly()
	{
		var sut = CreateSutWithRealConverter();
		const string content = "a,b\r\n1,2\n3,4";

		var result = sut.Parse(content).ToList();

		Assert.Equal(2, result.Count);
		Assert.Equal(1, result[0]["a"]);
		Assert.Equal(3, result[1]["a"]);
	}

	[Fact]
	public void Parse_QuotedFieldContainingDelimiter_IsKeptAsSingleField()
	{
		var sut = CreateSutWithRealConverter();
		const string content = "name,description\nJohn,\"Doe, Jr.\"";

		var result = sut.Parse(content).ToList();

		Assert.Single(result);
		Assert.Equal("Doe, Jr.", result[0]["description"]);
	}

	[Fact]
	public void Parse_QuotedFieldContainingNewline_IsKeptAsSingleField()
	{
		var sut = CreateSutWithRealConverter();
		const string content = "name,note\nJohn,\"line1\nline2\"";

		var result = sut.Parse(content).ToList();

		Assert.Single(result);
		Assert.Equal("line1\nline2", result[0]["note"]);
	}

	[Fact]
	public void Parse_EscapedQuotesInsideQuotedField_AreUnescaped()
	{
		var sut = CreateSutWithRealConverter();
		const string content = "name,quote\nJohn,\"She said \"\"hello\"\"\"";

		var result = sut.Parse(content).ToList();

		Assert.Single(result);
		Assert.Equal("She said \"hello\"", result[0]["quote"]);
	}

	[Fact]
	public void Parse_RowWithFewerFieldsThanHeaders_MissingFieldsAreNull()
	{
		var sut = CreateSutWithRealConverter();
		const string content = "a,b,c\n1,2";

		var result = sut.Parse(content).ToList();

		Assert.Single(result);
		Assert.Equal(1, result[0]["a"]);
		Assert.Equal(2, result[0]["b"]);
		Assert.Null(result[0]["c"]);
	}

	[Fact]
	public void Parse_RowWithMoreFieldsThanHeaders_ExtraFieldsAreIgnored()
	{
		var sut = CreateSutWithRealConverter();
		const string content = "a,b\n1,2,3,4";

		var result = sut.Parse(content).ToList();

		Assert.Single(result);
		Assert.Equal(2, result[0].Count);
		Assert.Equal(1, result[0]["a"]);
		Assert.Equal(2, result[0]["b"]);
	}

	[Fact]
	public void Parse_TrailingBlankLine_IsSkipped()
	{
		var sut = CreateSutWithRealConverter();
		const string content = "a,b\n1,2\n";

		var result = sut.Parse(content).ToList();

		Assert.Single(result);
	}

	[Fact]
	public void Parse_EmptyValues_AreConvertedToNull()
	{
		var sut = CreateSutWithRealConverter();
		const string content = "a,b\n,2";

		var result = sut.Parse(content).ToList();

		Assert.Single(result);
		Assert.Null(result[0]["a"]);
		Assert.Equal(2, result[0]["b"]);
	}

	[Fact]
	public void Parse_DelegatesValueConversion_ToInjectedConverter()
	{
		// Arrange
		var converterMock = new Mock<ICSVParserConverter>();
		converterMock.Setup(c => c.ConvertValue(It.IsAny<string?>())).Returns<string?>(s => $"converted:{s}");

		var sut = new CSVParser(converterMock.Object);
		const string content = "a,b\nraw1,raw2";

		// Act
		var result = sut.Parse(content).ToList();

		// Assert
		Assert.Single(result);
		Assert.Equal("converted:raw1", result[0]["a"]);
		Assert.Equal("converted:raw2", result[0]["b"]);
		converterMock.Verify(c => c.ConvertValue("raw1"), Times.Once);
		converterMock.Verify(c => c.ConvertValue("raw2"), Times.Once);
	}

	[Fact]
	public void Parse_DuplicateHeaders_LastValueWins()
	{
		var sut = CreateSutWithRealConverter();
		const string content = "a,a\n1,2";

		var result = sut.Parse(content).ToList();

		Assert.Single(result);
		Assert.Single(result[0]);
		Assert.Equal(2, result[0]["a"]);
	}

	[Fact]
	public void Parse_IsLazilyEvaluated_UntilEnumerated()
	{
		var converterMock = new Mock<ICSVParserConverter>();
		converterMock.Setup(c => c.ConvertValue(It.IsAny<string?>())).Returns<string?>(s => s);

		var sut = new CSVParser(converterMock.Object);
		const string content = "a\n1\n2\n3";

		var enumerable = sut.Parse(content);

		converterMock.Verify(c => c.ConvertValue(It.IsAny<string?>()), Times.Never);

		var _ = enumerable.First();

		converterMock.Verify(c => c.ConvertValue(It.IsAny<string?>()), Times.Once);
	}
}
