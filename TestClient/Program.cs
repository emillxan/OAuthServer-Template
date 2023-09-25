using IdentityServer4.AccessTokenValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient();

builder.Services.AddDbContext<DbContext>(options =>
{
    options.UseInMemoryDatabase("mmm");
});

builder.Services.AddAuthentication(config =>
{
    config.DefaultScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
    config.DefaultAuthenticateScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
})
    .AddJwtBearer("Bearer", config =>
    {
        //config.ApiName = "ForumAPI";
        config.Authority = "https://localhost:7060";
        config.Audience = "ForumAPI";
        config.RequireHttpsMetadata = false;
        config.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateAudience = false
        };
    })
    .AddOpenIdConnect("oidc", config =>
    {
        config.Authority = "https://localhost:7060";
        config.ClientId = "client_doubles_forum";
        config.ClientSecret = "client_secret_forum";
        config.SaveTokens = true;

        IdentityModelEventSource.ShowPII = true;

        config.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };

        config.ResponseType = "code id_token";
        config.GetClaimsFromUserInfoEndpoint = true;

        //config.Scope.Add("DoublesAPI");
        //config.Scope.Add("offine_access");

        //config.GetClaimsFromUserInfoEndpoint = true;

        //config.Configuration = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration();
        //config.ClaimActions.MapJsonKey()
    });

builder.Services.AddAuthorization(config =>
{

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
