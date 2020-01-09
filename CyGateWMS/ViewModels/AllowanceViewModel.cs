using CyGateWMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace CyGateWMS.ViewModels
{
    public class AllowanceViewModel
    {
        public int AllowanceID { get; set; }
        public string Comments { get; set; }
        public decimal Price { get; set; }
        public string ApprovedStatusId { get; set; }
        public List<SelectListItem> ApprovedStatus { get; set; }
        public string AllowanceTypeId { get; set; }
        public List<SelectListItem> AllowanceTypes { get; set; }
        public string AssignedToId { get; set; }
        public List<SelectListItem> AssignedToItems { get; set; }
        public DateTime? AllowanceDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public ApplicationUser AssignedBy { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public Category AssignedCategory { get; set; }
        public AllowanceType AllowanceType { get; set; }
        public ApprovedStatus Status { get; set; }
        public List<string> AllownanceDates { get; set; }
        public string AllownanceDatesjsonResult { get; set; }

        public List<AllowanceType> AllowanceTypesDetails { get; set; }
        public int NumberOfDays { get; set; }

        public Boolean IsFromFilter { get; set; }

        public string CategoryID { get; set; }
        public List<SelectListItem> Categories { get; set; }
        public string UserID { get; set; }
        public List<SelectListItem> Users { get; set; }

        public string StatusD { get; set; }
        public List<SelectListItem> StatusList { get; set; }

        public DateTime Month { get; set; }
        public string MonthId { get; set; }
        public List<Allowance> ExistingAllowances { get; set; }
    }
}
