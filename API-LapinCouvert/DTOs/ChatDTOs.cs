using LapinCouvert.Models;
using Models.Models;
using System.ComponentModel.DataAnnotations;

// DTOs/ChatDTOs.cs
namespace API_LapinCouvert.DTOs
{
    public class ChatDTO
    {
        public int CommandId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }

    public class ChatMessageDTO
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public string SenderId { get; set; }
        public string SenderType { get; set; } // "client" or "deliveryMan"
        public string MessageType { get; set; } // "text", "image", or "emoji"
        public Dictionary<string, string> Reactions { get; set; } = new Dictionary<string, string>();
        public bool IsRead { get; set; }
    }

    public class SendMessageDTO
    {
        public int CommandId { get; set; }
        public string Content { get; set; }
        public string SenderId { get; set; }
        public string SenderType { get; set; }
        public string MessageType { get; set; } = "text";
    }

    public class AddReactionDTO
    {
        public int CommandId { get; set; }
        public string MessageId { get; set; }
        public string UserId { get; set; }
        public string Reaction { get; set; }
    }
}