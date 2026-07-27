using System.ComponentModel.DataAnnotations;

namespace DataParser.Shared.DTOs.Authorization;

public class SignInRequestDto
{
	[Required(ErrorMessage = "Pole login jest wymagane.")]
	public string Login { get; set; } = string.Empty;

	[Required(ErrorMessage = "Pole hasło jest wymagane.")]
	public string Password { get; set; } = string.Empty;
}
