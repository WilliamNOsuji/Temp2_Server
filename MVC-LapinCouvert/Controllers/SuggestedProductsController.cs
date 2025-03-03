using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LapinCouvert.Models;
using MVC_LapinCouvert.Data;
using Microsoft.AspNetCore.Authorization;

namespace MVC_LapinCouvert.Controllers
{
    [Authorize(Roles = "admin")]
    public class SuggestedProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuggestedProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SuggestedProducts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.SuggestedProducts;
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: SuggestedProducts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var suggestedProduct = await _context.SuggestedProducts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (suggestedProduct == null)
            {
                return NotFound();
            }

            return View(suggestedProduct);
        }

        // GET: SuggestedProducts/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: SuggestedProducts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Brand,CategoryId,Photo,VotesFor,VotesAgainst")] SuggestedProduct suggestedProduct)
        {
            if (ModelState.IsValid)
            {
                _context.Add(suggestedProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View(suggestedProduct);
        }

        // GET: SuggestedProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var suggestedProduct = await _context.SuggestedProducts.FindAsync(id);
            if (suggestedProduct == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View(suggestedProduct);
        }

        // POST: SuggestedProducts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Brand,CategoryId,Photo,VotesFor,VotesAgainst,FinishDate")] SuggestedProduct suggestedProduct)
        {
            if (id != suggestedProduct.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
	                if (suggestedProduct.FinishDate.Kind == DateTimeKind.Unspecified)
	                {
		                suggestedProduct.FinishDate = DateTime.SpecifyKind(suggestedProduct.FinishDate, DateTimeKind.Utc)
			                .ToUniversalTime();
	                }
					_context.Update(suggestedProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SuggestedProductExists(suggestedProduct.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View(suggestedProduct);
        }

        // GET: SuggestedProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var suggestedProduct = await _context.SuggestedProducts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (suggestedProduct == null)
            {
                return NotFound();
            }

            return View(suggestedProduct);
        }

        // POST: SuggestedProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var suggestedProduct = await _context.SuggestedProducts.FindAsync(id);
            if (suggestedProduct != null)
            {
                _context.SuggestedProducts.Remove(suggestedProduct);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SuggestedProductExists(int id)
        {
            return _context.SuggestedProducts.Any(e => e.Id == id);
        }
    }
}
