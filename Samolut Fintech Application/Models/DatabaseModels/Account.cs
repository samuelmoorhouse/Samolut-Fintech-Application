using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Samolut_Fintech_Application.Models.DatabaseModels
{
    [Table("Account")]

    public class Account
    {
        [Key] //the Key and required stuff is made work by using data annotations above 
        public int ACCOUNT_ID { get; set; }
        [Required]
        public int CUSTOMER_ID { get; set; }
        [Required]
        public int COUNTRY_CURRENCY_ID { get; set; }
        [Required]
        public double ACCOUNT_BALANCE { get; set; }
        [Required]
        public int ACCOUNT_TYPE_ID { get;set; }

    }
}
