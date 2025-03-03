using Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.DTOs
{
    public class LoginSuccessDTO
    {
        public LoginSuccessDTO(string token, int clientId, string clientName, bool isDeliveryman, bool isActive)
        {
            Token = token;
            ClientId = clientId;
            ClientName = clientName;
            IsDeliveryMan = isDeliveryman;
            IsActive = isActive;
        }

        [Required]
        public string Token { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }

        public bool IsDeliveryMan { get; set; }

        public bool IsActive { get; set; }
    }
}
