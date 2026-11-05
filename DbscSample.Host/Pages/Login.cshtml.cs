using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DbscSample.Host.Pages;

[AllowAnonymous]
public class Login : PageModel
{
    [BindProperty, Required]
    public string Username { get; set; } = string.Empty;

    [BindProperty, Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Demo only: credentials are valid when username == password.
        if (!string.Equals(Username, Password, StringComparison.Ordinal))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }

        var claims = new[]
        {
            new Claim("sub", Username),
            new Claim("name", Username)
        };
        var identity = new ClaimsIdentity(claims, "pwd", nameType: "name", roleType: "role");

        await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(identity));

        // Only allow local redirects to avoid open-redirect issues.
        return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl! : "/");
    }
}
