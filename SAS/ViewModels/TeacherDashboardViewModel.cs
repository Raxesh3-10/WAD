using SAS.Models;
using System.Collections.Generic;
using System.Linq;

namespace SAS.ViewModels
{
    public class TeacherDashboardViewModel
    {
        public User Profile { get; set; }

        public Dictionary<string, Dictionary<string, List<Student>>> GroupedStudents { get; set; }
            = new Dictionary<string, Dictionary<string, List<Student>>>();

        public List<Notice> Notices { get; set; } = new List<Notice>();

        public List<int> AllStds => GroupedStudents?.Keys
            .Select(k => int.Parse(k))
            .OrderBy(k => k)
            .ToList() ?? new List<int>();
    }
}