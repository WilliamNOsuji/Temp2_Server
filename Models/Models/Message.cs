using LapinCouvert.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string? MessageText { get; set; }
        public string? Emoji { get; set; }
        public string? Photo { get; set; }
        public Client ClientSender { get; set; }
        
        //public Message GetMessageById(int messageId)
        //{
        //    return ;
        //}

        public Client GetCurrentSender()
        {
            return this.ClientSender;
        }
    }
}
