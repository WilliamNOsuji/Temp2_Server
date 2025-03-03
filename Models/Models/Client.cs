
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Humanizer.Localisation.DateToOrdinalWords;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Build.ObjectModelRemoting;
using Supabase.Gotrue;

namespace LapinCouvert.Models
{
    public class Client
    {
        public int Id { get; set; }
        public required string UserId { get; set; }

        public string Username { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        [JsonIgnore]
        public virtual IdentityUser User { get; set; }
        public bool IsBanned { get; set; } = false;

        public bool IsDeliveryMan { get; set; } = false;

        public bool IsAdmin { get; set; } = false;

        public string ImageURL { get; set; } = "";

        [ForeignKey("Cart")]
        public int? CartId { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual List<Command> Commands { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual DeliveryMan DeliveryMan { get; set; } 

        [JsonIgnore]
        [ValidateNever]
        public virtual Cart Cart { get; set; }

        //[JsonIgnore]
        //[ValidateNever]
        //public virtual List<SuggestedProduct>? ForClients { get; set; } = new List<SuggestedProduct>();
        //[JsonIgnore]
        //[ValidateNever]
        //public virtual List<SuggestedProduct>? AgainstClients { get; set; } = new List<SuggestedProduct>();

        public string GetFullName()
        {
            return FirstName + " " + LastName;
        }
    }
}
