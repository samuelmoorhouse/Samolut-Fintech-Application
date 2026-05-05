using System.ComponentModel.DataAnnotations;

namespace Samolut_Fintech_Application.Models.Transfers;

public class AddBankModel
{
    [Key] 
    public int BANK_ID { get; set; }
}