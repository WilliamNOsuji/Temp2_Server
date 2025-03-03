using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;
namespace LapinCouvert.Models
{
    public class DeliveryMan
    {
        public DeliveryMan() { }

        public DeliveryMan(Client client, string deviceToken)
        {
            Client = client;
            ClientId = client.Id;
            DeviceToken = deviceToken;
        }
        public int Id { get; set; }
        public double Money { get; set; } = 0;
        public bool IsActive { get; set; } = false; 
        public int ClientId { get; set; }
        public string? DeviceToken { get; set; }

        [ValidateNever]
        [JsonIgnore]
        public virtual List<Command> Commands { get; set; }

        [JsonIgnore]
        [ValidateNever] 
        public virtual Client Client { get; set; }

        public void IncreaseMoney(double gainedAmount) { this.Money += gainedAmount; }

        public void DecreaseMoney(double removedAmount) { this.Money += removedAmount; }
    }
}