using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CyGateWMS.Models;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace CyGateWMS.Controllers
{
    [Authorize]
    public class AllowanceTypesController : Controller
    {
        private readonly CygateWMSContext _context;

        public AllowanceTypesController(CygateWMSContext context)
        {
            _context = context;
        }

        // GET: AllowanceTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.AllowanceType.ToListAsync());
        }

        // GET: AllowanceTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var allowanceType = await _context.AllowanceType
                .FirstOrDefaultAsync(m => m.AllowanceTypeId == id);
            allowanceType.catagoriesList = !string.IsNullOrEmpty(allowanceType.Categories) ? JsonConvert.DeserializeObject<List<Category>>(allowanceType.Categories) : null;
            if (allowanceType == null)
            {
                return NotFound();
            }

            return View(allowanceType);
        }

        // GET: AllowanceTypes/Create
        public IActionResult Create()
        {
            AllowanceType allowanceType = new AllowanceType();
            allowanceType.catagoriesList = _context.Category.ToList();
            return View(allowanceType);
        }

        // POST: AllowanceTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Create(AllowanceType allowanceType)
        {
            if (ModelState.IsValid)
            {
                allowanceType.CreatedOn = DateTime.Now;
                allowanceType.IsActive = true;
                allowanceType.Categories = JsonConvert.SerializeObject(allowanceType.catagoriesList.Where(e => e.IsSelected));
                _context.Add(allowanceType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(allowanceType);
        }

        // GET: AllowanceTypes/Edit/5
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var allowanceType = await _context.AllowanceType.FindAsync(id);

            allowanceType.catagoriesList = _context.Category.ToList();

            List<Category> catagoriesList = !string.IsNullOrEmpty(allowanceType.Categories) ? JsonConvert.DeserializeObject<List<Category>>(allowanceType.Categories): null;

            if(catagoriesList !=null)
            {
                allowanceType.catagoriesList.ForEach(model =>
                {
                    Category catagory = catagoriesList.Where(e=>e.CategoryId == model.CategoryId).FirstOrDefault();
                    if(catagory != null)
                    {
                        model.IsSelected = catagory.IsSelected;
                    }

                });
            }
            if (allowanceType == null)
            {
                return NotFound();
            }
            return View(allowanceType);
        }

        // POST: AllowanceTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Edit(int id, AllowanceType allowanceType)
        {
            if (id != allowanceType.AllowanceTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var _allowanceType =await _context.AllowanceType.FindAsync(id);
                    _allowanceType.ModifiedOn = DateTime.Now;
                    _allowanceType.AllowanceTypeName = allowanceType.AllowanceTypeName;
                    _allowanceType.AllowanceTypePrice = allowanceType.AllowanceTypePrice;
                    _allowanceType.IsActive = allowanceType.IsActive;
                    _allowanceType.Categories = JsonConvert.SerializeObject(allowanceType.catagoriesList.Where(e => e.IsSelected));

                    _context.Update(_allowanceType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AllowanceTypeExists(allowanceType.AllowanceTypeId))
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
            return View(allowanceType);
        }

        // GET: AllowanceTypes/Delete/5
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var allowanceType = await _context.AllowanceType
                .FirstOrDefaultAsync(m => m.AllowanceTypeId == id);
            allowanceType.catagoriesList = !string.IsNullOrEmpty(allowanceType.Categories) ? JsonConvert.DeserializeObject<List<Category>>(allowanceType.Categories) : null;
            if (allowanceType == null)
            {
                return NotFound();
            }

            return View(allowanceType);
        }

        // POST: AllowanceTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var allowanceType = await _context.AllowanceType.FindAsync(id);
            _context.AllowanceType.Remove(allowanceType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AllowanceTypeExists(int id)
        {
            return _context.AllowanceType.Any(e => e.AllowanceTypeId == id);
        }
    }
}
