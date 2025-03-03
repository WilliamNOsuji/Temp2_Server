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
    public class CommandProduct
    {

        public CommandProduct() { }

        public CommandProduct(CartProducts p, int commandId)

        {
            CommandId = commandId;
            ProductId = p.ProductId;
            Name = p.Name;
            Price = p.Prix;
            Quantity = p.Quantity;
            MaxQuantity = p.Quantity;
        }

        public int Id { get; set; }
        public int CommandId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }

        public bool isOutofStock
        {
            get { return MaxQuantity <= 0; }
            set { }
        }
        public bool isOutofBound
        {
            get { return Quantity > MaxQuantity; }
            set { }
        }

        public int MaxQuantity { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual Command Command { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual Product Product { get; set; }
    }
}
