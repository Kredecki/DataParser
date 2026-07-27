using DataParser.Infrastructure.Abstractions.Converters;
using DataParser.Infrastructure.Abstractions.Validators;
using DataParser.Infrastructure.Exceptions;
using DataParser.Infrastructure.Strategies.ParseStrategy;
using DataParser.Shared.Enums;
using Moq;
using System.Text.Json;

namespace DataParser.Infrastructure.Tests.Strategies.ParseStrategy;

public class InternalJsonParserTests
{
	private readonly Mock<IInternalJsonParserValidator> _validatorMock = new();
	private readonly Mock<IInternalJsonParserConverter> _converterMock = new();

	private InternalJsonParser CreateSut() => new(_validatorMock.Object, _converterMock.Object);

	[Fact]
	public void Type_ReturnsInternalJson()
	{
		var sut = CreateSut();

		Assert.Equal(ParserType.INTERNAL_JSON, sut.Type);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Parse_NullOrWhitespaceContent_ReturnsEmpty_AndDoesNotValidate(string? content)
	{
		var sut = CreateSut();

		var result = sut.Parse(content!);

		Assert.Empty(result);
		_validatorMock.Verify(v => v.ValidateRoot(It.IsAny<JsonElement>()), Times.Never);
	}

	[Fact]
	public void Parse_InvalidJson_ThrowsParsingException()
	{
		var sut = CreateSut();
		const string content = "{ invalid json ";

		var ex = Assert.Throws<ParsingException>(() => sut.Parse(content));
		Assert.Contains("Nieprawidłowy format JSON", ex.Message);
	}

	[Fact]
	public void Parse_ValidArrayOfObjects_CallsValidatorAndConverterForEachRecord_ReturnsConvertedData()
	{
		// Arrange
		const string content = "[{\"a\":1},{\"a\":2}]";

		_validatorMock.Setup(v => v.ValidateRoot(It.IsAny<JsonElement>()));
		_validatorMock.Setup(v => v.ValidateRecord(It.IsAny<JsonElement>()));

		var callIndex = 0;
		_converterMock
			.Setup(c => c.ConvertObject(It.IsAny<JsonElement>()))
			.Returns(() =>
			{
				callIndex++;
				return new Dictionary<string, object?> { ["a"] = callIndex };
			});

		var sut = CreateSut();

		// Act
		var result = sut.Parse(content).ToList();

		// Assert
		Assert.Equal(2, result.Count);
		Assert.Equal(1, result[0]["a"]);
		Assert.Equal(2, result[1]["a"]);

		_validatorMock.Verify(v => v.ValidateRoot(It.IsAny<JsonElement>()), Times.Once);
		_validatorMock.Verify(v => v.ValidateRecord(It.IsAny<JsonElement>()), Times.Exactly(2));
		_converterMock.Verify(c => c.ConvertObject(It.IsAny<JsonElement>()), Times.Exactly(2));
	}

	[Fact]
	public void Parse_EmptyJsonArray_ReturnsEmptyCollection_ButStillValidatesRoot()
	{
		const string content = "[]";

		_validatorMock.Setup(v => v.ValidateRoot(It.IsAny<JsonElement>()));

		var sut = CreateSut();

		var result = sut.Parse(content);

		Assert.Empty(result);
		_validatorMock.Verify(v => v.ValidateRoot(It.IsAny<JsonElement>()), Times.Once);
		_validatorMock.Verify(v => v.ValidateRecord(It.IsAny<JsonElement>()), Times.Never);
		_converterMock.Verify(c => c.ConvertObject(It.IsAny<JsonElement>()), Times.Never);
	}

	[Fact]
	public void Parse_RootIsNotArray_PropagatesExceptionFromValidator()
	{
		const string content = "{\"a\":1}";

		_validatorMock
			.Setup(v => v.ValidateRoot(It.IsAny<JsonElement>()))
			.Throws(new ParsingException("Oczekiwano tablicy jako elementu głównego dokumentu JSON, otrzymano: Object."));

		var sut = CreateSut();

		var ex = Assert.Throws<ParsingException>(() => sut.Parse(content));
		Assert.Contains("tablicy", ex.Message);
		_converterMock.Verify(c => c.ConvertObject(It.IsAny<JsonElement>()), Times.Never);
	}

	[Fact]
	public void Parse_RecordIsNotObject_PropagatesExceptionFromValidator_AndStopsProcessing()
	{
		const string content = "[{\"a\":1}, \"not-an-object\", {\"a\":2}]";

		_validatorMock.Setup(v => v.ValidateRoot(It.IsAny<JsonElement>()));
		_validatorMock
			.SetupSequence(v => v.ValidateRecord(It.IsAny<JsonElement>()))
			.Pass()
			.Throws(new ParsingException("Oczekiwano obiektu JSON jako rekordu, otrzymano: String."));

		_converterMock
			.Setup(c => c.ConvertObject(It.IsAny<JsonElement>()))
			.Returns(new Dictionary<string, object?> { ["a"] = 1 });

		var sut = CreateSut();

		var ex = Assert.Throws<ParsingException>(() => sut.Parse(content));
		Assert.Contains("obiektu JSON", ex.Message);

		_converterMock.Verify(c => c.ConvertObject(It.IsAny<JsonElement>()), Times.Once);
	}
}
