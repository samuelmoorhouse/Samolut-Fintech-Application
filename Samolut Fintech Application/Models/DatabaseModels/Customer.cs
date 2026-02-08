using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Samolut_Fintech_Application.Models.DatabaseModels
{
    [Table("Customer")]

    public class Customer
    {
        [Key] //make c sharp know its the primary key
        public int ID { get; set; }
        [Required]
        public string FIRST_NAME { get; set; }
        public string? MIDDLE_NAME { get; set; } //use questionmakr so c sharp knows it can be empty
        [Required]
        public string LAST_NAME { get; set; }
        [Required]
        public int PHONE_NUMBER { get; set; }
        [Required]
        public string PASSWORD { get; set; }

    }
}
