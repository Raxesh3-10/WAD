using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SAS.ViewModels
{
    public class TimetableViewModel
    {
        [Required(ErrorMessage = "Lecture Duration is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Lecture Duration must be greater than 0.")]
        public int LectureDuration { get; set; }

        [Required(ErrorMessage = "Start Time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End Time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Lunch Start time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan LunchStart { get; set; }

        [Required(ErrorMessage = "Lunch Duration is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Lunch Duration must be greater than 0.")]
        public int LunchDuration { get; set; }

        [Required(ErrorMessage = "Days in Week is required.")]
        [Range(5, 6, ErrorMessage = "Days in Week must be either 5 or 6.")]
        public int DaysInWeek { get; set; }

        public ICollection<int> Stds { get; set; } = new List<int>();

        public ICollection<string> Subjects { get; set; } = new List<string>();
    }
}