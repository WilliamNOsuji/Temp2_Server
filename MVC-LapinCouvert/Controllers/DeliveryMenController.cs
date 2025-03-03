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
    public class DeliveryMenController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeliveryMenController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DeliveryMen
        public async Task<IActionResult> Index()
        {
            return View(await _context.DeliveryMans.ToListAsync());
        }

        // GET: DeliveryMen/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deliveryMan = await _context.DeliveryMans
                .FirstOrDefaultAsync(m => m.Id == id);
            if (deliveryMan == null)
            {
                return NotFound();
            }

            return View(deliveryMan);
        }

        // GET: DeliveryMen/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DeliveryMen/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Money,ImageURL,IsActive")] DeliveryMan deliveryMan)
        {
            if (ModelState.IsValid)
            {
                _context.Add(deliveryMan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(deliveryMan);
        }

        // GET: DeliveryMen/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deliveryMan = await _context.DeliveryMans.FindAsync(id);
            if (deliveryMan == null)
            {
                return NotFound();
            }
            return View(deliveryMan);
        }

        // POST: DeliveryMen/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Money,ImageURL,IsActive")] DeliveryMan deliveryMan)
        {
            if (id != deliveryMan.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(deliveryMan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeliveryManExists(deliveryMan.Id))
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
            return View(deliveryMan);
        }

        // GET: DeliveryMen/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deliveryMan = await _context.DeliveryMans
                .FirstOrDefaultAsync(m => m.Id == id);
            if (deliveryMan == null)
            {
                return NotFound();
            }

            return View(deliveryMan);
        }

        // POST: DeliveryMen/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deliveryMan = await _context.DeliveryMans.FindAsync(id);
            if (deliveryMan != null)
            {
                _context.DeliveryMans.Remove(deliveryMan);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DeliveryManExists(int id)
        {
            return _context.DeliveryMans.Any(e => e.Id == id);
        }
    }
}
