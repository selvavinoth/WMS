using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CyGateWMS.Models;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;

namespace CyGateWMS.Controllers
{
    [Authorize]
    public class HolidaysController : Controller
    {
        private readonly CygateWMSContext _context;

        public HolidaysController(CygateWMSContext context)
        {
            _context = context;
        }

        // GET: Holidays
        public async Task<IActionResult> Index()
        {
            return View(await _context.Holiday.ToListAsync());
        }

        // GET: Holidays/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var holiday = await _context.Holiday
                .FirstOrDefaultAsync(m => m.ID == id);
            if (holiday == null)
            {
                return NotFound();
            }

            return View(holiday);
        }

        // GET: Holidays/Create
        [Authorize(Roles = Constants.ADMIN)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Holidays/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constants.ADMIN)]
        public async Task<IActionResult> Create([Bind("Date,Description")] Holiday holiday)
        {
            if (ModelState.IsValid)
            {
                holiday.IsActive = true;
                holiday.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(holiday.Date.Month);
                holiday.Day = CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(holiday.Date.DayOfWeek);
                holiday.Year = holiday.Date.Year;

                _context.Add(holiday);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(holiday);
        }

        // GET: Holidays/Edit/5
        [Authorize(Roles = Constants.ADMIN)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var holiday = await _context.Holiday.FindAsync(id);
            if (holiday == null)
            {
                return NotFound();
            }
            return View(holiday);
        }

        // POST: Holidays/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constants.ADMIN)]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Month,Date,Day,Year,Description,IsActive")] Holiday holiday)
        {
            if (id != holiday.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    holiday.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(holiday.Date.Month);
                    holiday.Day = CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(holiday.Date.DayOfWeek);
                    holiday.Year = holiday.Date.Year;                    
                    _context.Update(holiday);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HolidayExists(holiday.ID))
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
            return View(holiday);
        }

        // GET: Holidays/Delete/5
        [Authorize(Roles = Constants.ADMIN)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var holiday = await _context.Holiday
                .FirstOrDefaultAsync(m => m.ID == id);
            if (holiday == null)
            {
                return NotFound();
            }

            return View(holiday);
        }

        // POST: Holidays/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constants.ADMIN)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var holiday = await _context.Holiday.FindAsync(id);
            _context.Holiday.Remove(holiday);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HolidayExists(int id)
        {
            return _context.Holiday.Any(e => e.ID == id);
        }
    }
}
