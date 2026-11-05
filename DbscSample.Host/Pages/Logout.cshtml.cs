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
        
        return SignOut(props, "Cookies");
    }
}