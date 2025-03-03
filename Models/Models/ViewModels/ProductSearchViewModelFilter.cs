using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models.ViewModels
{
    public class ProductSearchViewModelFilter
    {
        [Display(Name = "SearchNameText")] public string? SearchNameText { get; set; }
        [Range(3, 30, ErrorMessage = "{0} should be between {1} and {2}")]
        public int SelectedPageSize { get; set; }
        [Display(Name = "SelectedPageIndex")]
        public int SelectedPageIndex { get; set; }

        [Display(Name = "SelectedCategoryId")] 
        public int? SelectedCategoryId { get; set; }

        [DisplayName("SearchSellingPrice")]
        public double? SearchSellingPrice { get; set; }

        [DisplayName("SearchQuantity")]
        public int? SearchQuantity { get; set; }

        public bool SearchIsDeleted { get; set; } = false;

        [DisplayName("Description")]
        public string Description { get; set; }

        


        [DisplayName("Brand")]
        public string Brand { get; set; }

        [DisplayName("Retail Price")]
        public double RetailPrice { get; set; }

        public double PreviousSellingPrice { get; set; }

        [DisplayName("CategoryId")]
        public int CategoryId { get; set; }

        [DisplayName("Image")]
        public string PhotoURL { get; set; } = "";

        [DisplayName("Sort By")]
        public string SortBy { get; set; } // Column to sort by (e.g., "Price", "Quantity")

        [DisplayName("Sort Direction")]
        public string SortDirection { get; set; } // Sorting direction (e.g., "Asc", "Desc")


        public void VerifyProperties()
        {
            this.SelectedPageSize = this.SelectedPageSize == 0 ? 10 : this.SelectedPageSize;
            this.SelectedPageIndex = this.SelectedPageIndex;
            this.SearchNameText = this.SearchNameText?.Trim() == String.Empty ? null : this.SearchNameText;
            this.SelectedCategoryId = this.SelectedCategoryId == 0 ? null : this.SelectedCategoryId;

            // Set defaults for SearchQuantity and SearchSellingPrice if they are null
            this.SearchQuantity = this.SearchQuantity;
            this.SearchSellingPrice = this.SearchSellingPrice;
            this.SearchIsDeleted = this.SearchIsDeleted == true;

            // Default sorting
            this.SortBy = string.IsNullOrEmpty(this.SortBy) ? "Name" : this.SortBy;
            this.SortDirection = string.IsNullOrEmpty(this.SortDirection) ? "Asc" : this.SortDirection;
        }
    }
}
