using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Models.Models;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace LapinCouvert.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Le champ 'Nom' est requis")]
        [DisplayName("Nom")]
        [MaxLength(50, ErrorMessage ="Le champ 'Nom' ne peut contenir que 50 caractères")]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Le champ 'Quantité' est requis")]
        [DisplayName("Quantité")]
        [Range(0, int.MaxValue, ErrorMessage = "Le champ 'Quantité' doit avoir une valeur supérieur ou égale à 0")]
        public int Quantity { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Le champ 'Description' est requis")]
        [DisplayName("Description")]
        [MaxLength(200, ErrorMessage = "Le champ 'Description' ne peut contenir que 200 caractères")]
        [MinLength(5, ErrorMessage = "Le champ 'Description' doit contenir plus de 5 caractères")]
        public string Description { get; set; }

        [DisplayName("Marque")]
        public string? Brand { get; set; }

        [DisplayName("Prix d'achat")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Le champ 'Prix d'achat' doit être sous forme de montant d'argent")]
        public double RetailPrice { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Le champ 'Prix de vente' est requis")]
        [DisplayName("Prix de vente")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Le champ 'Prix de vente' doit être sous forme de montant d'argent")]
        public double SellingPrice { get; set; }
        
        [DisplayName("Catégorie")]
        public int? CategoryId { get; set; }

        [DisplayName("Photo")]
        public string? Photo { get; set; } = "";

        public bool IsDeleted { get; set; } = false;

        [JsonIgnore]
        [ValidateNever]
        public virtual List<CommandProduct> CommandProducts { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual List<CartProducts> CartProducts { get; set; }

        [ValidateNever]
        public virtual Category Category { get; set; }

        public bool IsAvailable()
        {
            if(this.Quantity == 0)
            {
                return false;
            }
            return true;
        }

        public void IncreaseQuantity(int quantityAdded)
        {
            this.Quantity += quantityAdded;
        }

        public void DecreaseQuantity(int quantityRemoved)
        {
            this.Quantity -= quantityRemoved;
        }


        //public void SetPrice(double newPrice)
        //{
        //    this.PreviousSellingPrice = this.SellingPrice;
        //    this.SellingPrice = newPrice;

        //    if(PreviousSellingPrice > SellingPrice)
        //    {
        //        IsDiscount = true;
        //    }
        //    else
        //    {
        //        IsDiscount = false;
        //    }
        //}
    }
}
