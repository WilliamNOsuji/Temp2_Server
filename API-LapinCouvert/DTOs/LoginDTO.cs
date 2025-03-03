using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.DTOs
{
    public class LoginDTO
    {
        [Required]
        public string Username { get; set; } = "";
        [Required] 
        public string Password { get; set; } = "";

        /// <summary>
        /// The following is a device token that will be everytime a user log's into his account
        /// The deviceToken will be saved in the database if the user is a deliveryMan
        /// In any other case the deviceToken should ignored
        /// </summary>
        public string? DeviceToken { get; set; }
    }
}
