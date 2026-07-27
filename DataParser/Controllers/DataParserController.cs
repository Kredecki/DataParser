using DataParser.Infrastructure.Commands.Authorization;
using DataParser.Infrastructure.Commands.DataParser;
using DataParser.Shared.DTOs.DataParser;
using DataParser.Shared.DTOs.Parser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataParser.API.Controllers;

[ApiController]
[Route("api/v1")]
public class DataParserController(IMediator sender) : Controller
{
	[Authorize]
	[HttpPost("parse-content")]
	[Consumes("application/json")]
	[Produces("application/json")]
	[RequestSizeLimit(10 * 1024 * 1024)]
	[ProducesResponseType(200, Type = typeof(DataParserRequestDto))]
	[ProducesResponseType(400, Type = typeof(ProblemDetails))]
	public async Task<IActionResult> ParseContent(
		[FromBody] DataParserRequestDto request, 
		CancellationToken cancellationToken = default)
	{
		// Verify that the parser type is supported
		if (!Enum.IsDefined(request.Type))
		{
			return BadRequest(new ProblemDetails
			{
				Title = "Unsupported type",
				Detail = $"Parser type '{request.Type}' is not supported."
			});
		}

		DataParserResponseDto response = await sender.Send(new ParseContentCommand(request), cancellationToken);

		if (response is not null)
			return Ok(response);

		return BadRequest();
	}
}
