using Humanizer;
using LapinCouvert.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models.Models
{
    public class CartProducts
    {
        public CartProducts(Product p, int cartId)

        {
            ProductId = p.Id;
            CartId = cartId;
            Name = p.Name;
            ImageUrl = p.Photo;
            Prix = p.SellingPrice;
            Quantity = 1;
            MaxQuantity = p.Quantity;
        }


        public CartProducts() { }
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public double Prix { get; set; }
        public bool isOutofStock { 
            get { return MaxQuantity <= 0;  }
            set { } } 
        public bool isOutofBound {
            get { return Quantity > MaxQuantity; }
            set { } }
        public int MaxQuantity { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual Product Product { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual Cart Cart { get; set; }

        public void IncreaseQuantity()
        {
            this.Quantity++;
        }

        public void DecreaseQuantity()
        {
            this.Quantity--;
        }

        public double GetQuantityPrice()
        {
            return this.Product.SellingPrice * this.Quantity;
        }
    }
}
