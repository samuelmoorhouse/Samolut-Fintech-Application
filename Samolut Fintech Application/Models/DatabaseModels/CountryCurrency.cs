using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Samolut_Fintech_Application.Models.DatabaseModels
{
    [Table("CountryCurrency")]
    public class CountryCurrency
    {
        [Key]
        public int COUNTRY_CURRENCY_ID { get; set; }
        [Required]
        public string COUNTRY_NAME { get; set; }
    }
}
