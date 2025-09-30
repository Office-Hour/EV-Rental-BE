// Pages/Account/Logout.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.UIAuthService;

namespace WebApp.Areas.Identity.Pages.Account;
public class LogoutModel(IUiAuthService auth) : PageModel
{
    public async Task<IActionResult> OnPostAsync()
    {
        await auth.LogoutAsync(HttpContext.RequestAborted);
        return LocalRedirect(Url.Content("~/"));
    }
}
