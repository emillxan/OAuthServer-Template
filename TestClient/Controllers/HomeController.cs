using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;
using TestClient.Models;
using TestClient.Services;
using System.Security.Claims;

namespace TestClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, 
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Get UserName in Id Token
        [Authorize]
        public async Task<IActionResult> Privacy()
        {
            var model = new ClaimManager(HttpContext, User);
            string userName;
            foreach (var item in model.Items)
            {
                foreach (var claim in GetClaims(item))
                {
                    if(claim.Type == "name")
                    {
                        userName = claim.Value;
                        break;
                    }
                }
            }

            
            //try
            //{
            //    //ViewBag.Message = await GetSecretAsync(model);
            //    return View(model);
            //}
            //catch (Exception exception)
            //{
            //    await RefreshToken(model.RefreshToken);
            //    var model2 = new ClaimManager(HttpContext, User);
            //    //ViewBag.Message = await GetSecretAsync(model2);
            //}
            return View(model);
        }

        public List<Claim> GetClaims(ClaimViewer item)
        {
            if (item.Claims != null && item.Claims.Count > 0)
            {
                return item.Claims;
            }
            return new List<Claim>();
        }

        private async Task<string> GetSecretAsync(ClaimManager model)
        {
            var client = _httpClientFactory.CreateClient();
            client.SetBearerToken(model.AccessToken);
            return await client.GetStringAsync("https://localhost:5001/site/secret");
        }

        private async Task RefreshToken(string refreshToken)
        {
            var refreshClient = _httpClientFactory.CreateClient();
            var resultRefreshTokenAsync = await refreshClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = "https://localhost:7060/connect/token",
                ClientId = "test_client_id",
                ClientSecret = "test_client_secret",
                RefreshToken = refreshToken,
                Scope = "openid OrdersAPI offline_access"
            });

            await UpdateAuthContextAsync(resultRefreshTokenAsync.AccessToken, resultRefreshTokenAsync.RefreshToken);
        }

        private async Task UpdateAuthContextAsync(string accessTokenNew, string refreshTokenNew)
        {
            var authenticate = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            authenticate.Properties.UpdateTokenValue("access_token", accessTokenNew);
            authenticate.Properties.UpdateTokenValue("refresh_token", refreshTokenNew);

            await HttpContext.SignInAsync(authenticate.Principal, authenticate.Properties);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}