using IdentityServer4.AspNetIdentity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OAuthServer;
using OAuthServer.CustomTokenProviders;
using OAuthServer.Data;
using OAuthServer.Services.Email;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();



builder.Services.AddDbContext<ApplicationDbContext>(config =>
{
    config.UseInMemoryDatabase("mem");
})
    .AddIdentity<IdentityUser, IdentityRole>(config =>
    {
        config.Password.RequireDigit = false;
        config.Password.RequireLowercase = false;
        config.Password.RequireNonAlphanumeric = false;
        config.Password.RequireUppercase = false;
        config.Password.RequiredLength = 6;

        config.User.RequireUniqueEmail = true;
        config.SignIn.RequireConfirmedEmail = true;
        config.Tokens.EmailConfirmationTokenProvider = "EmailConfirmation";
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddTokenProvider<EmailConfirmationTokenProvider<IdentityUser>>("EmailConfirmation");

builder.Services.Configure<DataProtectionTokenProviderOptions>(option =>
    option.TokenLifespan = TimeSpan.FromHours(2));

builder.Services.Configure<EmailConfirmationTokenProviderOptions>(option =>
    option.TokenLifespan = TimeSpan.FromDays(3));

var emailConfig = builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);
builder.Services.AddScoped<IEmailSender, EmailSender>();


builder.Services.AddIdentityServer(option =>
{
    option.UserInteraction.LoginUrl = "/Account/Login";
    option.UserInteraction.LogoutUrl = "/Account/Logout";
})
    .AddAspNetIdentity<IdentityUser>()
    .AddInMemoryClients(Configuration.GetClient())
    .AddInMemoryApiResources(Configuration.GetApiResources())
    .AddInMemoryIdentityResources(Configuration.GetIdentityResources())
    //.AddProfileService<ProfileService>()
    .AddDeveloperSigningCredential();


builder.Services.AddAuthentication()
    .AddFacebook(config =>
    {
        config.AppId = builder.Configuration["Authentication:Facebook:AppId"];
        config.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
        config.ClaimsIssuer = "Facebook";
    })
    .AddGoogle(config =>
    {
        config.ClientId = "721065043472-6lsiv55rv7ch2la99debaq0oap9tc0bp.apps.googleusercontent.com";
        config.ClientSecret = "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";
        config.ClaimsIssuer = "Google";
    })
    .AddOAuth("VK", "Vkontakte", config =>
    {
        config.ClientId = builder.Configuration["Authentication:VK:ClientId"];
        config.ClientSecret = builder.Configuration["Authentication:VK:ClientSecret"];
        config.ClaimsIssuer = builder.Configuration["Authentication:VK:ClaimsIssuer"];
        config.CallbackPath = new PathString(builder.Configuration["Authentication:VK:CallbackPath"]);
        config.AuthorizationEndpoint = builder.Configuration["Authentication:VK:AuthorizationEndpoint"];
        config.TokenEndpoint = builder.Configuration["Authentication:VK:TokenEndpoint"];
        config.Scope.Add("email");
        config.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "user_id");
        config.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        config.SaveTokens = true;
        config.Events = new OAuthEvents
        {
            OnCreatingTicket = context =>
            {
                context.RunClaimActions(context.TokenResponse.Response.RootElement);
                return Task.CompletedTask;
            },
            OnRemoteFailure = OnFailure
        };
    });

Task OnFailure(RemoteFailureContext arg)
{
    Console.WriteLine(arg);
    return Task.CompletedTask;
}

//builder.Services.AddScoped<IEmailSender, EmailSender>();

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
app.UseIdentityServer();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
