// Pages/Account/Login.cshtml.cs
using Application.Validators.Authentication.Commands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.UIAuthService;

namespace WebApp.Areas.Identity.Pages.Account;
public class LoginModel(IUiAuthService auth) : PageModel
{
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    [BindProperty] public bool RememberMe { get; set; }
    [BindProperty] public string ReturnUrl { get; set; } = "";

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await auth.LoginAsync(Email, Password, RememberMe, HttpContext.RequestAborted);
            
            if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                return LocalRedirect(ReturnUrl);
            return LocalRedirect(Url.Content("~/"));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}
