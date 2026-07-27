using DataParser.Infrastructure.Converters;

namespace DataParser.Infrastructure.Tests.Converters;

public class CSVParserConverterTests
{
	private readonly CSVParserConverter _sut = new();

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	public void ConvertValue_NullOrEmpty_ReturnsNull(string? raw)
	{
		var result = _sut.ConvertValue(raw);

		Assert.Null(result);
	}

	[Theory]
	[InlineData("true", true)]
	[InlineData("false", false)]
	[InlineData("True", true)]
	[InlineData("FALSE", false)]
	public void ConvertValue_BooleanLikeStrings_ReturnsBool(string raw, bool expected)
	{
		var result = _sut.ConvertValue(raw);

		Assert.IsType<bool>(result);
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData("0", 0)]
	[InlineData("42", 42)]
	[InlineData("-17", -17)]
	public void ConvertValue_IntegerStrings_ReturnsInt(string raw, int expected)
	{
		var result = _sut.ConvertValue(raw);

		Assert.IsType<int>(result);
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData("3.14", 3.14)]
	[InlineData("-0.5", -0.5)]
	[InlineData("1e3", 1000d)]
	public void ConvertValue_FloatingPointStrings_ReturnsDouble(string raw, double expected)
	{
		var result = _sut.ConvertValue(raw);

		Assert.IsType<double>(result);
		Assert.Equal(expected, (double)result!, precision: 6);
	}

	[Theory]
	[InlineData("hello")]
	[InlineData("hello world")]
	[InlineData("12a")]
	[InlineData("2024-01-01")]
	public void ConvertValue_NonNumericNonBooleanStrings_ReturnsTrimmedString(string raw)
	{
		var result = _sut.ConvertValue(raw);

		Assert.IsType<string>(result);
		Assert.Equal(raw.Trim(), result);
	}

	[Fact]
	public void ConvertValue_ValueWithLeadingAndTrailingWhitespace_IsTrimmedBeforeConversion()
	{
		var result = _sut.ConvertValue("   42   ");

		Assert.IsType<int>(result);
		Assert.Equal(42, result);
	}

	[Fact]
	public void ConvertValue_WhitespaceOnly_ReturnsTrimmedEmptyString()
	{
		var result = _sut.ConvertValue("   ");

		Assert.IsType<string>(result);
		Assert.Equal(string.Empty, result);
	}

	[Fact]
	public void ConvertValue_CommaAsDecimalSeparator_IsNotTreatedAsFloat_ReturnsString()
	{
		var result = _sut.ConvertValue("3,14");

		Assert.IsType<string>(result);
		Assert.Equal("3,14", result);
	}
}
