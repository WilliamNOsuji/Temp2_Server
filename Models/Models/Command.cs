using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Models.Models;
using System.ComponentModel;
using Newtonsoft.Json;

namespace LapinCouvert.Models
{
    public class Command
    {
        public int Id { get; set; }

        [DisplayName("Numéro de la commande")]
        public int CommandNumber { get; set; }

        [DisplayName("Numéro de téléphone client")]
        public string ClientPhoneNumber { get; set; }

        public string ArrivalPoint { get; set; }
        public double TotalPrice { get; set; }
        public string Currency { get; set; }
        public bool IsDelivered { get; set; } = false;
        public bool IsInProgress { get; set; } = false;

        public int ClientId { get; set; }

        public int? DeliveryManId { get; set; } = null;

        public DateTime OrderTime { get;set; }

        // This will ben used to send the notfications to the respective client

        public List<string> DeviceToken { get; set; } = new List<string>();

        [JsonIgnore]
        [ValidateNever]
        public virtual List<CommandProduct> CommandProducts { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual Client Client { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual DeliveryMan DeliveryMan { get; set;}
    }
}
