// Services/ChatService.cs
using API_LapinCouvert.DTOs;
using LapinCouvert.Models;
using Microsoft.EntityFrameworkCore;
using MVC_LapinCouvert.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_LapinCouvert.Services
{
    public class ChatService
    {
        private readonly ApplicationDbContext _context;
        private readonly FirebaseService _firebaseService;
        private readonly NotificationsService _notificationsService;

        public ChatService(
            ApplicationDbContext context,
            FirebaseService firebaseService,
            NotificationsService notificationsService)
        {
            _context = context;
            _firebaseService = firebaseService;
            _notificationsService = notificationsService;
        }

        // Create a new chat for a command
        public virtual async Task<string> CreateChatAsync(int commandId)
        {
            // Get the command to retrieve client and delivery man ids
            var command = await _context.Commands
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == commandId);

            if (command == null)
            {
                throw new Exception("Command not found");
            }

            if (command.DeliveryManId == null)
            {
                throw new Exception("Command has no assigned delivery man");
            }

            // Create chat in Firebase
            var firebaseChatId = await _firebaseService.CreateChatAsync(
                commandId,
                command.ClientId,
                command.DeliveryManId.Value);

            // Store chat reference in our database
            var existingChat = await _context.Chats
                .FirstOrDefaultAsync(c => c.CommandId == commandId);

            if (existingChat == null)
            {
                var chat = new Chat
                {
                    CommandId = commandId,
                    FirebaseId = firebaseChatId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Chats.Add(chat);
                await _context.SaveChangesAsync();
            }
            else if (!existingChat.IsActive)
            {
                existingChat.IsActive = true;
                existingChat.EndedAt = null;
                _context.Chats.Update(existingChat);
                await _context.SaveChangesAsync();
            }

            // Send notification to client that chat is available
            if (command.DeviceToken != null && command.DeviceToken.Any())
            {
                await _notificationsService.SendNotificationAsync(
                    "Chat disponible",
                    $"Vous pouvez maintenant discuter avec votre livreur pour la commande #{command.CommandNumber}",
                    new OrderRequestDTO
                    {
                        OrderContent = $"Chat disponible pour la commande #{command.CommandNumber}",
                        DeviceToken = command.DeviceToken.First()
                    }
                );
            }

            return firebaseChatId;
        }

        // End a chat when delivery is complete
        public virtual async Task EndChatAsync(int commandId)
        {
            // Update chat in Firebase
            await _firebaseService.EndChatAsync(commandId);

            // Update our database
            var chat = await _context.Chats
                .FirstOrDefaultAsync(c => c.CommandId == commandId);

            if (chat != null)
            {
                chat.IsActive = false;
                chat.EndedAt = DateTime.UtcNow;

                _context.Chats.Update(chat);
                await _context.SaveChangesAsync();
            }
        }

        // Send a text message
        public virtual async Task<string> SendTextMessageAsync(SendMessageDTO messageDTO)
        {
            // Validate command and chat exist
            var command = await _context.Commands.FindAsync(messageDTO.CommandId);
            if (command == null)
            {
                throw new Exception("Command not found");
            }

            // Check if chat is active
            var isActive = await _firebaseService.IsChatActiveAsync(messageDTO.CommandId);
            if (!isActive)
            {
                throw new Exception("Chat is not active");
            }

            // Send message to Firebase
            return await _firebaseService.SendTextMessageAsync(
                messageDTO.CommandId,
                messageDTO.SenderId,
                messageDTO.SenderType,
                messageDTO.Content);
        }

        // Add a reaction to a message
        public virtual async Task AddReactionAsync(AddReactionDTO reactionDTO)
        {
            // Validate command and chat exist
            var command = await _context.Commands.FindAsync(reactionDTO.CommandId);
            if (command == null)
            {
                throw new Exception("Command not found");
            }

            // Check if chat is active
            var isActive = await _firebaseService.IsChatActiveAsync(reactionDTO.CommandId);
            if (!isActive)
            {
                throw new Exception("Chat is not active");
            }

            // Add reaction to Firebase
            await _firebaseService.AddReactionAsync(
                reactionDTO.CommandId,
                reactionDTO.MessageId,
                reactionDTO.UserId,
                reactionDTO.Reaction);
        }

        // Remove a reaction from a message
        public virtual async Task RemoveReactionAsync(int commandId, string messageId, string userId)
        {
            // Validate command and chat exist
            var command = await _context.Commands.FindAsync(commandId);
            if (command == null)
            {
                throw new Exception("Command not found");
            }

            // Check if chat is active
            var isActive = await _firebaseService.IsChatActiveAsync(commandId);
            if (!isActive)
            {
                throw new Exception("Chat is not active");
            }

            // Remove reaction from Firebase
            await _firebaseService.RemoveReactionAsync(commandId, messageId, userId);
        }

        // Mark a message as read
        public virtual async Task MarkMessageAsReadAsync(int commandId, string messageId)
        {
            // Validate command and chat exist
            var command = await _context.Commands.FindAsync(commandId);
            if (command == null)
            {
                throw new Exception("Command not found");
            }

            // Mark message as read in Firebase
            await _firebaseService.MarkMessageAsReadAsync(commandId, messageId);
        }

        // Get unread messages count
        public virtual async Task<int> GetUnreadMessagesCountAsync(int commandId, string userId)
        {
            // Validate command and chat exist
            var command = await _context.Commands.FindAsync(commandId);
            if (command == null)
            {
                throw new Exception("Command not found");
            }

            // Get unread messages count from Firebase
            return await _firebaseService.GetUnreadMessagesCountAsync(commandId, userId);
        }

        // Check if chat is active
        public virtual async Task<bool> IsChatActiveAsync(int commandId)
        {
            // Check in our database first
            var chat = await _context.Chats
                .FirstOrDefaultAsync(c => c.CommandId == commandId);

            if (chat == null)
            {
                return false;
            }

            // Double-check with Firebase to ensure consistency
            return await _firebaseService.IsChatActiveAsync(commandId);
        }
    }
}