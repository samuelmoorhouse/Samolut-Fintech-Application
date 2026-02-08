using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Samolut_Fintech_Application.Models.DatabaseModels
{
    [Table("Role")]
    public class Role
    {
        [Key]
        public int ROLE_ID { get; set; }
        [Required]
        public string ROLE_DESCRIPTION { get; set; }

    }
}
