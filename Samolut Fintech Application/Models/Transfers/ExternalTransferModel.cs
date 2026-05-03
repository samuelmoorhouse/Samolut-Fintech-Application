using System.ComponentModel.DataAnnotations;
namespace Samolut_Fintech_Application.Models.Transfers;

public class ExternalTransferModel
{
    //so my customer model plus some transaction data
    [Required]
    public string PHONE_NUMBER { get; set; }
    
    [Required]
    public string FULL_NAME { get; set; }
    
    //after the check for passing the id
    public int CUSTOMER_ID { get; set; }
    
    
    
    
    //adding all these transacrion data from the internal currency model
    [Key]
    public int? TRANSACTION_ID { get; set; }
    [Required]
    public int SENDER_ACCOUNT_ID { get; set; }
    [Required]
    public int RECEIVER_ACCOUNT_ID { get; set; }
    [Required]
    //max like 1 million transfer
    [Range(0,1000000)]
    public double AMOUNT { get; set; }
        
    //added this so i can check if they have enough funds
    public double ORIGINAL_AMOUNT { get; set; }
        
        
    //this stuff gets calculated so isnt required until its calculated. added ? so can be null
        
    public double EXCHANGE_RATE { get; set; } 
    public int? START_CURRENCY { get; set; }
    public int? END_CURRENCY { get; set; }
        
    [Required]
    public DateTime TRANSACTION_TIME { get; set; } = DateTime.Now;
}