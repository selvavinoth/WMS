using CyGateWMS.Models;
using System.Collections.Generic;

namespace CyGateWMS.ViewModels
{
    public class AllowancesFilterViewModel
    {
        public List<AllowanceViewModel> AllowanceViewModelList { get; set; }
        public AllowanceViewModel AllowanceViewModel { get; set; }
        public List<ApplicationUser> Users { get; set; }
        public bool IsFromFilter { get; set; }
    }
}
