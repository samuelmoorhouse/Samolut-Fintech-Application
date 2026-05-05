using System.ComponentModel.DataAnnotations;

namespace Samolut_Fintech_Application.Models.Transfers;

public class AddCurrencyModel
{
    [Key] 
    public int CURRENCY_ACCOUNT_ID { get; set; }
}