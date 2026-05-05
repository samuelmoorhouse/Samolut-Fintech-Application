using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Samolut_Fintech_Application.Models.DatabaseModels;

[Table("CurrencyAccounts")]
public class CurrencyAccounts
{
    [Key] 
    public int CURRENCY_ACCOUNT_ID { get; set; }
    [Required]
    public int COUNTRY_CURRENCY_ID { get; set; }
    [Required]
    public int ACCOUNT_TYPE_ID { get;set; }
    [Required]  
    public string? ACCOUNT_NAME { get; set; }

    [ForeignKey("COUNTRY_CURRENCY_ID")]
    public CurrentCurrency CurrencyIdForeignKey { get; set; }

    [ForeignKey("ACCOUNT_TYPE_ID")]
    public AccountType AccountTypeIdForeignKey { get; set; }

}