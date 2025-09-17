using System;
using System.ComponentModel.DataAnnotations;

namespace SAS.Models
{
    public enum BillType
    {
        Electricity,
        Water,
        Internet,
        Maintenance,
        Stationery,
        Furniture,
        Transport,
        Examination,
        Printing,
        Software,
        Miscellaneous
    }

    public class Bill
    {
        public Guid Id { get; set; }

        public BillType Type { get; set; }

        public double Amount { get; set; }

        public DateTime BillDate { get; set; }

        public string VendorName { get; set; }
        public string VendorEmail { get; set; }
    }
}