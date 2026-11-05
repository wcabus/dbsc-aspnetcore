using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DbscSample.Host.Pages;

[Authorize]
public class Logout : PageModel
{
    public IActionResult OnPost()
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = "/"
        };
        
        // Let's not sign out of the OIDC provider, just our local cookie. Easier to reauthenticate during demos.
        return SignOut(props, "Cookies");
    }
}