using CyGateWMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CyGateWMS.ViewModels
{
    public class ReportViewModel
    {
        [Key]
        public int ReportID { get; set; }
        [Required]
        public string TicketNo { get; set; }
        [Required]
        public string Description { get; set; }

        public DateTime? ArrivalTime { get; set; }
        [Required]
        public DateTime? StartTime { get; set; }
        [Required]
        public string TimeSpent { get; set; }
        public DateTime? EndTime { get; set; }
        public string ShiftID { get; set; }
        public Shift Shift { get; set; }
        public List<SelectListItem> Shifts { get; set; }
        public string CategoryID { get; set; }
        public List<SelectListItem> Categories { get; set; }
        public string TypeID { get; set; }
        public List<SelectListItem> Types { get; set; }
        public List<Models.Type> FilterTypes { get; set; }
        public Models.Type Type { get; set; }
        public string UserID { get; set; }
        public ApplicationUser User { get; set; }
        public string OnCallEscalateID { get; set; }
        public List<SelectListItem> OnCallEscalates { get; set; }
        public OnCallEscalate OnCallEscalate { get; set; }
        public string UserName { get; set; }
        public string CategoryName { get; set; }
        public string ShiftName { get; set; }
        public string TypeName { get; set; }
        public string OnCallEscalateName { get; set; }

    }
}
