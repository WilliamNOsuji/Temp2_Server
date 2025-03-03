using Admin_API.Services;
using API_LapinCouvert.DTOs;
using LapinCouvert.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Models.Models;
using MVC_LapinCouvert.Data;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Admin_API.Hubs
{
    public static class UserHandler
    {
        public static Dictionary<string, string> UserConnections { get; set; } = new Dictionary<string, string>();
    }

    [Authorize]
    public class DeliveryHub : Hub
    {
        ApplicationDbContext _context;
        ClientsService _clientsService;
        public string groupName;
        
        public DeliveryHub(ApplicationDbContext context,
            ClientsService clientsService
            )
        {
            _context = context;
            _clientsService = clientsService;
        }


        public async Task JoinDeliveryManGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "DeliveryMan");
        }

        public async Task LeaveDeliveryManGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "DeliveryMan");
        }
       
 

        public async Task Connection(string userId, string connectionIdUser)
        {

            
            //groupName = "Delivery" + 
        
        }
        
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            UserHandler.UserConnections.Remove(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
        
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            var connectionIdUser = Context.ConnectionId;
            string userId = Context.UserIdentifier;
            Client client = _clientsService.GetClientFromUserId(userId);
            UserHandler.UserConnections.Add(connectionIdUser, userId);
        
            if (client != null)
            {
                await Connection(userId, connectionIdUser);
            }
        }

        // TODO :  Notifs using Signal
        public async Task NotifyNewOrder(string orderContent)
        {
            await Clients.Group("DeliveryMan").SendAsync("ReceiveNewOrder", orderContent);
        }

        public async Task NotifyDelivery(string orderContent)
        {
            await Clients.Group("DeliveryMan").SendAsync("ReceiveNewOrder", orderContent);
        }

        public async Task NotifyOrderCanceled(string orderContent)
        {
            await Clients.Group("DeliveryMan").SendAsync("ReceiveNewOrder", orderContent);
        }

        public async Task NotifyOrderAssigned(string orderContent)
        {
            await Clients.Group("DeliveryMan").SendAsync("ReceiveNewOrder", orderContent);
        }
    }
}
