using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Samolut_Fintech_Application.Models.DatabaseModels;

[Table("SuspiciousTransaction")]
public class SuspiciousTransaction
{
    [Key]
    public int SUSPENDED_TRANSACTION_ID { get; set; }
    [Required]
    public int SENDER_ACCOUNT_ID { get; set; }
    [Required]
    public int RECEIVER_ACCOUNT_ID { get; set; }
    [Required]
    public double AMOUNT { get; set; }
    [Required]
    public double EXCHANGE_RATE { get; set; }
    [Required]
    public int START_CURRENCY { get; set; }
    [Required]
    public int END_CURRENCY { get; set; }
    [Required]
    public DateTime TRANSACTION_TIME { get; set; }
    
    //added reason to transaction for suspicion
    public string? SUSPENDED_TRANSACTION_REASON { get; set; }
    
    
    
    
    [ForeignKey("SENDER_ACCOUNT_ID")]
    public Account SenderAccountIdForeignKey { get; set; }

    [ForeignKey("RECEIVER_ACCOUNT_ID")]
    public Account ReceiverAccountIdForeignKey { get; set; }

}