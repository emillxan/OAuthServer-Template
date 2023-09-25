using System.ComponentModel.DataAnnotations;

namespace OAuthServer.ViewModels;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }  
}