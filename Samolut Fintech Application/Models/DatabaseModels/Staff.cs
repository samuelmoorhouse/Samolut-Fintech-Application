using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Samolut_Fintech_Application.Models.DatabaseModels
{
    [Table("Staff")]
    public class Staff
    {
        [Key]
        public int STAFF_ID { get; set; }
        [Required]
        public int ROLE_ID { get; set; }
        [Required]
        public string FIRST_NAME { get; set; }

        public string? MIDDLE_NAME { get; set; }
        [Required]
        public string LAST_NAME { get; set; }
        [Required]
        public string PASSWORD { get; set; }
        //while none of trhese key or required or foreign key names are neceseary in the [], the tutorial said it was good practice

        [ForeignKey("ROLE_ID")]
        public Role RoleIdForeignKey { get; set; }

    }
}