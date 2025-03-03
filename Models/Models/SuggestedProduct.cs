using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Models.Models;

namespace LapinCouvert.Models
{
    public class SuggestedProduct
    {
        public SuggestedProduct()
        {
            FinishDate = DateTime.UtcNow.AddDays(7);
        }

        public int Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Le champ 'Nom' est requis")]
        [DisplayName("Nom")]
        [MaxLength(50, ErrorMessage = "Le champ 'Nom' ne peut contenir que 50 caractères")]
        public string Name { get; set; }

        [DisplayName("Photo")]
        public string? Photo { get; set; } = "";

        [DisplayName("Date de fin")]
        public DateTime FinishDate { get; set; }

        //[JsonIgnore]
        //[ValidateNever]
        //[DisplayName("Votes pour")]
        //public virtual List<Client> ForClients { get; set; }
        //[JsonIgnore]
        //[ValidateNever]
        //[DisplayName("Votes contre")]
        //public virtual List<Client> AgainstClients { get; set; } 

        public virtual List<Vote> Votes { get; set; } = new List<Vote>();
    }
}
