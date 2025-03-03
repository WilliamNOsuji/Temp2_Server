using API_LapinCouvert.Controllers;
using API_LapinCouvert.DTOs;
using FirebaseAdmin.Messaging;
using LapinCouvert.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Models;
using MVC_LapinCouvert.Data;
using Supabase.Gotrue;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Client = LapinCouvert.Models.Client;
using Command = LapinCouvert.Models.Command;

namespace API_LapinCouvert.Services
{
    public class CommandsService
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationsService _notificationsService;
        private readonly RandomService _randomService;
        private readonly ChatService _chatService;

        public CommandsService(ApplicationDbContext context, NotificationsService notificationsService, RandomService randomService, ChatService chatService)
        {
            _context = context;
            _notificationsService = notificationsService;
            _randomService = randomService;
            _chatService = chatService;
        }
        public virtual async Task<Command> CreateCommand(int clientId ,CommandDTO commandDTO)
        {
            //generation numero de commande
            int commandNumber = _randomService.Next(100000, 1000000);

            Command command = new Command
            {
                CommandNumber = commandNumber,
                ClientPhoneNumber = commandDTO.PhoneNumber,
                ArrivalPoint = commandDTO.Address,
                TotalPrice = commandDTO.TotalPrice,
                Currency = commandDTO.Currency,
                ClientId = clientId,
                OrderTime = DateTime.Now.ToUniversalTime(),
                DeviceToken = commandDTO.DeviceTokens
            };

            await _context.AddAsync(command);
            await _context.SaveChangesAsync();

            return command;
        }

        public virtual async Task<Command> SaveCommand(Command command)
        {
            await _context.AddAsync(command);
            await _context.SaveChangesAsync();

            return command;
        }

        public virtual async Task UpdateInventory(List<CartProducts> cartProducts)
        {
            foreach (CartProducts cartProduct in cartProducts)
            {
                Product product = await _context.Products
                    .Where(p => p.Id == cartProduct.ProductId)
                    .SingleOrDefaultAsync();

                if (product != null)
                {
                    product.Quantity -= cartProduct.Quantity;
                    _context.Update(product);
                }
            }

            await _context.SaveChangesAsync();
        }

        public virtual async Task<Command> GetCommandById(int commandId)
        {
            return await _context.Commands
                .Include(c => c.Client)
                .Include(c => c.DeliveryMan)
                .FirstOrDefaultAsync(c => c.Id == commandId);
        }

        public virtual async Task<List<CommandProduct>> ConvertCartToCommandProducts(Cart cart, Command command)
        {
            List<CommandProduct> commandProducts = new List<CommandProduct>();
            List<CartProducts> cartProducts = cart.CartProducts.ToList();

            foreach (CartProducts cartProduct in cartProducts)
            {
                commandProducts.Add(new CommandProduct(cartProduct, command.Id));
            }

            command.CommandProducts = commandProducts;

            _context.Update(command);
            await _context.SaveChangesAsync();

            return commandProducts;
        }

        public virtual async Task ClearCartAsync(Cart cart)
        {
            cart.CartProducts.Clear();
            _context.Update(cart);
            await _context.SaveChangesAsync();
        }

        public virtual async Task<List<Command>> GetClientCommand(int clientId)
        {
            return await _context.Commands
                .Where(c => c.ClientId == clientId)
                .OrderBy(d => d.OrderTime)
                .ToListAsync();
        }

        public virtual async Task<List<Command>> GetAvailableCommands()
        {
            return await _context.Commands.Where(c => c.DeliveryManId == null)
                .OrderBy(c=>c.OrderTime)
                .ToListAsync();
        }
                
        public virtual async Task<List<Command>> GetMyDeliveries(int deliveryManId)
        {
            List<Command> listeCommandeClient = await _context.Commands
                .Where(c => c.DeliveryManId == deliveryManId && c.IsDelivered == false)
                .OrderBy(d => d.OrderTime)
                .ToListAsync();

            return listeCommandeClient;
        }
                
        public virtual async Task<string> AssignADelivery(int deliveryManId, int commandId)
        {
            Command command = await  _context.Commands
                .Include(c => c.Client)
                .SingleOrDefaultAsync(c => c.Id == commandId);

            if (command == null)
            {
                return "Commande introuvable."; 
            }

            if (command.IsDelivered)
            {
                return "Commande déjà livrée."; 
            }

            if (command.DeliveryManId != null)
            {
                return "La commande est déjà assignée à un autre livreur.";
            }

            command.DeliveryManId = deliveryManId;
            _context.Update(command);
            await _context.SaveChangesAsync();

            // Envoi de la notification
            if (command.DeviceToken != null && command.DeviceToken.Any())
            {
                 _notificationsService.SendNotificationAsync(
                    "Commande assignée.",
                    $"Votre commande #{command.CommandNumber} a été assignée à un livreur.",
                    new OrderRequestDTO { OrderContent = $"Votre commande #{command.CommandNumber} a été assignée à un livreur.", DeviceToken = command.DeviceToken.First() }
                );
            }

            return "Assignation réussie.";
        }

