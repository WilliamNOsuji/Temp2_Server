using LapinCouvert.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Interface;
using NuGet.DependencyResolver;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Models.Models.ViewModels
{
    public class ProductSearchViewModel : ProductSearchViewModelFilter
    {
        public ProductSearchViewModel() { }

        public ProductSearchViewModel(ProductSearchViewModelFilter filter)
        {
            this.SearchNameText = filter.SearchNameText;
            this.SelectedPageSize = filter.SelectedPageSize;
            this.SelectedPageIndex = filter.SelectedPageIndex;
            this.SelectedCategoryId = filter.SelectedCategoryId;
            this.SearchSellingPrice = filter.SearchSellingPrice;
            this.SearchQuantity = filter.SearchQuantity;
            this.SearchIsDeleted = filter.SearchIsDeleted;

            this.SortBy = filter.SortBy;
            this.SortDirection = filter.SortDirection;
            this.VerifyProperties();
        }

        [Display(Name = "Items")] public IPaginatedList<Product> Items { get; set; }
        [Display(Name = "Categories")] public SelectList Categories { get; set; }
        [Display(Name = "AvailablePageSizes")] public SelectList AvailablePageSizes { get; set; }
        [Display(Name = "Aucun Résultas")]  public string NoResultsMessage { get; set; }

    }
}
