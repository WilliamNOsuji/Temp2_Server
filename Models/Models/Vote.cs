using LapinCouvert.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Models.Models
{
    public class Vote
    {
        public int Id { get; set; }

        public int SuggestedProductId { get; set; }
        [JsonIgnore]
        public virtual SuggestedProduct SuggestedProduct { get; set; }

        public int ClientId { get; set; }
        [JsonIgnore]
        public virtual Client Client { get; set; }

        public bool IsFor { get; set; }
    }
}
