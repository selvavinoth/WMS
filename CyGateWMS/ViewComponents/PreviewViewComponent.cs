using CyGateWMS.Models;
using CyGateWMS.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CyGateWMS.ViewComponents
{
    public class PreviewViewComponent : ViewComponent
    {
        private readonly CygateWMSContext context;
        private readonly UserManager<ApplicationUser> userManager;
        public PreviewViewComponent(CygateWMSContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public IViewComponentResult Invoke(RosterViewModel model)
        {
            model.Rosters.Clear();
            foreach (ApplicationUser user in model.AllUsers)
            {
                List<Roster> roster = context.Rosters.Include(e => e.RosterShift).Where(e => e.UserId == user.Id && e.Date.Month == model.Month.Month).ToList();
                if (roster.Count <= 0)
                {
                    foreach (var date in model.Dates)
                    {
                        var regularShift = model.RosterShifts.Where(e => e.RosterShiftName == Constants.R).FirstOrDefault();
                        if(date.DayOfWeek.ToString() == Constants.SATURDAY || date.DayOfWeek.ToString() == Constants.SUNDAY)
                        {
                            regularShift = model.RosterShifts.Where(e => e.RosterShiftName == Constants.OFF).FirstOrDefault();
                        }
                        context.Rosters.Add(new Roster
                        {
                            Date = date,
                            CreatedOn = DateTime.Now,
                            IsActive = true,
                            IsSelected = false,
                            User = user,
                            RosterShift = user.IsRegularShift ? regularShift : null
                        });
                    }
                    context.SaveChanges();
                    model.Rosters.AddRange(context.Rosters.Include(e => e.RosterShift).Where(e => e.UserId == user.Id && e.Date.Month == DateTime.Now.Month).ToList());
                }
                else
                {
                    model.Rosters.AddRange(roster);
                }
            }
                return View(model);
        }
        private List<DateTime> GetDates(int year, int month)
        {
            return Enumerable.Range(1, DateTime.DaysInMonth(year, month))  // Days: 1, 2 ... 31 etc.
                             .Select(day => new DateTime(year, month, day)) // Map each day to a date
                             .ToList(); // Load dates into a list
        }
    }
}
