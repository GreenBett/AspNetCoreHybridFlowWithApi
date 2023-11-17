using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using WebExtraClaimsCached;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<MyClaimsTransformation>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    options.SignInScheme = "Cookies";
    options.Authority = "https://localhost:44352";
    options.RequireHttpsMetadata = true;
    options.ClientId = "codeflowpkceclient";
    options.ClientSecret = "codeflow_pkce_client_secret";
    options.ResponseType = "code";
    options.UsePkce = true;
    options.Scope.Add("profile");
    options.Scope.Add("offline_access");
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.ClaimActions.MapUniqueJsonKey("preferred_username", "preferred_username");
    options.ClaimActions.MapUniqueJsonKey("gender", "gender");
});

builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Events.OnTokenValidated = async context =>
    {
        await context.HttpContext.RequestServices
            .GetRequiredService<MyClaimsTransformation>()
            .TransformAsync(context.Principal!);
    };
});

builder.Services.AddRazorPages();

var app = builder.Build();

IdentityModelEventSource.ShowPII = true;
JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
