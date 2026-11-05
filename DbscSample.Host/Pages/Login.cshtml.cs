using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DbscSample.Host.Pages;

[AllowAnonymous]
public class Login : PageModel
{
    public IActionResult OnGet()
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.IsLocalUrl(Request.Query["ReturnUrl"]) 
                ? Request.Query["ReturnUrl"]! 
                : "/"
        };
        
        return Challenge(props, "oidc");
    }
}
