using System.ComponentModel.DataAnnotations;

namespace TaskMonitoringApp.Models.ViewModel
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "The length of {0} must be between {2} and {1} characters.")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "The length of {0} must be between {2} and {1} characters.")]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
