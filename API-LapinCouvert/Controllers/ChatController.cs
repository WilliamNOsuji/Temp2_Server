// Controllers/ChatController.cs
using API_LapinCouvert.DTOs;
using API_LapinCouvert.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API_LapinCouvert.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;
        private readonly Admin_API.Services.ClientsService _clientsService;
        private readonly CommandsService _commandsService;

        public ChatController(
            ChatService chatService,
            Admin_API.Services.ClientsService clientsService,
            CommandsService commandsService)
        {
            _chatService = chatService;
            _clientsService = clientsService;
            _commandsService = commandsService;
        }

        // Create a chat for a command
        [HttpPost("{commandId}")]
        public async Task<IActionResult> CreateChat(int commandId)
        {
            try
            {
                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Verify the user has access to this command (either as client or delivery man)
                var command = await _commandsService.GetCommandById(commandId);
                if (command == null)
                {
                    return NotFound("Command not found");
                }

                var client = _clientsService.GetClientFromUserId(userId);
                if (client == null)
                {
                    return NotFound("Client not found");
                }

                // Check if user is either the client or the delivery man for this command
                //bool isAuthorized = command.ClientId == client.Id;
                //
                //if (!isAuthorized && command.DeliveryManId.HasValue)
                //{
                //    var deliveryMan = _commandsService.GetDeliveryManById(userId);
                //    isAuthorized = deliveryMan != null && command.DeliveryManId == deliveryMan.Id;
                //}
                //
                //if (!isAuthorized)
                //{
                //    return Forbid("You are not authorized to create a chat for this command");
                //}

                // Create the chat
                var chatId = await _chatService.CreateChatAsync(commandId);

                return Ok(new { ChatId = chatId });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to create chat: {ex.Message}");
            }
        }

        // Send a text message
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDTO messageDTO)
        {
            try
            {
                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Verify the user has access to this command
                var command = await _commandsService.GetCommandById(messageDTO.CommandId);
                if (command == null)
                {
                    return NotFound("Command not found");
                }

                var client = _clientsService.GetClientFromUserId(userId);
                if (client == null)
                {
                    return NotFound("Client not found");
                }

                // Check if user is either the client or the delivery man for this command
                bool isAuthorized = command.ClientId == client.Id;

                if (!isAuthorized && command.DeliveryManId.HasValue)
                {
                    var deliveryMan = _commandsService.GetDeliveryManById(userId);
                    isAuthorized = deliveryMan != null && command.DeliveryManId == deliveryMan.Id;
                }

                if (!isAuthorized)
                {
                    return Forbid("You are not authorized to send messages for this command");
                }

                // Set the SenderId to the current user's ID
                if (messageDTO.SenderType == "client")
                {
                    messageDTO.SenderId = client.Id.ToString();
                }
                else if (messageDTO.SenderType == "deliveryMan")
                {
                    var deliveryMan = _commandsService.GetDeliveryManById(userId);
                    if (deliveryMan == null)
                    {
                        return NotFound("Delivery man not found");
                    }

                    messageDTO.SenderId = deliveryMan.Id.ToString();
                }

                // Send the message
                var messageId = await _chatService.SendTextMessageAsync(messageDTO);

                return Ok(new { MessageId = messageId });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to send message: {ex.Message}");
            }
        }

        // Add a reaction to a message
        [HttpPost]
        public async Task<IActionResult> AddReaction([FromBody] AddReactionDTO reactionDTO)
        {
            try
            {
                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Verify the user has access to this command
                var command = await _commandsService.GetCommandById(reactionDTO.CommandId);
                if (command == null)
                {
                    return NotFound("Command not found");
                }

                var client = _clientsService.GetClientFromUserId(userId);
                if (client == null)
                {
                    return NotFound("Client not found");
                }

                // Check if user is either the client or the delivery man for this command
                bool isAuthorized = command.ClientId == client.Id;

                if (!isAuthorized && command.DeliveryManId.HasValue)
                {
                    var deliveryMan = _commandsService.GetDeliveryManById(userId);
                    isAuthorized = deliveryMan != null && command.DeliveryManId == deliveryMan.Id;
                }

                if (!isAuthorized)
                {
                    return Forbid("You are not authorized to add reactions for this command");
                }

                // Add the reaction
                await _chatService.AddReactionAsync(reactionDTO);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to add reaction: {ex.Message}");
            }
        }

        // Remove a reaction from a message
        [HttpDelete("{commandId}/{messageId}")]
        public async Task<IActionResult> RemoveReaction(int commandId, string messageId)
        {
            try
            {
                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Verify the user has access to this command
                var command = await _commandsService.GetCommandById(commandId);
                if (command == null)
                {
                    return NotFound("Command not found");
                }

                var client = _clientsService.GetClientFromUserId(userId);
                if (client == null)
                {
                    return NotFound("Client not found");
                }

                // Check if user is either the client or the delivery man for this command
                bool isAuthorized = command.ClientId == client.Id;
                string userIdForReaction = client.Id.ToString();

                if (!isAuthorized && command.DeliveryManId.HasValue)
                {
                    var deliveryMan = _commandsService.GetDeliveryManById(userId);
                    isAuthorized = deliveryMan != null && command.DeliveryManId == deliveryMan.Id;

                    if (isAuthorized)
                    {
                        userIdForReaction = deliveryMan.Id.ToString();
                    }
                }

                if (!isAuthorized)
                {
                    return Forbid("You are not authorized to remove reactions for this command");
                }

                // Remove the reaction
                await _chatService.RemoveReactionAsync(commandId, messageId, userIdForReaction);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to remove reaction: {ex.Message}");
            }
        }

        // Mark a message as read
        [HttpPut("{commandId}/{messageId}/read")]
        public async Task<IActionResult> MarkAsRead(int commandId, string messageId)
        {
            try
            {
                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Verify the user has access to this command
                var command = await _commandsService.GetCommandById(commandId);
                if (command == null)
                {
                    return NotFound("Command not found");
                }

                var client = _clientsService.GetClientFromUserId(userId);
                if (client == null)
                {
                    return NotFound("Client not found");
                }

                // Check if user is either the client or the delivery man for this command
                bool isAuthorized = command.ClientId == client.Id;

                if (!isAuthorized && command.DeliveryManId.HasValue)
                {
                    var deliveryMan = _commandsService.GetDeliveryManById(userId);
                    isAuthorized = deliveryMan != null && command.DeliveryManId == deliveryMan.Id;
                }

                if (!isAuthorized)
                {
                    return Forbid("You are not authorized to mark messages as read for this command");
                }

                // Mark the message as read
                await _chatService.MarkMessageAsReadAsync(commandId, messageId);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to mark message as read: {ex.Message}");
            }
        }

        // Get unread messages count
        [HttpGet("{commandId}/unread")]
        public async Task<IActionResult> GetUnreadCount(int commandId)
        {
            try
            {
                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Verify the user has access to this command
                var command = await _commandsService.GetCommandById(commandId);
                if (command == null)
                {
                    return NotFound("Command not found");
                }

                var client = _clientsService.GetClientFromUserId(userId);
                if (client == null)
                {
                    return NotFound("Client not found");
                }

                // Check if user is either the client or the delivery man for this command
                bool isAuthorized = command.ClientId == client.Id;
                string userIdForCount = client.Id.ToString();

                if (!isAuthorized && command.DeliveryManId.HasValue)
                {
                    var deliveryMan = _commandsService.GetDeliveryManById(userId);
                    isAuthorized = deliveryMan != null && command.DeliveryManId == deliveryMan.Id;

                    if (isAuthorized)
                    {
                        userIdForCount = deliveryMan.Id.ToString();
                    }
                }

                if (!isAuthorized)
                {
                    return Forbid("You are not authorized to get unread count for this command");
                }

                // Get the unread messages count
                var count = await _chatService.GetUnreadMessagesCountAsync(commandId, userIdForCount);

                return Ok(new { UnreadCount = count });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to get unread count: {ex.Message}");
            }
        }

        // Check if chat is active
        [HttpGet("{commandId}/active")]
        public async Task<IActionResult> IsChatActive(int commandId)
        {
            try
            {
                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Verify the user has access to this command
                var command = await _commandsService.GetCommandById(commandId);
                if (command == null)
                {
                    return NotFound("Command not found");
                }

                var client = _clientsService.GetClientFromUserId(userId);
                if (client == null)
                {
                    return NotFound("Client not found");
                }

                // Check if user is either the client or the delivery man for this command
                bool isAuthorized = command.ClientId == client.Id;

                if (!isAuthorized && command.DeliveryManId.HasValue)
                {
                    var deliveryMan = _commandsService.GetDeliveryManById(userId);
                    isAuthorized = deliveryMan != null && command.DeliveryManId == deliveryMan.Id;
                }

                if (!isAuthorized)
                {
                    return Forbid("You are not authorized to check chat status for this command");
                }

                // Check if chat is active
                var isActive = await _chatService.IsChatActiveAsync(commandId);

                return Ok(new { IsActive = isActive });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to check chat status: {ex.Message}");
            }
        }
    }
}