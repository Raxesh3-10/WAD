using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SAS.ViewModels
{
    public class NoticeViewModel
    {
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

        public string? Documents { get; set; }
        public List<IFormFile>? NewDocuments { get; set; }
        public List<int>? RemoveDocIndexes { get; set; } = new List<int>();
    }
}