using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DbscSample.Host.Pages;

[Authorize]
public class Secure : PageModel;