using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.All;

    foreach (var header in new[]
             {
                 "Cookie", "Origin", "Priority", "Sec-Fetch-Mode", "Sec-Fetch-Dest", "Sec-Fetch-Site",
                 "Sec-Secure-Session-Id", "Secure-Session-Response", "Secure-Session-Challenge"
             })
    {
        options.RequestHeaders.Add(header);
    }

    foreach (var header in new[]
             {
                 "Set-Cookie", "Cache-Control", "Pragma",
                 "Secure-Session-Registration", "Secure-Session-Challenge"
             })
    {
        options.ResponseHeaders.Add(header);
    }
    
    options.CombineLogs = true;
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
    })
    .AddCookie("Cookies", options =>
    {
        options.Cookie.Name = "__dbsc-auth";
        
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
    })
    .AddDeviceBoundSession("Cookies", options =>
    {
        options.ShortLivedCookieExpiration = TimeSpan.FromMinutes(4);
        options.ChallengeMaxAge = TimeSpan.FromSeconds(2);
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseWhen(
        ctx => ctx.Request.Path.StartsWithSegments("/.well-known/dbsc"),
        branch => branch.UseHttpLogging());
}

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
