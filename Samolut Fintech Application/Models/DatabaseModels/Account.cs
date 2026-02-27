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


        //from my sql the keys what i need tro add 
        //FOREIGN KEY(CUSTOMER_ID) REFERENCES Customer(CUSTOMER_ID),
        //FOREIGN KEY(COUNTRY_CURRENCY_ID) REFERENCES CurrentCurrency(COUNTRY_CURRENCY_ID),
        //FOREIGN KEY(ACCOUNT_TYPE_ID) REFERENCES AccountType(ACCOUNT_TYPE_ID)

        [ForeignKey("COUNTRY_CURRENCY_ID")]
        public CurrentCurrency CurrencyIdForeignKey { get; set; }

        [ForeignKey("CUSTOMER_ID")]
        public Customer CustomerIdForeignKey { get; set; }

        [ForeignKey("ACCOUNT_TYPE_ID")]
        public AccountType AccountTypeIdForeignKey { get; set; }

    }
}
