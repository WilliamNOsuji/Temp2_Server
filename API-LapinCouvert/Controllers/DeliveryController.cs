using API_LapinCouvert.DTOs;
using LapinCouvert.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_LapinCouvert.Data;
using System.Security.Claims;

namespace API_LapinCouvert.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DeliveryController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private RoleManager<IdentityRole> roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        public DeliveryController(UserManager<IdentityUser> userManager, ApplicationDbContext context, RoleManager<IdentityRole> roleMgr)

        {
            _context = context;
            roleManager = roleMgr;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<DeliveryManDTO>> GetDeliveryManInfo(int deliveryManId)
        {
            DeliveryMan deliveryMan = _context.DeliveryMans.Where(d => d.Id == deliveryManId).FirstOrDefault();

            Client client = _context.Clients.Where(c => c.Id == deliveryMan.ClientId).FirstOrDefault();

            DeliveryManDTO deliveryManDTO = new DeliveryManDTO();
            deliveryManDTO.Username = client.Username;
            deliveryManDTO.FullName = client.GetFullName();

            return Ok(deliveryManDTO);
        }

        [HttpGet("{deviceToken}")]
        [Authorize]
        public async Task<IActionResult> BecomeDeliveryMan(string? deviceToken)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }
            Client client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);

            IdentityUser currentUser = await _userManager.FindByNameAsync(client.User.UserName);

            // Si il est dans le rôle
            if(await _userManager.IsInRoleAsync(currentUser, "deliveryMan"))
            {
                client.DeliveryMan.IsActive = !client.DeliveryMan.IsActive;
                client.IsDeliveryMan = true;
                _context.Update(client);
                _context.SaveChanges();
                return Ok("State changed");
            }

            if(client.DeliveryMan == null)
            {
                client.DeliveryMan = new DeliveryMan(client, deviceToken);
            }
            client.IsDeliveryMan = true;
            _context.Update(client);
            _context.SaveChanges();
            await _userManager.AddToRoleAsync(currentUser, "deliveryMan");

            return Ok("Added as delivery Man");
        }

        [HttpGet]
        [Authorize(Roles = "deliveryMan")]
        [Authorize]
        public async Task<IActionResult> Resign()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return Unauthorized();
            }

            Client? client = await _context.Clients
                .Include(c => c.User) 
                .Include(c => c.DeliveryMan)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (client == null || client.User == null || client.DeliveryMan == null)
            {
                return NotFound("Client or DeliveryMan not found");
            }

            IdentityUser currentUser = await _userManager.FindByNameAsync(client.User.UserName);
            bool IsInRole = await _userManager.IsInRoleAsync(currentUser, "deliveryMan");
            if (currentUser == null)
            {
                return NotFound("User not found");
            }

            // Supprimer le device token
            client.DeliveryMan.DeviceToken = null;
            client.IsDeliveryMan = false;
            _context.Update(client); // Sauvegarde dans la DB
            await _context.SaveChangesAsync();

            var result = await _userManager.RemoveFromRoleAsync(currentUser, "deliveryMan");
            
            if (!result.Succeeded)
            {
                return StatusCode(500, "Failed to remove role");
            }

            return Ok("Removed from Role");
        }


        [HttpPost("{deviceToken}")]
        [Authorize]
        public async Task<IActionResult> UpdateDeviceToken(string newDeviceToken)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { Error = "L'utilisateur n'est pas authentifié." });
            }

            Client client = await _context.Clients
                .Include(c => c.DeliveryMan)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (client == null || client.DeliveryMan == null)
            {
                return NotFound(new { Error = "Client ou livreur introuvable." });
            }

            // Update the device token
            client.DeliveryMan.DeviceToken = newDeviceToken;
            _context.Update(client);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Token mis à jour avec succès." });
        }

        public class UpdateTokenDTO
        {
            public string DeviceToken { get; set; }
        }
    }
}
