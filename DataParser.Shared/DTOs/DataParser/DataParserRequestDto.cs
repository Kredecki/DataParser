using System.Text.Json.Serialization;
using DataParser.Shared.Enums;

namespace DataParser.Shared.DTOs.DataParser;

public class DataParserRequestDto
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public ParserType Type { get; set; }
	public string Content { get; set; } = string.Empty;
}
