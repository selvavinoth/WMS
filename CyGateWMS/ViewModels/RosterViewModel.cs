using CyGateWMS.Models;
using System;
using System.Collections.Generic;

namespace CyGateWMS.ViewModels
{
    public class RosterViewModel
    {
        public RosterViewModel()
        {
            Rosters = new List<Roster>();
            Categories = new List<Category>();
        }
        public List<DateTime> Dates { get; set; }
        public int NumberOfDays { get; set; }
        public string UserID { get; set; }
        public List<Category> Categories { get; set; }
        public List<ApplicationUser> Users { get; set; }
        public List<ApplicationUser> AllUsers { get; set; }
        public List<Roster> Rosters { get; set; }
        public List<RosterShift> RosterShifts { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime Month { get; set; }
        public string RosterVendorString { get; set; }

    }
}
