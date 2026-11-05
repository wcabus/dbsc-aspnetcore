var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Retry oidc + distributed ticket store, also adding the distributed store to the DBCS bits?
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
        // Don't set these too short. Refreshing too often will trigger a quota on the TPM and will cause the refresh to fail.
        options.ShortLivedCookieExpiration = TimeSpan.FromMinutes(4);
        options.ChallengeMaxAge = TimeSpan.FromSeconds(2);
    });

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

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
