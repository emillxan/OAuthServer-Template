using IdentityModel;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OAuthServer.Entities;
using OAuthServer.Services.Email;
using OAuthServer.ViewModels;
using System.Security.Claims;

namespace OAuthServer.Controllers;

[Route("[controller]")]
public class AccountController : Controller
{
    #region CONSTRUCTOR

    private readonly IIdentityServerInteractionService _interactionService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IEmailSender _emailSender;

    public AccountController(IIdentityServerInteractionService interactionService,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IEmailSender emailSender)
        {
            _interactionService = interactionService;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

    #endregion

    #region [Login]

    /// <summary>
    /// Get Login View
    /// </summary>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> Login(string returnUrl)
    {
        try
        {
            if (returnUrl == null)
                return View("Error", new ErorViewModel { Discruption = "[Login Get Error] : ReturnUrl is null"});

            var externalProviders = await _signInManager.GetExternalAuthenticationSchemesAsync();
            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalProviders = externalProviders
            });
        }
        catch (Exception ex)
        {
            return View("Error", new ErorViewModel { Discruption = $"[Login Get Error] : {ex.Message}" });
        }
    }

    /// <summary>
    /// Post Login
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        try
        {
            /*if (!ModelState.IsValid)
            {
            return View(model);
            }*/
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                ModelState.AddModelError("UserName", "User not found");
                return View(model);
            }

            var signinResult = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (signinResult.Succeeded)
            {
                if(model.ReturnUrl != null)
                    return Redirect(model.ReturnUrl);
                return View();
            }
            ModelState.AddModelError("UserName", "Something not found");

            return View(model);
        }
        catch(Exception ex)
        {
            return View("Error", new ErorViewModel { Discruption = $"[Login Post Error] : {ex.Message}" });
        }
    }

    #endregion

    #region [Register]
    
    [Route("[action]")]
    public IActionResult Register(string returnUrl)
    {
        try
        {
            if (returnUrl == null)
                return View("Error", new ErorViewModel { Discruption = "[Register Get Error] : ReturnUrl is null" });
            return View(new RegisterViewModel
            {
                ReturnUrl = returnUrl
            });
        }
        catch (Exception ex)
        {
            return View("Error", new ErorViewModel { Discruption = $"[Register Get Error] : {ex.Message}" });
        }
    }
    
    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        try
        {
            var emailUser = await _userManager.FindByEmailAsync(model.Email);
            if (emailUser != null)
            {
                ModelState.AddModelError("UserName", "A user with this email already exists");
                return View();
            }
            var nameUser = await _userManager.FindByNameAsync(model.UserName);
            if (nameUser != null)
            {
                ModelState.AddModelError("UserName", "A user with the same name already exists");
                return View();
            }

            var createUser = await _userManager.CreateAsync(new IdentityUser
            {
                UserName = model.UserName,
                Email = model.Email

            }, model.Password);

            var newUser = await _userManager.FindByNameAsync(model.UserName);

            var mailToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { mailToken, userName = newUser.UserName }, Request.Scheme);
            var message = new Message(new string[] { newUser.Email }, "Confirmation email link", confirmationLink, null);

            await _emailSender.SendEmailAsync(message);

            // await _userManager.AddToRoleAsync(newUser, "Visitor");

            if (createUser.Succeeded)
                return RedirectToAction("Login", new { returnUrl = model.ReturnUrl});

            return View(model);
        }
        catch (Exception ex)
        {
            return View(ex.Message);
        }
    }
    
    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> ConfirmEmail(string mailToken, string userName)
    {
        try
        {
            if (mailToken == null)
                return View("Error", new ErorViewModel { Discruption = "[ConfirmEmail Get Error] : Mail-Token is null" });

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) 
                return View("Error", new ErorViewModel { Discruption = "[ConfirmEmail Get Error] : User is null" });

            var result = await _userManager.ConfirmEmailAsync(user, mailToken);

            if (result.Succeeded)
            {
                return View();
            }
            return View("Error", new ErorViewModel { Discruption = "[ConfirmEmail Get Error] : Confirm Error" });
        }
        catch(Exception ex)
        {
            return View(ex.Message);
        }
    }


    #endregion

    #region [Logout]

    [Route("[action]")]
    public async Task<IActionResult> Logout(string logoutId)
    {
        try
        {
            if(logoutId == null) 
                return View("Error", new ErorViewModel { Discruption = $"[ConfirmEmail Get Error] : Logout Id is null" });

            await _signInManager.SignOutAsync();
            var logoutResult = await _interactionService.GetLogoutContextAsync(logoutId);
            if (logoutResult != null)
            {
                return Redirect("../Home");
            }
            return Redirect("../Home");
        }
        catch (Exception ex)
        {
            return View("Error", new ErorViewModel { Discruption = $"[ConfirmEmail Get Error] : {ex.Message}" });
        }
    }

    #endregion

    #region [Forgot Password Actions]

    [HttpGet]
    [Route("[action]")]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotPasswordModel)
    {
        try
        {
            if (!ModelState.IsValid)
                return View(forgotPasswordModel);

            var user = await _userManager.FindByEmailAsync(forgotPasswordModel.Email);
            if (user == null)
                return RedirectToAction(nameof(ForgotPasswordConfirmation));

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callback = Url.Action(nameof(ResetPassword), "Account", new { token, email = user.Email }, Request.Scheme);

            var message = new Message(new string[] { user.Email }, "Reset password token", callback, null);
            await _emailSender.SendEmailAsync(message);

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }
        catch (Exception ex)
        {
            return View("Error", new ErorViewModel { Discruption = $"[ForgotPassword Get Error] : {ex.Message}" });
        }
    }

    [HttpGet]
    [Route("[action]")]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    [Route("[action]")]
    public IActionResult ResetPassword(string token, string email)
    {
        try
        {
            if (token == null)
                return View("Error", new ErorViewModel { Discruption = "[ResetPassword Get Error] : Token is null" });
            if (email == null)
                return View("Error", new ErorViewModel { Discruption = "[ResetPassword Get Error] : Email is null" });

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }
        catch (Exception ex)
        {
            return View("Error", new ErorViewModel { Discruption = $"[ResetPassword Get Error] : {ex.Message}" });
        }
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordModel)
    {
        try
        {
            if (!ModelState.IsValid)
                return View(resetPasswordModel);

            var user = await _userManager.FindByEmailAsync(resetPasswordModel.Email);
            if (user == null)
                RedirectToAction(nameof(ResetPasswordConfirmation));

            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordModel.Token, resetPasswordModel.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }

                return View();
            }

            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }
        catch(Exception ex)
        {
            return View("Error", new ErorViewModel { Discruption = $"[ResetPassword Get Error] : {ex.Message}" });
        }
    }

    [HttpGet]
    [Route("[action]")]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

        #endregion

    #region [Login With External Services]

    [Route("[action]")]
    public IActionResult ExternalLogin(string provider, string returnUrl)
    {
        try
        {
            if (provider == null)
                return View("Error", new ErorViewModel { Discruption = "[ExternalLogin Get Error] : Provider is null" });
            if (provider == null)
                return View("Error", new ErorViewModel { Discruption = "[ExternalLogin Get Error] : ReturnUrl is null" });

            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }
        catch (Exception ex)
        {
            return View("Error", new ErorViewModel { Discruption = $"[ExternalLogin Get Error] : {ex.Message}" });
        }
    }

    [Route("[action]")]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl)
    {
        try
        {
            if (returnUrl == null)
                return View("Error", new ErorViewModel { Discruption = "[ExternalLoginCallback Get Error] : ReturnUrl is null" });

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login");
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("RegisterExternal", new ExternalLoginViewModel
            {
                ReturnUrl = returnUrl,
                UserName = info.Principal.FindFirstValue(ClaimTypes.Name)
            });
        }
        catch (Exception ex)
        {
            return View("Error", new ErorViewModel { Discruption = $"[ExternalLoginCallback Get Error] : {ex.Message}" });
        }
    }

    [HttpGet]
    [Route("[action]")]
    public IActionResult RegisterExternal(ExternalLoginViewModel model)
    {
        return View();
    }
     
    [HttpPost]
    [Route("[action]")]
    [ActionName("RegisterExternal")]
    public async Task<IActionResult> RegisterExternalConfirmed(ExternalLoginViewModel model)
    {
        try
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login");
            }

            var user = new ApplicationUser(model.UserName);

            var result = await _userManager.CreateAsync(user);

            if (result.Succeeded)
            {

                var claimsResult = await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, "Administrator"));
                if (claimsResult.Succeeded)
                {
                    var identityResult = await _userManager.AddLoginAsync(user, info);
                    if (identityResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, false);
                        return Redirect(model.ReturnUrl);
                    }
                }
            }

            return View(model);
        }
        catch (Exception ex)
        {
            return View("Error", new ErorViewModel { Discruption = $"[RegisterExternalConfirmed Post Error] : {ex.Message}" });
        }
    }

    #endregion
}