using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenAuth.Domain.Users;

namespace OpenAuth.Api.Pages.Account;

public class Register : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    
    public Register(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
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
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; init; } = string.Empty;
        
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; init; } = string.Empty;
        
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; init; } = string.Empty;
    }
    
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = new User
        {
            UserName = Input.Username,
            Email = Input.Email
        };

        var result = await _userManager.CreateAsync(user, Input.Password);
        
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, false);
            return LocalRedirect(ReturnUrl ?? "/");
        }
        
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
            
        return Page();
    }
}