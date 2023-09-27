using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace OAuthServer.Data;

public class Databaseinitializer
{
    public static void Init(IServiceProvider scopeServiceProvider)
    {
        //var context = scopeServiceProvider.GetService<ApplicationDbContext>(); 
        var userManager = scopeServiceProvider.GetService<UserManager<IdentityUser>>();

        var user = new IdentityUser
        {
            UserName = "User",
        };

        var result = userManager.CreateAsync(user, "123qwe").GetAwaiter().GetResult();
        if (result.Succeeded)
        {
            userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, "Administrator")).GetAwaiter().GetResult();
        }


        //context.Users.Add(user);
        //context.SaveChanges();
    }
}