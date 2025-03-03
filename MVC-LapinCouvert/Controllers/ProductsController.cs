using LapinCouvert.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Models.ViewModels;
using MVC_LapinCouvert.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Models.Interface;
using Microsoft.AspNetCore.Mvc.Rendering;
using JuliePro.Utility;
using NuGet.DependencyResolver;
using Microsoft.AspNetCore.Authorization;


namespace MVC_LapinCouvert.Controllers
{
    [Authorize(Roles = "admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _serviceProduct;
        private readonly IServiceBaseAsync<Category> _categoryService;

        public ProductsController(ApplicationDbContext context, IProductService service, IServiceBaseAsync<Category> anotherService)
        {
            _context = context;
            _serviceProduct = service;
            _categoryService = anotherService;
        }

        // GET: Products
        public async Task<IActionResult> Index(ProductSearchViewModelFilter filter)
        {

            filter.SelectedPageIndex = filter.SelectedPageIndex;

            var products = await this._serviceProduct.GetAllAsync(filter);
            return View(products);
        }

        public async Task<IActionResult> Filter(ProductSearchViewModelFilter filter)
        {
            var viewModel = await _serviceProduct.GetAllAsync(filter);
            return View("Index", viewModel);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _serviceProduct.GetByIdAsync(id.Value);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

		// GET: Products/Create
		public async Task<IActionResult> Create(int? suggestedProductId)
		{
			var product = new Product();

			if (suggestedProductId.HasValue)
			{
				var suggestedProduct = await _context.SuggestedProducts.FindAsync(suggestedProductId.Value);
				if (suggestedProduct != null)
				{
					product.Name = suggestedProduct.Name;
					product.Photo = suggestedProduct.Photo;
				}
			}

			ViewData["CategoryId"] = new SelectList(await _categoryService.GetAllAsync(), "Id", "Name");
			return View(product);
		}

		// POST: Products/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromServices] IImageFileManager fileManager, Product product)
        {
            ModelState.Remove(nameof(product.Category));

            if (ModelState.IsValid)
            {
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Name.ToLower() == product.Name.ToLower());

                if (existingProduct != null)
                {
                    ModelState.AddModelError("Name", "Un produit avec ce nom existe déja.");
                }
                else
                {
                    if (HttpContext.Request.Form.Files.Count != 0)
                    {
                        await _serviceProduct.CreateWithFilePhotoAsync(product, HttpContext.Request.Form);
                    }
                    else
                    {
                        await _serviceProduct.CreateWithURLPhotoAsync(product);
                    }

                    return RedirectToAction(nameof(Index));
                }
            }

            // If there is an error, repopulate the category dropdown list and return the view
            ViewData["CategoryId"] = new SelectList(await _categoryService.GetAllAsync(), "Id", "Name");
            return View(product);
        }


        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _serviceProduct.GetByIdAsync(id.Value);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(await _categoryService.GetAllAsync(), "Id", "Name");
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromServices] IImageFileManager fileManager, int id, Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }
            ModelState.Remove(nameof(product.Category));

            if (ModelState.IsValid)
            {
                try
                {
                    if (HttpContext.Request.Form.Files.Count != 0)
                    {
                        await _serviceProduct.EditWithFilePhotoAsync(product, HttpContext.Request.Form);
                    }
                    else
                    {
                        await _serviceProduct.EditWithURLPhotoAsync(product);
                    }
                    
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _serviceProduct.ExistsAsync(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(await _categoryService.GetAllAsync(), "Id", "Name");
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _serviceProduct.GetByIdAsync(id.Value);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //await _serviceProduct.DeleteAsync(id);
            var product = await _serviceProduct.GetByIdAsync(id);
            _serviceProduct.SoftDeleteProduct(product);
            //product.IsDeleted = true;
            //_context.Update(product);
            //await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
