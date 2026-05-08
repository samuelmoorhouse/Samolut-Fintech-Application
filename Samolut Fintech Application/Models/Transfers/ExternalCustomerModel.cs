using System.ComponentModel.DataAnnotations;
namespace Samolut_Fintech_Application.Models.Transfers;

public class ExternalCustomerModel
{   
    //to check against db to see if we can find an account
    [Required(ErrorMessage = "Phone number is required.")]
    public string PHONE_NUMBER { get; set; }
    
    [Required(ErrorMessage = "Name is required.")]
    public string FULL_NAME { get; set; }
    
    //after the check for passing the id
    public int CUSTOMER_ID { get; set; }
    

    
    
}