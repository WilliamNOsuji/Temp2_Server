// Services/FirebaseService.cs
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace API_LapinCouvert.Services
{
    public class FirebaseService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public FirebaseService(IConfiguration configuration)
        {
            // Get the project ID from configuration
            string projectId = configuration["Firebase:ProjectId"];

            // Initialize Firestore with explicit project ID
            _firestoreDb = FirestoreDb.Create(projectId);

            // Initialize Storage
            _storageClient = StorageClient.Create();
            _bucketName = configuration["Firebase:StorageBucket"];
        }

        #region Chat Methods

        // Create a new chat in Firestore
        public async Task<string> CreateChatAsync(int commandId, int clientId, int deliveryManId)
        {
            var chatRef = _firestoreDb.Collection("chats").Document(commandId.ToString());
            
            // Check if chat already exists
            var chatDoc = await chatRef.GetSnapshotAsync();
            if (chatDoc.Exists)
            {
                // If chat exists but is not active, reactivate it
                var chatData = chatDoc.ConvertTo<Dictionary<string, object>>();
                if (chatData.ContainsKey("isActive") && !(bool)chatData["isActive"])
                {
                    await chatRef.UpdateAsync(new Dictionary<string, object>
                    {
                        { "isActive", true },
                        { "endedAt", null }
                    });
                }
                return commandId.ToString();
            }
            
            // Create new chat
            var chat = new Dictionary<string, object>
            {
                { "commandId", commandId },
                { "clientId", clientId },
                { "deliveryManId", deliveryManId },
                { "isActive", true },
                { "createdAt", Timestamp.FromDateTime(DateTime.UtcNow) }
            };
            
            await chatRef.SetAsync(chat);
            return commandId.ToString();
        }
        
        // End a chat in Firestore
        public async Task EndChatAsync(int commandId)
        {
            var chatRef = _firestoreDb.Collection("chats").Document(commandId.ToString());
            
            await chatRef.UpdateAsync(new Dictionary<string, object>
            {
                { "isActive", false },
                { "endedAt", Timestamp.FromDateTime(DateTime.UtcNow) }
            });
        }
        
        // Send a text message to Firestore
        public async Task<string> SendTextMessageAsync(int commandId, string senderId, string senderType, string text)
        {
            var messageRef = _firestoreDb
                .Collection("chats")
                .Document(commandId.ToString())
                .Collection("messages")
                .Document();
                
            var message = new Dictionary<string, object>
            {
                { "content", text },
                { "timestamp", Timestamp.FromDateTime(DateTime.UtcNow) },
                { "senderId", senderId },
                { "senderType", senderType },
                { "messageType", "text" },
                { "reactions", new Dictionary<string, object>() },
                { "isRead", false }
            };
            
            await messageRef.SetAsync(message);
            return messageRef.Id;
        }
        
        // Add a reaction to a message in Firestore
        public async Task AddReactionAsync(int commandId, string messageId, string userId, string reaction)
        {
            var messageRef = _firestoreDb
                .Collection("chats")
                .Document(commandId.ToString())
                .Collection("messages")
                .Document(messageId);
            
            await messageRef.UpdateAsync(new Dictionary<string, object>
            {
                { $"reactions.{userId}", reaction }
            });
        }

        // Remove a reaction from a message in Firestore
        public async Task RemoveReactionAsync(int commandId, string messageId, string userId)
        {
            var messageRef = _firestoreDb
                .Collection("chats")
                .Document(commandId.ToString())
                .Collection("messages")
                .Document(messageId);

            // Firestore requires a FieldPath for field deletion
            // Use the Google.Cloud.Firestore namespace instead of FirebaseAdmin.Firestore
            await messageRef.UpdateAsync(new Dictionary<string, object>
            {
                [$"reactions.{userId}"] = Google.Cloud.Firestore.FieldValue.Delete
            });
        }

        // Mark a message as read in Firestore
        public async Task MarkMessageAsReadAsync(int commandId, string messageId)
        {
            var messageRef = _firestoreDb
                .Collection("chats")
                .Document(commandId.ToString())
                .Collection("messages")
                .Document(messageId);
            
            await messageRef.UpdateAsync("isRead", true);
        }
        
        // Get a chat from Firestore
        public async Task<Dictionary<string, object>> GetChatAsync(int commandId)
        {
            var chatRef = _firestoreDb.Collection("chats").Document(commandId.ToString());
            
            var chatDoc = await chatRef.GetSnapshotAsync();
            if (!chatDoc.Exists)
            {
                return null;
            }
            
            return chatDoc.ConvertTo<Dictionary<string, object>>();
        }
        
        // Check if a chat is active
        public async Task<bool> IsChatActiveAsync(int commandId)
        {
            var chatData = await GetChatAsync(commandId);
            if (chatData == null)
            {
                return false;
            }
            
            return chatData.ContainsKey("isActive") && (bool)chatData["isActive"];
        }
        
        // Get unread messages count
        public async Task<int> GetUnreadMessagesCountAsync(int commandId, string userId)
        {
            var query = _firestoreDb
                .Collection("chats")
                .Document(commandId.ToString())
                .Collection("messages")
                .WhereEqualTo("isRead", false)
                .WhereNotEqualTo("senderId", userId);
            
            var querySnapshot = await query.GetSnapshotAsync();
            return querySnapshot.Count;
        }
        
        #endregion
    }
}