        // Update DeliveryInProgress method to initialize chat
        public virtual async Task<string> DeliveryInProgress(int deliveryManId, int commandId)
        {
            var command = await _context.Commands
                .Include(c => c.Client)
                .SingleOrDefaultAsync(c => c.Id == commandId);

            if (command == null)
            {
                return "Commande introuvable.";
            }

            if (command.IsDelivered)
            {
                return "Commande déjà livrée.";
            }

            if (command.DeliveryManId != deliveryManId)
            {
                return "Cette commande n'est pas assignée à ce livreur.";
            }

            command.IsInProgress = true;
            _context.Update(command);
            await _context.SaveChangesAsync();

            // Initialize chat when delivery is in progress
            await _chatService.CreateChatAsync(commandId);

            // Send notification to client
            //if (command.DeviceToken != null && command.DeviceToken.Any())
            //{
            //    await _notificationsService.SendNotificationAsync(
            //       "Commande en cours de livraison",
            //       $"Votre commande #{command.CommandNumber} est en cours de livraison. Vous pouvez maintenant discuter avec votre livreur.",
            //       new OrderRequestDTO
            //       {
            //           OrderContent = $"Votre commande #{command.CommandNumber} est en cours de livraison.",
            //           DeviceToken = command.DeviceToken.First()
            //       }
            //    );
            //}

            return "Commande en cours de livraison. Chat initialisé.";
        }

        // Update CommandDelivered method to end chat
        public virtual async Task<string> CommandDelivered(DeliveryMan deliveryMan, int commandId)
        {
            Command command = deliveryMan.Commands.FirstOrDefault(c => c.Id == commandId);

            if (command == null)
            {
                return "Commande introuvable pour ce livreur.";
            }

            if (command.IsDelivered)
            {
                return "Commande déjà livrée.";
            }

            command.IsDelivered = true;
            command.IsInProgress = false;
            _context.Update(command);
            await _context.SaveChangesAsync();

            // End chat when delivery is completed
            await _chatService.EndChatAsync(commandId);

            if (command.DeviceToken != null && command.DeviceToken.Any())
            {
                await _notificationsService.SendNotificationAsync(
                    "Commande livrée.",
                    $"Votre commande #{command.CommandNumber} a été livrée !",
                    new OrderRequestDTO
                    {
                        OrderContent = $"Votre commande #{command.CommandNumber} a été livrée !",
                        DeviceToken = command.DeviceToken.First()
                    }
                );
            }

            return "Livraison réussie. Chat terminé.";
        }

        // Update CancelADelivery method to end chat
        public virtual async Task<string> CancelADelivery(DeliveryMan deliveryMan, int commandId)
        {
            var command = deliveryMan.Commands.FirstOrDefault(c => c.Id == commandId);

            if (command == null)
            {
                return "Commande introuvable pour ce livreur.";
            }

            if (command.IsDelivered)
            {
                return "Commande déjà livrée.";
            }

            command.DeliveryManId = null;
            command.DeliveryMan = null;
            command.IsInProgress = false;

            _context.Update(command);
            await _context.SaveChangesAsync();

            // End chat when delivery is cancelled
            await _chatService.EndChatAsync(commandId);

            // Envoi de la notification
            if (command.DeviceToken != null && command.DeviceToken.Any())
            {
                await _notificationsService.SendNotificationAsync(
                    "Commande annulée",
                    $"Votre commande #{command.CommandNumber} a été annulée. Un nouveau livreur sera assigné sous peu.",
                    new OrderRequestDTO
                    {
                        OrderContent = $"Votre commande #{command.CommandNumber} a été annulée. Un nouveau livreur sera assigné sous peu.",
                        DeviceToken = command.DeviceToken.First()
                    }
                );
            }

            return "Annulation réussie. Chat terminé.";
        }
                
        public virtual async Task<DeliveryMan> GetDeliveryManById(string userId)
        {
            DeliveryMan deliveryMan = await _context.DeliveryMans
                .Include(d => d.Client)
                .SingleOrDefaultAsync(d => d.Client.UserId == userId);

            return deliveryMan;
        }
    }
}
