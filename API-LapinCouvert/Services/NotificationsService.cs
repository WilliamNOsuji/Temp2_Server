using API_LapinCouvert.DTOs;
using FirebaseAdmin.Messaging;
using LapinCouvert.Models;
using Microsoft.EntityFrameworkCore;
using MVC_LapinCouvert.Data;

namespace API_LapinCouvert.Services
{
    public class NotificationsService
    {
        private readonly ApplicationDbContext _context;

        public NotificationsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public virtual async Task SendNotificationAsync(string title, string body, OrderRequestDTO orderRequest)
        {
            await Task.Delay(1000);
            Message message = new()
            {
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body
                },
                Data = new Dictionary<string, string>
                {
                    { "OrderContent", orderRequest.OrderContent }
                },
                Token = orderRequest.DeviceToken
            };

            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }

        public virtual List<string> GetAllDeliveryMenDeviceTokens()
        {
            return _context.DeliveryMans
                .Include(d => d.Client)
                .Where(d => d.Client.IsDeliveryMan && !string.IsNullOrEmpty(d.DeviceToken))
                .Select(d => d.DeviceToken)
                .ToList();
        }

        public virtual async Task SendFirebaseNotificationToDeliveryMen(string message)
        {
            await Task.Delay(1000);

            List<string> deliveryMenTokens = GetAllDeliveryMenDeviceTokens();

            if (deliveryMenTokens.Count == 0) return;

            MulticastMessage firebaseMessage = new()
            {
                Tokens = deliveryMenTokens,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = "Nouvelle commande",
                    Body = message,
                },
                Data = new Dictionary<string, string>
                {
                    { "OrderContent", message }
                }
            };
            BatchResponse response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(firebaseMessage);

            if (response.FailureCount > 0)
            {
                for (int i = 0; i < response.Responses.Count; i++)
                {
                    if (!response.Responses[i].IsSuccess)
                    {
                        string failedToken = deliveryMenTokens[i];
                        await RemoveInvalidToken(failedToken);
                    }
                }
            }
        }

        public virtual async Task RemoveInvalidToken(string invalidToken)
        {
            var deliveryMan = await _context.DeliveryMans.FirstOrDefaultAsync(d => d.DeviceToken == invalidToken);

            if (deliveryMan != null)
            {
                deliveryMan.DeviceToken = null;
                _context.Update(deliveryMan);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Invalid token removed for deliveryman ID: {deliveryMan.Id}");
            }
        }
    }
}
