using Microsoft.AspNetCore.Mvc;
using LapinCouvert.Models;
using MVC_LapinCouvert.Data;
using API_LapinCouvert.DTOs;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Models.Models;
using Microsoft.EntityFrameworkCore;
using API_LapinCouvert.Services;
using Supabase.Gotrue;
using Microsoft.AspNetCore.SignalR;
using Admin_API.Hubs;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Authorization;
using MVC_LapinCouvert.Services;
using Client = LapinCouvert.Models.Client;

namespace API_LapinCouvert.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly CommandsService _commandsService;
        private readonly Admin_API.Services.ClientsService _clientsService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHubContext<DeliveryHub> _hubContext;
        private readonly CartService _cartService;
        private readonly NotificationsService _notificationsService;

        public CommandsController(UserManager<IdentityUser> userManager, CommandsService commandsService, IHubContext<DeliveryHub> hubContext, Admin_API.Services.ClientsService clientsService, CartService cartService, NotificationsService notificationsService)
        {
            _userManager = userManager;
            _commandsService = commandsService;
            _hubContext = hubContext;
            _clientsService = clientsService;
            _cartService = cartService;
            _notificationsService = notificationsService;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Command>> Create(CommandDTO commandDTO)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Client client =  _clientsService.GetClientFromUserId(userId);

            if (client == null)
            {
                return NotFound("Client introuvable");
            }

            Cart cart = _cartService.GetCartWithIncludeFromClientId(client.Id);

            if (cart == null)
            {
                return NotFound("Commande introuvable");
            }

            Command command = await _commandsService.CreateCommand(client.Id, commandDTO);

            if (command == null)
            {
                return BadRequest("Erreur lors de la creation de la commande");
            }

            List<CommandProduct> commandProducts = await _commandsService.ConvertCartToCommandProducts(cart, command);

            if (command == null)
            {
                return BadRequest("Erreur lors de la conversion");
            }

            await _commandsService.UpdateInventory(cart.CartProducts.ToList());

            await _commandsService.ClearCartAsync(cart);

            await _notificationsService.SendFirebaseNotificationToDeliveryMen($"Nouvelle commande #{command.CommandNumber} a été passée.");

            return Ok(command);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<Command>>> GetClientCommands()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Client client = _clientsService.GetClientFromUserId(userId);

            if (client == null)
            {
                return NotFound("Le client est introuvable.");
            }

            List<Command>? listeCommandeClient = _commandsService.GetClientCommand(client.Id);

            if(listeCommandeClient == null)
            {
                return NotFound("Commandes introuvables ou aucune commande.");
            }

            return listeCommandeClient;
        }

        [HttpGet]
        //[Authorize(Roles = "deliveryMan")]
        [Authorize]
        public async Task<ActionResult<List<Command>>> GetAllAvailableCommands()
        {
            List<Command>? listeCommandeClient = await _commandsService.GetAvailableCommands();

            if (listeCommandeClient == null)
            {
                return NotFound("Commandes introuvables ou aucune commande.");
            }

            return listeCommandeClient;
        }

        [HttpGet]
        //[Authorize(Roles = "deliveryMan")]
        [Authorize]
        public async Task<ActionResult<List<Command>>> GetMyDeliveries()
        {

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            DeliveryMan deliveryMan =  _commandsService.GetDeliveryManById(userId);

            if (deliveryMan == null)
            {
                return NotFound("Le livreur est introuvable.");
            }

            List<Command>? listeCommandeClient =  await _commandsService.GetMyDeliveries(deliveryMan.Id);

            if (listeCommandeClient == null)
            {
                return NotFound("Commandes introuvables ou aucune commande.");
            }

            return listeCommandeClient;
        }

        [HttpGet("{commandId}")]
        [Authorize(Roles = "deliveryMan")]
        [Authorize]
        public async Task<ActionResult<Command>> AssignADelivery(int commandId)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            DeliveryMan deliveryMan = _commandsService.GetDeliveryManById(userId);

            if(deliveryMan == null)
            {
                return NotFound("Le livreur est introuvable.");
            }

            string? message = await _commandsService.AssignADelivery(deliveryMan.Id, commandId);

            return Ok(message);
        }

        [HttpGet("{commandId}")]
        //[Authorize(Roles = "deliveryMan")]
        [Authorize]
        public async Task<ActionResult<Command>> DeliveryInProgress(int commandId)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            DeliveryMan deliveryMan = _commandsService.GetDeliveryManById(userId);

            if (deliveryMan == null)
            {
                return NotFound("Le livreur est introuvable.");
            }

            string? message = await _commandsService.DeliveryInProgress(deliveryMan.Id, commandId);

            return Ok(message);
        }

        [HttpGet("{commandId}")]
        //[Authorize(Roles = "deliveryMan")]
        [Authorize]
        public async Task<ActionResult<Command>> CommandDelivered(int commandId)
        {

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            DeliveryMan deliveryMan = _commandsService.GetDeliveryManById(userId);

            if (deliveryMan == null)
            {
                return NotFound("Le livreur est introuvable.");
            }

            string message = await _commandsService.CommandDelivered(deliveryMan, commandId);

            return Ok(message);
        }

        [HttpGet("{commandId}")]
        //[Authorize(Roles = "deliveryMan")]
        [Authorize]
        public async Task<ActionResult<Command>> CancelADelivery(int commandId)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            DeliveryMan deliveryMan = _commandsService.GetDeliveryManById(userId);

            if (deliveryMan == null)
            {
                return NotFound("Le livreur est introuvable.");
            }

            string message = await _commandsService.CancelADelivery(deliveryMan, commandId);

            return Ok(message);
        }

        [HttpGet("{commandId}")]
        [Authorize]
        public async Task<ActionResult> GetCommand(int commandId)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Command command = await _commandsService.GetCommandById(commandId);
            if (command == null)
            {
                return NotFound("Command non trouver");
            }

            // Verify the user has access to this command
            Client client = _clientsService.GetClientFromUserId(userId);
            if (client == null)
            {
                return NotFound("Client non trouver");
            }

            // Check if user is either the client or the delivery man for this command
            bool isAuthorized = command.ClientId == client.Id;

            if (!isAuthorized && command.DeliveryManId.HasValue)
            {
                DeliveryMan deliveryMan = _commandsService.GetDeliveryManById(userId);
                isAuthorized = deliveryMan != null && command.DeliveryManId == deliveryMan.Id;
            }

            if (!isAuthorized)
            {
                return Forbid("Vous etes pas authoriser a utiliser cette commande");
            }

            return Ok(command);
        }
    }
}
