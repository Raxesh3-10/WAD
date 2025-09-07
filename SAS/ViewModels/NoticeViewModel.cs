using System;
using System.ComponentModel.DataAnnotations;

namespace SAS.ViewModels
{
    public class NoticeViewModel
    {
        public Guid NoticeId { get; set; }  // Needed for Edit form (hidden input)

        [Required(ErrorMessage = "'Subject' is required.")]
        [StringLength(200)]
        public string Subject { get; set; }

        [Required(ErrorMessage = "'Message' is required.")]
        public string Message { get; set; }

        [Required(ErrorMessage = "'Date' is required.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public Guid UserId { get; set; }
    }
}