using Microsoft.AspNetCore.Authentication.OAuth;
using Octokit;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "OAuth";
})
    .AddCookie("Cookies")
    .AddOAuth("OAuth", options =>
     {

         options.SignInScheme = "Cookies";
         options.ClientId = "0eef5d1d814041c68949";
         options.ClientSecret = "274c77ea4f00032b9c8d7aa101c69776d607c659";
         options.CallbackPath = "/OAuth/github"; // The URL the OAuth provider will redirect to after authentication
         options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
         options.TokenEndpoint = "https://github.com/login/oauth/access_token";
         options.SaveTokens = true; // Store access and refresh tokens in the authentication properties
         options.Events = new OAuthEvents
         {
             OnCreatingTicket = async context =>
             {
                 // Retrieve user information from GitHub API
                 var accessToken = context.AccessToken;
                 var gitHubClient = new GitHubClient(new ProductHeaderValue("YourAppName"));
                 gitHubClient.Credentials = new Credentials(accessToken);
                 var user = await gitHubClient.User.Current();

                 // Add the user information to the authentication ticket
                 var claims = new[]
                 {
                                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                                new Claim(ClaimTypes.Name, user.Login),
                                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                                // Add more claims if needed, such as roles, etc.
                            };
                 var identity = new ClaimsIdentity(claims, "GitHub");
                 context.Principal.AddIdentity(identity);
             }
         };
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


//app.MapControllerRoute(
//        name: "oauth-callback",
//        pattern: "oauth/callback",
//        defaults: new { controller = "OAuth", action = "OAuthCallback" },
//            constraints: new { controller = "OAuth" } // Add constraints to match specific controller name.
//);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
