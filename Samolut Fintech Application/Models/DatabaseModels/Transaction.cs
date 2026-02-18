using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Samolut_Fintech_Application.Models.DatabaseModels
{
    [Table("Transaction")]
    public class Transaction
    {
        [Key]
        public int TRANSACTION_ID { get; set; }
        [Required]
        public int SENDER_ACCOUNT_ID { get; set; }
        [Required]
        public int RECEIVER_ACCOUNT_ID { get; set; }
        [Required]
        public double AMOUNT { get; set; }
        [Required]
        public double EXCHANGE_RATE { get; set; }
        [Required]
        public string START_CURRENCY { get; set; }
        [Required]
        public string END_CURRENCY { get; set; }
        [Required]
        //sends the dstabase the time it was done instead of the database choosing when the transaction happened, because i thought the c sharp was harder if i tried to ignore this.
        public DateTime TRANSACTION_TIME { get; set; } = DateTime.Now;

    }
}
