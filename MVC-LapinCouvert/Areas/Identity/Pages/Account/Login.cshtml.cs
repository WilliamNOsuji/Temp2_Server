using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MVC_LapinCouvert.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace MVC_LapinCouvert.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ApplicationDbContext _context;

        public LoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<LoginModel> logger,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Nom d'Utilisateur")]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                _logger.LogInformation("Attempting to log in with username: {Username}", Input.Username);

                // Check if the user exists
                var user = await _userManager.FindByNameAsync(Input.Username);
                if (user == null)
                {
                    _logger.LogWarning("User {Username} not found.", Input.Username);
                    ModelState.AddModelError(string.Empty, "User not found.");
                    return Page();
                }

                // Check if the password is valid
                var isPasswordValid = await _userManager.CheckPasswordAsync(user, Input.Password);
                if (!isPasswordValid)
                {
                    _logger.LogWarning("Invalid password for user {Username}.", Input.Username);
                    ModelState.AddModelError(string.Empty, "Invalid password.");
                    return Page();
                }

                // Define claims for the user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id), // User ID
                    new Claim(ClaimTypes.Name, user.UserName),     // Username
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token ID
                };

                // Check if the user store supports roles
                //if (_userManager.SupportsUserRole)
                //{
                //    // Retrieve roles for the user
                //    var roles = await _userManager.GetRolesAsync(user);
                //    if (roles != null && roles.Any())
                //    {
                //        // Add roles as claims
                //        foreach (var role in roles)
                //        {
                //            claims.Add(new Claim(ClaimTypes.Role, role));
                //        }
                //    }
                //}

                // Attempt to sign in the user with claims
                await _signInManager.SignInWithClaimsAsync(user, Input.RememberMe, claims);

                _logger.LogInformation("User {Username} logged in successfully.", Input.Username);

                // Generate a JWT token (replicating the API's logic)
                var token = GenerateJwtToken(user);

                // Redirect to the return URL
                return LocalRedirect(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            _logger.LogWarning("Model state is invalid for login attempt.");
            return Page();
        }

        private string GenerateJwtToken(IdentityUser user)
        {
            // Define claims for the token
            var authClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Define the signing key and token options
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("C'est tellement la meilleure cle qui a jamais ete cree dans l'histoire de l'humanite (doit etre longue)"));
            var issuer = "http://localhost:5180";
            var expirationTime = DateTime.Now.AddMinutes(30);

            // Create the token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: "your-audience", // Set your audience
                claims: authClaims,
                expires: expirationTime,
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );

            // Serialize the token to a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}