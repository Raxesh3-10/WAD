using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SAS.Models;

namespace SAS.ViewModels
{
    public class BillViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Bill type is required.")]
        public BillType Type { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be positive.")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "Bill date is required.")]
        public DateTime BillDate { get; set; } = DateTime.Now;
        public string? Documents { get; set; }
        public List<IFormFile>? NewDocuments { get; set; }
        public List<int>? RemoveDocIndexes { get; set; } = new List<int>();
        [StringLength(100)]
        public string VendorName { get; set; }
        [EmailAddress]
        public string VendorEmail { get; set; }
    }
}