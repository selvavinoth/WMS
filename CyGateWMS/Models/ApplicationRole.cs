using Microsoft.AspNetCore.Identity;
using System;

namespace CyGateWMS.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string IPAddress { get; set; }
        public bool IsActive { get; set; }
    }
}
