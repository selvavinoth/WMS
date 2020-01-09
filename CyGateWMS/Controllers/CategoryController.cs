using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CyGateWMS.Models;
using Microsoft.AspNetCore.Authorization;
using CyGateWMS.ViewModels;

namespace CyGateWMS.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly CygateWMSContext _context;        
        public CategoryController(CygateWMSContext context)
        {
            _context = context;
        }

        // GET: CustomerTypes
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Category.ToListAsync());
        }

        // GET: CustomerTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customerType = await _context.Category
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (customerType == null)
            {
                return NotFound();
            }

            return View(customerType);
        }

        // GET: CustomerTypes/Create
        [Authorize(Constants.ADMIN)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: CustomerTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Create(Category department)
        {
            if (ModelState.IsValid)
            {
                department.CreatedOn = DateTime.Now;
                department.IsActive = true;
                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // GET: CustomerTypes/Edit/5
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Category.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        // POST: CustomerTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Edit(int id, Category department)
        {
            if (id != department.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var _department = await _context.Category.FindAsync(id);
                    _department.CategoryName = department.CategoryName;
                    _department.ModifiedOn = DateTime.Now;
                    _department.IsActive = department.IsActive;
                    _context.Update(_department);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerTypeExists(department.CategoryId))
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
            return View(department);
        }

        // GET: CustomerTypes/Delete/5
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customerType = await _context.Category
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (customerType == null)
            {
                return NotFound();
            }

            return View(customerType);
        }

        // POST: CustomerTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customerType = await _context.Category.FindAsync(id);
            _context.Category.Remove(customerType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerTypeExists(int id)
        {
            return _context.Category.Any(e => e.CategoryId == id);
        }
    }
}
