using DataParser.Infrastructure.Converters;
using System.Text.Json;

namespace DataParser.Infrastructure.Tests.Converters;

public class InternalJsonParserConverterTests
{
	private readonly InternalJsonParserConverter _sut = new();

	private static JsonElement ParseElement(string json)
	{
		using var document = JsonDocument.Parse(json);
		return document.RootElement.Clone();
	}

	[Fact]
	public void ConvertValue_StringElement_ReturnsString()
	{
		var element = ParseElement("\"hello\"");

		var result = _sut.ConvertValue(element);

		Assert.Equal("hello", result);
	}

	[Fact]
	public void ConvertValue_IntegerNumber_ReturnsLong()
	{
		var element = ParseElement("42");

		var result = _sut.ConvertValue(element);

		Assert.IsType<long>(result);
		Assert.Equal(42L, result);
	}

	[Fact]
	public void ConvertValue_DecimalNumber_ReturnsDouble()
	{
		var element = ParseElement("3.14");

		var result = _sut.ConvertValue(element);

		Assert.IsType<double>(result);
		Assert.Equal(3.14, result);
	}

	[Fact]
	public void ConvertValue_VeryLargeIntegerExceedingLongRange_FallsBackToDouble()
	{
		// A number too large for Int64 should fall back to double via TryGetInt64 returning false.
		var element = ParseElement("99999999999999999999999999999999");

		var result = _sut.ConvertValue(element);

		Assert.IsType<double>(result);
	}

	[Fact]
	public void ConvertValue_True_ReturnsBoolTrue()
	{
		var element = ParseElement("true");

		var result = _sut.ConvertValue(element);

		Assert.Equal(true, result);
	}

	[Fact]
	public void ConvertValue_False_ReturnsBoolFalse()
	{
		var element = ParseElement("false");

		var result = _sut.ConvertValue(element);

		Assert.Equal(false, result);
	}

	[Fact]
	public void ConvertValue_Null_ReturnsNull()
	{
		var element = ParseElement("null");

		var result = _sut.ConvertValue(element);

		Assert.Null(result);
	}

	[Fact]
	public void ConvertValue_Array_ReturnsListOfConvertedValues()
	{
		var element = ParseElement("[1, \"two\", true, null]");

		var result = _sut.ConvertValue(element);

		var list = Assert.IsType<List<object?>>(result);
		Assert.Equal(4, list.Count);
		Assert.Equal(1L, list[0]);
		Assert.Equal("two", list[1]);
		Assert.Equal(true, list[2]);
		Assert.Null(list[3]);
	}

	[Fact]
	public void ConvertValue_NestedArray_ConvertsRecursively()
	{
		var element = ParseElement("[[1,2],[3,4]]");

		var result = _sut.ConvertValue(element);

		var outer = Assert.IsType<List<object?>>(result);
		Assert.Equal(2, outer.Count);
		var inner = Assert.IsType<List<object?>>(outer[0]);
		Assert.Equal(new object?[] { 1L, 2L }, inner);
	}

	[Fact]
	public void ConvertValue_Object_ReturnsDictionary()
	{
		var element = ParseElement("{\"x\":1,\"y\":\"z\"}");

		var result = _sut.ConvertValue(element);

		var dict = Assert.IsType<Dictionary<string, object?>>(result);
		Assert.Equal(1L, dict["x"]);
		Assert.Equal("z", dict["y"]);
	}

	[Fact]
	public void ConvertObject_FlatObject_ReturnsDictionaryWithAllProperties()
	{
		var element = ParseElement("{\"name\":\"John\",\"age\":30,\"active\":true}");

		var result = _sut.ConvertObject(element);

		Assert.Equal(3, result.Count);
		Assert.Equal("John", result["name"]);
		Assert.Equal(30L, result["age"]);
		Assert.Equal(true, result["active"]);
	}

	[Fact]
	public void ConvertObject_NestedObject_ConvertsRecursively()
	{
		var element = ParseElement("{\"address\":{\"city\":\"Warsaw\",\"zip\":\"00-001\"}}");

		var result = _sut.ConvertObject(element);

		var nested = Assert.IsType<Dictionary<string, object?>>(result["address"]);
		Assert.Equal("Warsaw", nested["city"]);
		Assert.Equal("00-001", nested["zip"]);
	}

	[Fact]
	public void ConvertObject_EmptyObject_ReturnsEmptyDictionary()
	{
		var element = ParseElement("{}");

		var result = _sut.ConvertObject(element);

		Assert.Empty(result);
	}

	[Fact]
	public void ConvertObject_ObjectWithArrayProperty_ConvertsArrayToList()
	{
		var element = ParseElement("{\"tags\":[\"a\",\"b\",\"c\"]}");

		var result = _sut.ConvertObject(element);

		var tags = Assert.IsType<List<object?>>(result["tags"]);
		Assert.Equal(new object?[] { "a", "b", "c" }, tags);
	}
}
