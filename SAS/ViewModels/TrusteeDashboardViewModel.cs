using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SAS.Models;

namespace SAS.ViewModels
{
    public class TrusteeDashboardViewModel
    {
        public User Profile { get; set; }

        public IEnumerable<Notice> Notices { get; set; } = new List<Notice>();

        public List<User> Teachers { get; set; } = new List<User>();
        public List<User> Principals { get; set; } = new List<User>();
    }
}