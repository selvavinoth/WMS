using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CyGateWMS.ViewModels
{
    public class UserViewModel
    {
        public string UserId { get; set; }
        [Required]
        public string UserName { get; set; }
        public string EmployeeId { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm password doesn't match, Type again !")]
        public string ConfirmPassword { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        public List<SelectListItem> ApplicationRoles { get; set; }
        [Display(Name = "Role")]
        public string ApplicationRoleId { get; set; }
        public List<SelectListItem> Categories { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public bool IsRegularShift { get; set; }
        public bool IsCab { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
