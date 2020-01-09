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
    public class RosterShiftsController : Controller
    {
        private readonly CygateWMSContext _context;

        public RosterShiftsController(CygateWMSContext context)
        {
            _context = context;
        }

        // GET: RosterShifts
        public async Task<IActionResult> Index()
        {
            return View(await _context.RosterShifts.ToListAsync());
        }

        // GET: RosterShifts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rosterShift = await _context.RosterShifts
                .FirstOrDefaultAsync(m => m.RosterShiftId == id);
            if (rosterShift == null)
            {
                return NotFound();
            }

            return View(rosterShift);
        }

        // GET: RosterShifts/Create
        [Authorize(Constants.ADMIN)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: RosterShifts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Create(RosterShift rosterShift)
        {
            if (ModelState.IsValid)
            {
                rosterShift.CreatedOn = DateTime.Now;
                rosterShift.IsActive = true;
                _context.Add(rosterShift);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rosterShift);
        }

        // GET: RosterShifts/Edit/5
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rosterShift = await _context.RosterShifts.FindAsync(id);
            if (rosterShift == null)
            {
                return NotFound();
            }
            return View(rosterShift);
        }

        // POST: RosterShifts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Edit(int id, [Bind("RosterShiftId,RosterShiftName,IsActive")] RosterShift rosterShift)
        {
            if (id != rosterShift.RosterShiftId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var _rosterShift = _context.RosterShifts.Single(e => e.RosterShiftId == id);
                    _rosterShift.IsActive = rosterShift.IsActive;
                    _rosterShift.RosterShiftName = rosterShift.RosterShiftName;
                    _rosterShift.ModifiedOn = DateTime.Now;

                    _context.Update(_rosterShift);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RosterShiftExists(rosterShift.RosterShiftId))
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
            return View(rosterShift);
        }

        // GET: RosterShifts/Delete/5
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rosterShift = await _context.RosterShifts
                .FirstOrDefaultAsync(m => m.RosterShiftId == id);
            if (rosterShift == null)
            {
                return NotFound();
            }

            return View(rosterShift);
        }

        // POST: RosterShifts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rosterShift = await _context.RosterShifts.FindAsync(id);
            _context.RosterShifts.Remove(rosterShift);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RosterShiftExists(int id)
        {
            return _context.RosterShifts.Any(e => e.RosterShiftId == id);
        }
    }
}
