using System.ComponentModel.DataAnnotations;

namespace TpvVyber.Client.Classes;

public class LoginModel
{
    [Required(ErrorMessage = "Username is Required")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Input Invalid")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is Required")]
    public string Password { get; set; } = string.Empty;
}
