using Admin_API.Services;
using API_LapinCouvert.DTOs;
using LapinCouvert.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.DTOs;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Admin_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private ClientsService _clientsService;

        public AccountController(UserManager<IdentityUser> userManager,
                                    SignInManager<IdentityUser> signInManager,
                                    ClientsService clientsService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _clientsService = clientsService;
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginDTO loginDTO)
        {
            var result = await _signInManager.PasswordSignInAsync(loginDTO.Username, loginDTO.Password, true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(loginDTO.Username);
                if (user == null)
                {
                    return NotFound(new { Error = "User not found." });
                }


                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var role in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }

                SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("C'est tellement la meilleure cle qui a jamais ete cree dans l'histoire de l'humanite (doit etre longue)"));
                string issuer = "http://localhost:5180";
                DateTime expirationTime = DateTime.Now.AddMinutes(30);

                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: "your-audience", // Set your audience
                    claims: authClaims,
                    expires: expirationTime,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );

                string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                LapinCouvert.Models.Client client = await _clientsService.GetClientFromUserId(user.Id);
                bool isActive = false;
                bool isDeliveryMan = await _userManager.IsInRoleAsync(user, "deliveryMan");
                if (client.DeliveryMan != null)
                {
                    isActive = client.DeliveryMan.IsActive;
                    client.DeliveryMan.DeviceToken = loginDTO.DeviceToken;
                    _clientsService.UpdateClient(client);
                }

                await _signInManager.SignInWithClaimsAsync(user, true, authClaims);


                return Ok(new LoginSuccessDTO(tokenString, client.Id, client.User.UserName, isDeliveryMan, isActive));
            }

            return NotFound(new { Error = "L'utilisateur est introuvable ou le mot de passe ne concorde pas" });
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterDTO registerDTO)
        {
            try
            {
                if (registerDTO.Password != registerDTO.PasswordConfirm)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Le mot de passe et la confirmation ne sont pas identique" });
                }

                IdentityUser user = new IdentityUser()
                {
                    UserName = registerDTO.Username,
                    Email = registerDTO.Email
                };
                IdentityResult identityResult = await _userManager.CreateAsync(user, registerDTO.Password);

                if (!identityResult.Succeeded)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Error = identityResult.Errors });
                }

                _clientsService.CreateClient(user, registerDTO);

                return await Login(new LoginDTO() { Username = registerDTO.Username, Password = registerDTO.Password });
            }
            catch (Exception ex)
            {
                return BadRequest("Une erreur est arriver lors de l'enregistrement: " + ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ProfileDTO>> GetProfileInfo()
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                ProfileDTO profileDTO = await _clientsService.GetProfileInfo(userId);
                return Ok(profileDTO);
            }
            catch (Exception ex)
            {
                return BadRequest("Erreur lors de l'obtention du profile");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProfileDTO>> ModifyProfile(ProfileModificationDTO profileModificationDTO)
        {
            if (profileModificationDTO == null)
            {
                return BadRequest("Profile data is invalid.");
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Retrieve the client and user identity
            Client client = await _clientsService.GetClientFromUserId(userId);
            if (client == null)
            {
                return NotFound("Client introuvable");
            }

            IdentityUser identityUser = await _userManager.FindByIdAsync(userId);
            if (identityUser == null)
            {
                return NotFound("User introuvable");
            }

            // Modify First Name if provided
            if (!string.IsNullOrEmpty(profileModificationDTO.NewFirstName))
            {
                client.FirstName = profileModificationDTO.NewFirstName;
            }

            // Modify Last Name if provided
            if (!string.IsNullOrEmpty(profileModificationDTO.NewLastName))
            {
                client.LastName = profileModificationDTO.NewLastName;
            }

            // Modify Password if provided
            if (!string.IsNullOrEmpty(profileModificationDTO.NewPassword))
            {
                IdentityResult identityResult = await _userManager.ChangePasswordAsync(identityUser, profileModificationDTO.OldPassword, profileModificationDTO.NewPassword);

                if (!identityResult.Succeeded)
                {
                    return BadRequest("Échec du changement de mot de passe");
                }
            }

            try
            {
                _clientsService.UpdateClient(client);
            }
            catch (Exception e)
            {
                return NotFound("Client non trouvé dans la base de données");
            }

            // Fetch updated profile info
            ProfileDTO profileDTO = await _clientsService.GetProfileInfo(userId);
            if (profileDTO == null)
            {
                return BadRequest("Erreur lors de l'obtention du profile");
            }

            return Ok(profileDTO);
            
        }

        [HttpPost]
        public async Task<ActionResult> ModifyImage(IFormFile file)
        {

            string url = "https://jaewoltprrassmozmehq.supabase.co"; // Replace with your Supabase project URL
            string key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImphZXdvbHRwcnJhc3Ntb3ptZWhxIiwicm9sZSI6ImFub24iLCJpYXQiOjE3Mzg2ODk4MTksImV4cCI6MjA1NDI2NTgxOX0.7_eIE2cT6AvERSxChnWrV2TCN6tAN3277m_LhbGVgrw"; // Replace with your Supabase anon key

            Supabase.Client _supabaseClient = new Supabase.Client(url, key);

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Client client = await _clientsService.GetClientFromUserId(userId);
            if (client == null)
            {
                return NotFound("Client introuvable");
            }


            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided.");
            }

            string fileName = $"{Guid.NewGuid()}_{file.FileName}";
            string filePath = $"uploads/{fileName}";

            using (var memoryStream = new MemoryStream())
            {
                await file.OpenReadStream().CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray(); // 🔥 Convert Stream to byte[]

                string result = await _supabaseClient.Storage
                    .From("images") // Assure-toi d'avoir un bucket "images" sur Supabase
                    .Upload(fileBytes, filePath, new Supabase.Storage.FileOptions { ContentType = file.ContentType });
            }

            string publicUrl = _supabaseClient.Storage.From("images").GetPublicUrl(filePath);

            client.ImageURL = publicUrl;
            try
            {
                _clientsService.UpdateClient(client);
            }
            catch (Exception e)
            {
                return NotFound("Client non trouvé dans la base de données");
            }
            

            return Ok("Profile image updated successfully.");
        }

        [Authorize]
        [HttpGet]
        public ActionResult<string[]> PrivateData()
        {
            return new string[] { "figue", "banane", "noix" };
        }

        [HttpGet]
        public ActionResult<string[]> PublicData()
        {
            return new string[] { "public", "publique" };
        }

        [HttpGet]
        [Authorize(Policy = "RequireAuthenticatedUser")]
        public IActionResult GetPrivateData()
        {
            return Ok("This is private data.");
        }
    }
}
