using System.ComponentModel.DataAnnotations;

namespace IdentityServerHost.Quickstart.UI;

public class LoginInputModel
{
    [Required]
    public required string Username { get; set; }
    [Required]
    public required string Password { get; set; }
    public bool RememberLogin { get; set; }
    public string? ReturnUrl { get; set; }
}
