// Pages/Account/Register.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.UIAuthService;

namespace WebApp.Areas.Identity.Pages.Account;
public class RegisterModel(IUiAuthService auth) : PageModel
{
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string? UserName { get; set; }
    [BindProperty] public string Password { get; set; } = "";
    [BindProperty] public string ConfirmPassword { get; set; } = "";
    [BindProperty] public string ReturnUrl { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await auth.RegisterAsync(Email, UserName, Password, ConfirmPassword, HttpContext.RequestAborted, signInAfter: true);
            if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                return LocalRedirect(ReturnUrl);
            return LocalRedirect(Url.Content("~/"));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}
