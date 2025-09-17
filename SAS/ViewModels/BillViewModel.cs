using System;
using System.ComponentModel.DataAnnotations;
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
        public DateTime BillDate { get; set; }
        [Required]
        [StringLength(100)]
        public string VendorName { get; set; }
        [Required]
        [EmailAddress]
        public string VendorEmail { get; set; }
    }
}