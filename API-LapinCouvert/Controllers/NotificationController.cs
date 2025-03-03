using API_LapinCouvert.DTOs;
using API_LapinCouvert.Services;
using Microsoft.AspNetCore.Mvc;

namespace API_LapinCouvert.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationsService _notificationsService;

        public NotificationController(NotificationsService notificationsService)
        {
            _notificationsService = notificationsService;
        }

        [HttpPost]
        public async Task<IActionResult> NewOrder([FromBody] OrderRequestDTO orderRequest)
        {
            await Task.Delay(1000);
            await _notificationsService.SendNotificationAsync(
                "Une nouvelle commande a été ajoutée. 👀‼️",
                "Soyez prêt à prendre cette commande.",
                orderRequest);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendOrder([FromBody] OrderRequestDTO orderRequest)
        {
            await Task.Delay(1000);
            await _notificationsService.SendNotificationAsync(
                "Commande acceptée.",
                $"{orderRequest.OrderContent} est en préparation et sera bientôt prête pour la livraison. 😁👌",
                orderRequest);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> OrderDelivery([FromBody] OrderRequestDTO orderRequest)
        {
            await Task.Delay(1000);
            await _notificationsService.SendNotificationAsync(
                "La commande est en cours de prise en charge !",
                $"{orderRequest.OrderContent} est en route. 🚚💨",
                orderRequest);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> OrderDelivered([FromBody] OrderRequestDTO orderRequest)
        {
            await Task.Delay(1000);
            await _notificationsService.SendNotificationAsync(
                "Commande livrée.",
                $"{orderRequest.OrderContent} a été livrée ! 🤟🥳🤙",
                orderRequest);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CanceledOrder([FromBody] OrderRequestDTO orderRequest)
        {
            await Task.Delay(1000);
            await _notificationsService.SendNotificationAsync(
                "Commande annulée.",
                "Votre commande a été annulée, un nouveau livreur sera assigné sous peu. 😭",
                orderRequest);
            return Ok();
        }
    }
}
