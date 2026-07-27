namespace DataParser.Shared.DTOs.Parser;

public class DataParserResponseDto
{
	public string Status { get; set; } = string.Empty;

	public int Count { get; set; }

	public IEnumerable<object> Data { get; set; } = [];
}
