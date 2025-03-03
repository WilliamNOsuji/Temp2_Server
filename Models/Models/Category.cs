using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Win32;
using System.Text.Json.Serialization;

namespace LapinCouvert.Models
{
    public class Category
    {
        public Category() { }

        public const int FROZEN_ID = 1;
        public const int DAIRY_ID = 2;
        public const int CRACKERS_ID = 3;
        public const int BUISCUIT_ID = 4;
        public const int LEGUMINEUSE_ID = 5;
        public const int BEVERAGES_ID = 6;
        public const int SNACKS_ID = 7;
        public const int CANNED_GOODS_ID = 8;
        public const int BAKERY_ID = 9;
        public const int MEAT_ID = 10;
        public const int SEAFOOD_ID = 11;
        public const int CONDIMENTS_ID = 12;
        public const int PASTA_ID = 13;
        public const int CEREAL_ID = 14;
        public const int FRUITS_ID = 15;
        public const int VEGETABLES_ID = 16;
        public const int NON_SPECIFIED = 17;

        public int Id { get; set; }
        public string Name { get; set; }

        // A la place d'avoir une liste de produit dans category, on va trouver ceux qui font partie
        // de la category selon le categoryId dans produit - William
        //[JsonIgnore]
        //public virtual List<Product> Products { get; set; }
    }
}
