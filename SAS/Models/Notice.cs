using System;
using System.ComponentModel.DataAnnotations;

namespace SAS.Models
{
    public class Notice
    {
        [Key]
        public Guid NoticeId { get; set; }

        [Required(ErrorMessage = "'Subject' is required.")]
        [StringLength(200)]
        public string Subject { get; set; }

        [Required(ErrorMessage = "'Message' is required.")]
        public string Message { get; set; }

        [Required(ErrorMessage = "'Date' is required.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public Guid UserId { get; set; }

        public User User { get; set; }
    }
}
