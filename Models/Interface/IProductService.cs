using LapinCouvert.Models;
using Microsoft.AspNetCore.Http;
using Models.Models.ViewModels;
using NuGet.DependencyResolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Interface
{
    public interface IProductService : IServiceBaseAsync<Product>
    {
        public Task<ProductSearchViewModel> GetAllAsync(ProductSearchViewModelFilter filter);
        public Task<Product> CreateWithFilePhotoAsync(Product model, IFormCollection form);
        public Task EditWithFilePhotoAsync(Product model, IFormCollection form);
        public Task<Product> CreateWithURLPhotoAsync(Product model);
        public Task EditWithURLPhotoAsync(Product model);
        public void SoftDeleteProduct(Product product);
    }
}
