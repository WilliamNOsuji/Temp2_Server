using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Models.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace LapinCouvert.Models
{
    public class Cart
    {
        public int Id { get; set; }

        [ForeignKey("Client")]
        public int ClientId { get; set; }

        public virtual List<CartProducts> CartProducts { get; set; }

        public virtual Client Client { get; set; }

        public void AddProduct(int productId, int quantity)
        {
            CartProducts c = new CartProducts();

            c.Quantity = quantity;
            c.ProductId = productId;

            CartProducts.Add(c);
        }

        public void ClearCart()
        {
            CartProducts.Clear();
        }
    }
}
