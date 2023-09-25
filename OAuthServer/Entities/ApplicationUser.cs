using Microsoft.AspNetCore.Identity;

namespace OAuthServer.Entities;

public class ApplicationUser : IdentityUser
{
    public ApplicationUser()
    {

    }

    public ApplicationUser(string username) 
        : base(username)
    {

    }


}