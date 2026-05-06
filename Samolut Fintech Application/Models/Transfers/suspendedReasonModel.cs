using System.ComponentModel.DataAnnotations;
namespace Samolut_Fintech_Application.Models.Transfers;

public class suspendedReasonModel
{
    [Required]
    public string SUSPENDED_TRANSACTION_REASON { get; set; }
}