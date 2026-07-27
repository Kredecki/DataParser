using DataParser.Infrastructure.Exceptions;
using DataParser.Infrastructure.Validators;
using System.Text.Json;

namespace DataParser.Infrastructure.Tests.Validators;

public class InternalJsonParserValidatorTests
{
	private readonly InternalJsonParserValidator _sut = new();

	private static JsonElement ParseElement(string json)
	{
		using var document = JsonDocument.Parse(json);
		return document.RootElement.Clone();
	}

	[Fact]
	public void ValidateRoot_ArrayElement_DoesNotThrow()
	{
		var element = ParseElement("[]");

		var exception = Record.Exception(() => _sut.ValidateRoot(element));

		Assert.Null(exception);
	}

	[Theory]
	[InlineData("{}")]
	[InlineData("\"text\"")]
	[InlineData("42")]
	[InlineData("true")]
	[InlineData("null")]
	public void ValidateRoot_NonArrayElement_ThrowsParsingException(string json)
	{
		var element = ParseElement(json);

		var ex = Assert.Throws<ParsingException>(() => _sut.ValidateRoot(element));
		Assert.Contains("tablicy", ex.Message);
	}

	[Fact]
	public void ValidateRecord_ObjectElement_DoesNotThrow()
	{
		var element = ParseElement("{\"a\":1}");

		var exception = Record.Exception(() => _sut.ValidateRecord(element));

		Assert.Null(exception);
	}

	[Theory]
	[InlineData("[]")]
	[InlineData("\"text\"")]
	[InlineData("42")]
	[InlineData("true")]
	[InlineData("null")]
	public void ValidateRecord_NonObjectElement_ThrowsParsingException(string json)
	{
		var element = ParseElement(json);

		var ex = Assert.Throws<ParsingException>(() => _sut.ValidateRecord(element));
		Assert.Contains("obiektu JSON", ex.Message);
	}

	[Fact]
	public void ValidateRoot_ExceptionMessage_ContainsActualValueKind()
	{
		var element = ParseElement("42");

		var ex = Assert.Throws<ParsingException>(() => _sut.ValidateRoot(element));

		Assert.Contains(JsonValueKind.Number.ToString(), ex.Message);
	}

	[Fact]
	public void ValidateRecord_ExceptionMessage_ContainsActualValueKind()
	{
		var element = ParseElement("\"text\"");

		var ex = Assert.Throws<ParsingException>(() => _sut.ValidateRecord(element));

		Assert.Contains(JsonValueKind.String.ToString(), ex.Message);
	}
}
