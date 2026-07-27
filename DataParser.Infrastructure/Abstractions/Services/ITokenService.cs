using DataParser.Shared.Models;

namespace DataParser.Infrastructure.Abstractions.Services;

public interface ITokenService
{
	public string GenerateToken(User user);
}
