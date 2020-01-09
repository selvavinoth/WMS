using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CyGateWMS.Models;
using Microsoft.AspNetCore.Authorization;

namespace CyGateWMS.Controllers
{
    [Authorize]
    public class TypeController : Controller
    {
        private readonly CygateWMSContext _context;

        public TypeController(CygateWMSContext context)
        {
            _context = context;
        }

        // GET: CustomerCatagories
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Type.ToListAsync());
        }

        // GET: CustomerCatagories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var type = await _context.Type
                .FirstOrDefaultAsync(m => m.TypeId == id);
            if (type == null)
            {
                return NotFound();
            }

            return View(type);
        }

        // GET: CustomerCatagories/Create
        [Authorize(Constants.ADMIN)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: CustomerCatagories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Create(Models.Type type)
        {
            if (ModelState.IsValid)
            {
                type.IsActive = true;
                type.CreatedOn = DateTime.Now;
                _context.Add(type);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(type);
        }

        // GET: CustomerCatagories/Edit/5
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var type = await _context.Type.FindAsync(id);
            if (type == null)
            {
                return NotFound();
            }
            return View(type);
        }

        // POST: CustomerCatagories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Edit(int id, Models.Type type)
        {
            if (id != type.TypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var _type = await _context.Type.FindAsync(id);
                    _type.ModifiedOn = DateTime.Now;
                    _type.TypeName = type.TypeName;
                    _type.FilterValue = type.FilterValue;
                    _type.IsActive = type.IsActive;
                    _context.Update(_type);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerCatagoriesExists(type.TypeId))
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
            return View(type);
        }

        // GET: CustomerCatagories/Delete/5
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customerCatagories = await _context.Type
                .FirstOrDefaultAsync(m => m.TypeId == id);
            if (customerCatagories == null)
            {
                return NotFound();
            }

            return View(customerCatagories);
        }

        // POST: CustomerCatagories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var type = await _context.Type.FindAsync(id);
            _context.Type.Remove(type);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerCatagoriesExists(int id)
        {
            return _context.Type.Any(e => e.TypeId == id);
        }
    }
}
