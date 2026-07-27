using DataParser.Infrastructure.Abstractions.Strategies.Parse;
using DataParser.Shared.DTOs.DataParser;
using DataParser.Shared.DTOs.Parser;
using System.Text;
using MediatR;

namespace DataParser.Infrastructure.Commands.DataParser;

public sealed record ParseContentCommand(DataParserRequestDto Request) : IRequest<DataParserResponseDto>;

public class ParseContentCommandHandler(IParseStrategyResolver resolver) : IRequestHandler<ParseContentCommand, DataParserResponseDto>
{
	public async Task<DataParserResponseDto> Handle(ParseContentCommand command, CancellationToken cancellationToken)
	{
		// Decode the base64 content to a string
		byte[] bytes = Convert.FromBase64String(command.Request.Content);
		string content = Encoding.UTF8.GetString(bytes);

		// Use the strategy resolver to get the appropriate parsing strategy based on the request type
		IParseStrategy strategy = resolver.Resolve(command.Request.Type);
		IEnumerable<Dictionary<string, object?>> data = strategy.Parse(content);

		List<Dictionary<string, object?>> dataList = data.ToList();

		// Return the parsed data in a response DTO
		return new DataParserResponseDto
		{
			Status = "Success",
			Count = dataList.Count,
			Data = dataList
		};
	}
}