using CyGateWMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CyGateWMS.ViewModels
{
    public class ReportGenerationViewModel
    {
        public List<Report> Reports { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string ShiftID { get; set; }
        public Shift Shift { get; set; }
        public List<SelectListItem> Shifts { get; set; }
        public string CategoryID { get; set; }
        public List<SelectListItem> Categories { get; set; }
        public string TypeID { get; set; }
        public List<SelectListItem> Types { get; set; }
        public Models.Type Type { get; set; }
        public string UserID { get; set; }
        public List<SelectListItem> Users { get; set; }
        public List<ApplicationUser> User { get; set; }
        public string OnCallEscalateID { get; set; }
        public List<SelectListItem> OnCallEscalaties { get; set; }
        public OnCallEscalate OnCallEscalate { get; set; }
    }
}
