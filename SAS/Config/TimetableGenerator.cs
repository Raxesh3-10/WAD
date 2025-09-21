using System;
using System.Collections.Generic;

namespace SAS.Config
{
    public class Slot
    {
        public string Start { get; set; } = string.Empty;
        public string End { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Teacher { get; set; }
    }

    public class TimetableGenerator
    {
        private readonly int _lectureDuration;
        private readonly TimeSpan _startTime;
        private readonly TimeSpan _endTime;
        private readonly TimeSpan _lunchStart;
        private readonly int _lunchDuration;
        private readonly int _daysInWeek;
        private readonly List<string> _subjects;

        public TimetableGenerator(int lectureDuration, TimeSpan startTime, TimeSpan endTime,
            TimeSpan lunchStart, int lunchDuration, int daysInWeek, List<string> subjects)
        {
            _lectureDuration = lectureDuration;
            _startTime = startTime;
            _endTime = endTime;
            _lunchStart = lunchStart;
            _lunchDuration = lunchDuration;
            _daysInWeek = daysInWeek;
            _subjects = subjects;
        }

        public Dictionary<string, List<Slot>> GetSchedule()
        {
            var schedule = new Dictionary<string, List<Slot>>();
            var days = _daysInWeek == 5
                ? new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" }
                : new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

            foreach (var day in days)
            {
                var slots = new List<Slot>();
                var currentTime = _startTime;

                while (currentTime + TimeSpan.FromMinutes(_lectureDuration) <= _endTime)
                {
                    if (currentTime >= _lunchStart && currentTime < _lunchStart + TimeSpan.FromMinutes(_lunchDuration))
                    {
                        slots.Add(new Slot
                        {
                            Start = _lunchStart.ToString(@"hh\:mm"),
                            End = (_lunchStart + TimeSpan.FromMinutes(_lunchDuration)).ToString(@"hh\:mm"),
                            Subject = "Lunch Break"
                        });
                        currentTime = _lunchStart + TimeSpan.FromMinutes(_lunchDuration);
                        continue;
                    }

                    var subject = _subjects[new Random().Next(_subjects.Count)];
                    slots.Add(new Slot
                    {
                        Start = currentTime.ToString(@"hh\:mm"),
                        End = (currentTime + TimeSpan.FromMinutes(_lectureDuration)).ToString(@"hh\:mm"),
                        Subject = subject
                    });

                    currentTime += TimeSpan.FromMinutes(_lectureDuration);
                }

                schedule[day] = slots;
            }

            return schedule;
        }
    }
}