using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CyGateWMS.Models
{
    public class Roster
    {
        public Roster()
        {
            RosterShifts = new List<SelectListItem>();
        }
        [Key]
        public int RosterId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }

        [ForeignKey("UserId")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime Date { get; set; }        
        public RosterShift RosterShift { get; set; }
        public bool IsSelected { get; set; }
        [NotMapped]
        public string RosterShiftsId { get; set; }

        [NotMapped]
        public List<SelectListItem> RosterShifts { get; set; }
    }
}
