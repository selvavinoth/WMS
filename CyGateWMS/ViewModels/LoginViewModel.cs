using System.ComponentModel.DataAnnotations;

namespace CyGateWMS.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
        public string Code { get; set; }
    }
}
