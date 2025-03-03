using Microsoft.AspNetCore.Http.Connections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models.ViewModels
{
    public class ProductEditVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public string Brand { get; set; }
        public double RetailPrice { get; set; }
        public double SellingPrice { get; set; }
        public int CategoryId { get; set; }
        public string ImageUrl { get; set; } = "";

        public List<AvailableCategorie> AvailableCategories { get; set; } = new List<AvailableCategorie>();
    }
}
