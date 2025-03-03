// Models/Chat.cs
using LapinCouvert.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LapinCouvert.Models
{
    public class Chat
    {
        [Key]
        public int Id { get; set; }

        public string FirebaseId { get; set; } // References the Firestore document ID

        public int CommandId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? EndedAt { get; set; } = null;

        // Navigation properties
        [ForeignKey("CommandId")]
        [ValidateNever]
        public virtual Command Command { get; set; }
    }
}

