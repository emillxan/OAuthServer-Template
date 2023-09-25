using System.ComponentModel.DataAnnotations;

namespace OAuthServer.ViewModels;

public class ExternalLoginViewModel
{
    [Required]
    public string UserName { get; set; }

    [Required]
    public string ReturnUrl { get; set; }
}