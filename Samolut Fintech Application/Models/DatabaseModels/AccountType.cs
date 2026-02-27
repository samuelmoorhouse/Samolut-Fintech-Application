using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Samolut_Fintech_Application.Models.DatabaseModels
{
    [Table("AccountType")]
    public class AccountType
    {
        [Key]
        public int ACCOUNT_TYPE_ID { get; set; }
        [Required]
        public string TYPE_NAME { get; set; }
        [Required]
        public string DESCRIPTION { get; set; }



    }
}
