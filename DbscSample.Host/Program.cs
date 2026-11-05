using DbscSample.Host;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
        options.DefaultChallengeScheme = "oidc";
    })
    .AddCookie("Cookies", options =>
    {
        options.Cookie.Name = "__dbcs-auth";
        
        // Setting this to None. This allows sending the cookie to
        // the dbcs registration endpoint after the redirect from the identity provider.
        // Not ideal...
        options.Cookie.SameSite = SameSiteMode.None;
    })
    .AddDeviceBoundSession("Cookies", options =>
    {
        // Don't set these too short. Refreshing too often will trigger a quota on the TPM and will cause the refresh to fail.
        options.ShortLivedCookieExpiration = TimeSpan.FromMinutes(4);
        options.ChallengeMaxAge = TimeSpan.FromMinutes(2);
    })
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = "https://demo.duendesoftware.com";
        
        options.ClientId = "interactive.confidential";
        options.ClientSecret = "secret";
        
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("api");
        
        options.ResponseType = "code";
        options.UsePkce = true;
        
        options.MapInboundClaims = false;
        options.SaveTokens = true;
        options.DisableTelemetry = true;
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

builder.AddRedisDistributedCache("redis");
builder.Services.AddDistributedCacheTicketStore("Cookies");
// Potential improvement: add ticket stores for the DBSC schemes as well.

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/.well-known/dbsc"))
    {
        var log = ctx.RequestServices.GetRequiredService<ILogger<Program>>();
        log.LogInformation("DBSC {Method} {Path} cookies=[{Cookies}] sessionId={SessionId}",
            ctx.Request.Method, ctx.Request.Path,
            string.Join(", ", ctx.Request.Cookies.Keys),
            ctx.Request.Headers["Sec-Secure-Session-Id"].ToString());

        await next();

        log.LogInformation("DBSC {Path} -> {Status}", ctx.Request.Path, ctx.Response.StatusCode);
        return;
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
