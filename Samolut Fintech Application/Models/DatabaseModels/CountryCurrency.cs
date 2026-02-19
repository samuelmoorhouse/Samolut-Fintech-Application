using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Samolut_Fintech_Application.Models.DatabaseModels
{
    [Table("CurrentCurrency")]
    public class CurrentCurrency
    {
        [Key]
        public int COUNTRY_CURRENCY_ID { get; set; }
        [Required]
        public string COUNTRY_CURRENCY_NAME { get; set; }
    }
}
