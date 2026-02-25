using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Samolut_Fintech_Application.Models.LoginSignUpModels
{
    public class LoginModel
    {
        [Required]
        [MinLength(10, ErrorMessage = "Please enter a valid phone number - Length must be 10/11!")]
        [MaxLength(11, ErrorMessage = "Please enter a valid phone number - Length must be 10/11!")]
        public string PHONE_NUMBER { get; set; }
        [Required]
        [MinLength(8, ErrorMessage = "Please enter a valid password - Length must be at least 8 characters long!")]
        public string PASSWORD { get; set; }

    }
}
