using System.ComponentModel.DataAnnotations;
namespace Samolut_Fintech_Application.Models.Transfers;

public class ExternalCustomerModel
{   
    //to check against db to see if we can find an account
    [Required]
    public string PHONE_NUMBER { get; set; }
    
    [Required]
    public string FULL_NAME { get; set; }
    
    //after the check for passing the id
    public int CUSTOMER_ID { get; set; }
    

    
    
}