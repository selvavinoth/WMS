using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CyGateWMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace CyGateWMS.Controllers
{
    [Authorize]
    public class ApplicationRoleController : Controller
    {
        private readonly CygateWMSContext _context;
        private readonly RoleManager<ApplicationRole> roleManager;

        public ApplicationRoleController(CygateWMSContext context, RoleManager<ApplicationRole> roleManager)
        {
            this._context = context;
            this.roleManager = roleManager;
        }

        // GET: Roles
        public async Task<IActionResult> Index()
        {
            return View(await roleManager.Roles.ToListAsync());
        }

        // GET: Roles/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // GET: Roles/Create
        [Authorize(Constants.ADMIN)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Create([Bind("Description,Name")] ApplicationRole role)
        {
            if (ModelState.IsValid)
            {
                role.IPAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                role.CreatedDate = DateTime.Now;
                role.IsActive = true;
                IdentityResult roleRuslt =  await roleManager.CreateAsync(role);
                if (roleRuslt.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }                
            }
            
            return View(role);
        }

        // GET: Roles/Edit/5
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // POST: Roles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Edit(string id, ApplicationRole role)
        {
            if (id != role.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var roleTemp = await roleManager.FindByIdAsync(id);
                    roleTemp.Description = role.Description;
                    roleTemp.IsActive = role.IsActive;
                    roleTemp.ModifiedOn = DateTime.Now;
                    await roleManager.UpdateAsync(roleTemp);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (roleManager.FindByIdAsync(role.Id) == null)
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
            return View(role);
        }

        // GET: Roles/Delete/5
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Constants.ADMIN)]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            await roleManager.DeleteAsync(role);
            return RedirectToAction(nameof(Index));
        }
        
    }
}
