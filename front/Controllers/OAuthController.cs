using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

namespace front.Controllers
{
    public class OAuthController : Controller
    {
        public IActionResult Login()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/Oauth/OAuthCallback" }, "OAuth");
        }

        [AllowAnonymous]
        public async Task<IActionResult> OAuthCallback()
        {
            var context = HttpContext.Request.GetDisplayUrl();
            if (!string.IsNullOrEmpty(Request.Query["error"]))
            {
                return View("Error");
            }

            var code = Request.Query["code"].ToString();
            var state = Request.Query["state"].ToString();

            var accessToken = await GetAccessTokenFromOAuthProvider(code);


            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "some-unique-user-id"),
        new Claim(ClaimTypes.Name, "John Doe"),
    };

            var identity = new ClaimsIdentity(claims, "OAuth");

            await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Book");
        }

        private async Task<string> GetAccessTokenFromOAuthProvider(string code)
        {
            var clientId = "0eef5d1d814041c68949";
            var clientSecret = "274c77ea4f00032b9c8d7aa101c69776d607c659";

            var redirectUri = "/OAuth/github";

            var tokenEndpoint = "https://github.com/login/oauth/access_token";

            using (var httpClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri)
                });

                var response = await httpClient.PostAsync(tokenEndpoint, content);

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();

                var accessToken = ExtractAccessToken(responseContent);

                return accessToken;
            }
        }

        private string ExtractAccessToken(string responseContent)
        {
            var parts = responseContent.Split('&');
            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length == 2 && keyValue[0] == "access_token")
                {
                    return keyValue[1];
                }
            }

            return null;
        }
    }
}
