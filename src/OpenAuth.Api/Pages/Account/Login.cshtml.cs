using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenAuth.Domain.Users;

namespace OpenAuth.Api.Pages.Account;

public class Login : PageModel
{
    private readonly SignInManager<User> _signInManager;
    
    public Login(SignInManager<User> signInManager)
    {
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();
    
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; init; } = string.Empty;
        
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; init; } = string.Empty;
        
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; init; } = false;
    }
    
    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return LocalRedirect(ReturnUrl ?? "/");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await _signInManager.PasswordSignInAsync(
            Input.Username,
            Input.Password,
            Input.RememberMe,
            true);


        if (result.Succeeded)
            return LocalRedirect(ReturnUrl ?? "/");
        
        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account has been locked out, please try again later.");
            return Page();
        }
        
        ModelState.AddModelError(string.Empty, "Invalid username of password.");
        return Page();
    }
}