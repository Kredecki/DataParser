using DataParser.Infrastructure.Abstractions.Services;
using DataParser.Shared.DTOs.Authorization;
using DataParser.Shared.Models;
using MediatR;

namespace DataParser.Infrastructure.Commands.Authorization;

public sealed record SignInCommand(SignInRequestDto Dto) : IRequest<SignInResponseDto>;

public class SignInQueryHandler(ITokenService tokenService) : IRequestHandler<SignInCommand, SignInResponseDto>
{
	public async Task<SignInResponseDto> Handle(SignInCommand command, CancellationToken cancellationToken)
	{
		// In a real application, you would validate the user's credentials against a database or other data source.

		User user = new()
		{
			Id = Guid.NewGuid(),
			Login = command.Dto.Login,
			PassHash = "d5369d3db39f63ed9cb16c6db1b42203b68afa0f92e03bc3d1a9e200b6819d02",
			Salt = "HBm43p3AUdJUZuCS"
		};

		return new SignInResponseDto
		{
			Login = command.Dto.Login,
			Token = tokenService.GenerateToken(user)
		};
	}
}