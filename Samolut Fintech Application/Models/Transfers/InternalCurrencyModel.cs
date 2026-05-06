using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Samolut_Fintech_Application.Models.Transfers
{
    public class InternalCurrencyModel
    {
        
        //need to add rules so that liekt e amoujnt sent cant be less than 0
        //used this site to find all the attributes for validation https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-10.0
        [Key]
        public int? TRANSACTION_ID { get; set; }
        [Required]
        public int SENDER_ACCOUNT_ID { get; set; }
        [Required]
        public int RECEIVER_ACCOUNT_ID { get; set; }
        [Required]
        public double AMOUNT { get; set; }
        
        //added this so i can check if they have enough funds
        public double ORIGINAL_AMOUNT { get; set; }
        
        public double SENDER_GBP_EXCHANGE_RATE { get; set; }
        
        
        //this stuff gets calculated so isnt required until its calculated. added ? so can be null
        
        public double EXCHANGE_RATE { get; set; } 
        public int? START_CURRENCY { get; set; }
        public int? END_CURRENCY { get; set; }
        
        [Required]
        public DateTime TRANSACTION_TIME { get; set; } = DateTime.Now;



    }
}
