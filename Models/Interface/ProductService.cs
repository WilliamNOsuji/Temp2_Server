using JuliePro.Utility;
using LapinCouvert.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models.Models.ViewModels;
using MVC_LapinCouvert.Data;
using NuGet.DependencyResolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Models.Interface.ServiceBaseEf;

namespace Models.Interface
{
    public class ProductService : ServiceBaseEF<Product>, IProductService
    {
        private IImageFileManager fileManager;

        public ProductService(ApplicationDbContext dbContext, IImageFileManager fileManager) : base(dbContext)
        {
            this.fileManager = fileManager;
        }

        public async Task<Product> CreateWithFilePhotoAsync(Product model, IFormCollection form)
        {
            model.Photo = await fileManager.UploadImage(form, false, null);
            return await base.CreateAsync(model);
        }

        public async Task<Product> CreateWithURLPhotoAsync(Product model)
        {
            return await base.CreateAsync(model);
        }

        public async Task EditWithFilePhotoAsync(Product model, IFormCollection form)
        {
            var old = await _dbContext.Products.Where(x => x.Id == model.Id).Select(x => x.Photo).FirstOrDefaultAsync();
            model.Photo = await fileManager.UploadImage(form, true, old);
            await this.EditAsync(model);
        }

        public async Task EditWithURLPhotoAsync(Product model)
        {
            //var old = await _dbContext.Products.Where(x => x.Id == model.Id).Select(x => x.Photo).FirstOrDefaultAsync();
            //model.Photo = await fileManager.UploadImage(form, true, old);
            await this.EditAsync(model);
        }

        public async Task<ProductSearchViewModel> GetAllAsync(ProductSearchViewModelFilter filter)
        {
            filter.VerifyProperties();

            var result = new ProductSearchViewModel(filter);

            var query = _dbContext.Products.AsQueryable();

            var products = await query.ToListAsync();

            if (!string.IsNullOrEmpty(filter.SearchNameText))
            {
                string[] names = filter.SearchNameText.ToLower().Split(' ');
                string searchName = names[0];
                query = query.Where(t => t.Name.ToLower().Contains(searchName));
            }

            // Filter by category
            if (filter.SelectedCategoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == filter.SelectedCategoryId.Value);
            }

            // Apply Quantity filter if not default
            if (filter.SearchQuantity.HasValue)
            {
                query = query.Where(p => p.Quantity <= filter.SearchQuantity);
            }

            // Apply Selling Price filter if not default
            if (filter.SearchSellingPrice.HasValue)
            {
                query = query.Where(p => p.SellingPrice <= filter.SearchSellingPrice);
            }

            // Apply IsDeleted filter if SearchIsDeleted is true
            if (filter.SearchIsDeleted)
            {
                query = query.Where(p => p.IsDeleted == true);
            }
            else
            {
                query = query.Where(p => p.IsDeleted == false);
            }

            // Apply sorting
            switch (filter.SortBy)
            {
                case "Price":
                    query = filter.SortDirection == "Asc"
                        ? query.OrderBy(p => p.SellingPrice)
                        : query.OrderByDescending(p => p.SellingPrice);
                    break;
                case "Quantity":
                    query = filter.SortDirection == "Asc"
                        ? query.OrderBy(p => p.Quantity)
                        : query.OrderByDescending(p => p.Quantity);
                    break;
                case "Name":
                default:
                    query = filter.SortDirection == "Asc"
                        ? query.OrderBy(p => p.Name)
                        : query.OrderByDescending(p => p.Name);
                    break;
            }

            result.Items = await query.ToPaginatedAsync(filter.SelectedPageIndex, filter.SelectedPageSize);

            result.AvailablePageSizes = new SelectList(new List<int>() { 12, 15, 20, 25 });
            result.Categories = new SelectList(await _dbContext.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");

            if (result.Items.Count == 0)
            {
                result.NoResultsMessage = "Aucun produit trouvé avec les critères de filtrage spécifiés.";
            }

            return result;
        }

        public async void SoftDeleteProduct(Product product)
        {
            //var product = await _dbContext.Products.FindAsync(id);
            if(product != null)
            {
                product.IsDeleted = true;
                _dbContext.Update(product);
                _dbContext.SaveChanges();
            }
        }
    }
}
