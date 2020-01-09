using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CyGateWMS.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string EmployeeId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifieddOn { get; set; }
        public bool IsAddressFilled { get; set; }

        [Required]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
        public bool IsActive { get; set; }
        public bool IsRegularShift { get; set; }
        public bool IsCab { get; set; }
    }
}
