using Microsoft.AspNetCore.Mvc;
using SAS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAS.ViewComponents
{
    public class TimetableViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            // Provide default values for the form
            var model = new TimetableViewModel
            {
                Stds = Enumerable.Range(1, 12).ToList(),
                DaysInWeek = 5,
                LectureDuration = 45,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(15, 0, 0),
                LunchStart = new TimeSpan(12, 0, 0),
                LunchDuration = 60
            };

            return View(model);
        }
    }
}
