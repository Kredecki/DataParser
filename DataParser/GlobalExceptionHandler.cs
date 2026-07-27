using DataParser.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DataParser.API;

public class GlobalExceptionHandler(IProblemDetailsService _pds) : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(
		HttpContext context,
		Exception ex,
		CancellationToken cancellationToken)
	{
		var (status, title, detail) = ex switch
		{
			FormatException => (
				StatusCodes.Status400BadRequest,
				"Invalid Base64 content",
				"Pole 'content' nie zawiera poprawnie zakodowanego ciągu Base64."),

			ParsingException => (
				StatusCodes.Status400BadRequest,
				"Parsing failed",
				ex.Message),

			NotSupportedException => (
				StatusCodes.Status400BadRequest,
				"Unsupported type",
				ex.Message),

			_ => (
				StatusCodes.Status500InternalServerError,
				"Server error",
				"Wystąpił nieoczekiwany błąd serwera.")
		};

		var problem = new ProblemDetails
		{
			Status = status,
			Title = title,
			Detail = ex.Message,
			Type = $"https://httpstatuses.com/{status}",
			Instance = context.Request.Path
		};

		problem.Extensions["traceId"] = context.TraceIdentifier;

		context.Response.StatusCode = status;
		return await _pds.TryWriteAsync(new ProblemDetailsContext
		{
			HttpContext = context,
			ProblemDetails = problem,
			Exception = ex
		});
	}
}
