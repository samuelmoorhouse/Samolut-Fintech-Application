using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Samolut_Fintech_Application.Models.DatabaseModels
{
    [Table("Customer")]

    public class Customer
    {   
        
        [Key] //make c sharp know its the primary key
        public int CUSTOMER_ID { get; set; }
        [Required(ErrorMessage = "First name is required, please enter your first name.")]
        public string FIRST_NAME { get; set; }
        public string? MIDDLE_NAME { get; set; } //use questionmakr so c sharp knows it can be empty
        [Required(ErrorMessage = "Last name is required, please enter your last name.")]
        public string LAST_NAME { get; set; }
        [Required(ErrorMessage = "Phone number is required, please enter a valid phone number.")]
        [MinLength(10, ErrorMessage = "Please enter a valid phone number - Length must be 10/11!")]
        [MaxLength(11, ErrorMessage = "Please enter a valid phone number - Length must be 10/11!")]
        public string PHONE_NUMBER { get; set; }
        [Required(ErrorMessage = "Password is required, please enter a valid password.")]
        [MinLength(8, ErrorMessage = "Please enter a valid password - Length must be at least 8 characters long!")]
        public string PASSWORD { get; set; }
        
        //added for the suspended account stuff
        [Required]
        public int SUSPENDED { get; set; }

    }
}
