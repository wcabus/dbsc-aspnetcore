var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
    })
    .AddCookie("Cookies", options =>
    {
        options.Cookie.Name = "__dbcs-auth";
        
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

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
