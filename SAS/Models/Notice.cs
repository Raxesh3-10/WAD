using System;
using System.ComponentModel.DataAnnotations;

namespace SAS.Models
{
    public class Notice
    {
        [Key]
        public Guid NoticeId { get; set; }

        [StringLength(200)]
        public string Subject { get; set; }

        public string Message { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public Guid UserId { get; set; }

        public User User { get; set; }

        public string? Documents { get; set; }
    }
}