using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SAS.ViewModels;
using SAS.Models;
using SAS.Repositories;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SAS.Controllers
{
    public class TimetableController : Controller
    {
        private readonly IRepository<User> _userRepo;
        private readonly IUserDetailsRepository _userDetailsRepo;
        private readonly IRepository<Student> _studentRepo;

        public TimetableController(IRepository<User> userRepo,
                                   IUserDetailsRepository userDetailsRepo,
                                   IRepository<Student> studentRepo)
        {
            _userRepo = userRepo;
            _userDetailsRepo = userDetailsRepo;
            _studentRepo = studentRepo;
        }

        [HttpPost]
        public IActionResult GenerateTimetable(TimetableViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var std = model.Stds?.FirstOrDefault() ?? 0;
            if (std == 0) return BadRequest("STD is required.");

            var allUserDetails = _userDetailsRepo.GetAll();

            // --- Find users who teach this standard ---
            var usersForStd = allUserDetails
                .Where(ud => UserHasStd(ud, std))
                .ToList();

            if (!usersForStd.Any())
                return NotFound($"No teachers or staff found for STD {std}");

            // --- Divisions from Student table ---
            var divisions = _studentRepo.GetAll()
                .Where(s => s.Std == std && !string.IsNullOrWhiteSpace(s.Div))
                .Select(s => s.Div.Trim().ToUpper())
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            if (!divisions.Any())
                return NotFound($"No divisions found for STD {std}");

            // --- Get teachers (name + subjects) ---
            var teachers = usersForStd
                .Select(ud =>
                {
                    var name = _userRepo.GetAll().FirstOrDefault(u => u.Id == ud.UserId)?.Name;
                    var subjects = ParseSubjects(ud.Subjects);
                    return new
                    {
                        Name = name,
                        Subjects = subjects
                    };
                })
                .Where(t => !string.IsNullOrWhiteSpace(t.Name) && t.Subjects.Any())
                .ToList();

            if (!teachers.Any())
                return NotFound($"No teachers with subjects found for STD {std}");

            // --- Map subjects to teachers ---
            var subjectTeacherMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in teachers)
            {
                foreach (var sub in t.Subjects)
                {
                    var subjectKey = sub.Trim();
                    if (string.IsNullOrEmpty(subjectKey)) continue;

                    if (!subjectTeacherMap.ContainsKey(subjectKey))
                        subjectTeacherMap[subjectKey] = new List<string>();

                    if (!subjectTeacherMap[subjectKey].Contains(t.Name!, StringComparer.OrdinalIgnoreCase))
                        subjectTeacherMap[subjectKey].Add(t.Name!);
                }
            }

            if (!subjectTeacherMap.Any())
                return NotFound($"No subject-to-teacher mapping available for STD {std}");

            // --- Prepare timetable data ---
            var teacherSchedule = new Dictionary<string, Dictionary<string, List<string>>>(StringComparer.OrdinalIgnoreCase);
            var timetableData = new Dictionary<string, Dictionary<string, List<Slot>>>();
            var nopCountPerDay = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var div in divisions)
            {
                var subjects = subjectTeacherMap.Keys.ToList();

                var timetableGenerator = new EqualDistributionTimetableGenerator(
                    model.LectureDuration,
                    model.StartTime,
                    model.EndTime,
                    model.LunchStart,
                    model.LunchDuration,
                    model.DaysInWeek,
                    subjects
                );

                var schedule = timetableGenerator.GetSchedule();

                // Assign teachers for each slot
                foreach (var day in schedule.Keys.ToList())
                {
                    int nopCountDay = 0; // count NOP for this day
                    var slots = schedule[day];
                    for (int i = 0; i < slots.Count; i++)
                    {
                        var slot = slots[i];
                        if (string.Equals(slot.Subject, "Lunch Break", StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (!subjectTeacherMap.TryGetValue(slot.Subject, out var availableForSubject))
                        {
                            slot.Teacher = "NOP";
                            nopCountDay++;
                        }
                        else
                        {
                            var shuffledTeachers = Shuffle(availableForSubject);
                            var availableTeachers = shuffledTeachers
                                .Where(tName =>
                                {
                                    if (!teacherSchedule.ContainsKey(tName)) teacherSchedule[tName] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                                    if (!teacherSchedule[tName].ContainsKey(day)) teacherSchedule[tName][day] = new List<string>();
                                    return !teacherSchedule[tName][day].Contains($"{slot.Start}-{slot.End}");
                                }).ToList();

                            var assignedTeacher = availableTeachers.Any() ? RandomPick(availableTeachers) : "NOP";
                            if (assignedTeacher == "NOP") nopCountDay++;

                            if (assignedTeacher != "NOP")
                            {
                                if (!teacherSchedule.ContainsKey(assignedTeacher)) teacherSchedule[assignedTeacher] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                                if (!teacherSchedule[assignedTeacher].ContainsKey(day)) teacherSchedule[assignedTeacher][day] = new List<string>();
                                teacherSchedule[assignedTeacher][day].Add($"{slot.Start}-{slot.End}");
                            }

                            slot.Teacher = assignedTeacher;
                        }
                    }

                    // sort slots by start time
                    schedule[day] = slots.OrderBy(s => TimeSpan.Parse(s.Start)).ToList();

                    // save daily nop count
                    nopCountPerDay[$"{div}-{day}"] = nopCountDay;
                }

                timetableData[div] = schedule;
            }

            // --- Generate PDF using QuestPDF ---
            var pdfStream = new System.IO.MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(12));
                    page.Header().Text($"Timetable for STD {std}")
                                 .FontSize(18)
                                 .SemiBold()
                                 .AlignCenter();

                    page.Content().Column(col =>
                    {
                        foreach (var div in divisions)
                        {
                            col.Item().Text($"Division {div}").FontSize(16).Bold().Underline();

                            foreach (var day in timetableData[div].Keys)
                            {
                                col.Item().Text(day).FontSize(14).Bold().Underline();

                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("Time");
                                        header.Cell().Element(CellStyle).Text("Subject");
                                        header.Cell().Element(CellStyle).Text("Teacher");
                                    });

                                    foreach (var slot in timetableData[div][day])
                                    {
                                        table.Cell().Element(CellStyle).Text($"{slot.Start} - {slot.End}");
                                        table.Cell().Element(CellStyle).Text(slot.Subject);
                                        table.Cell().Element(CellStyle).Text(slot.Teacher ?? "");
                                    }

                                    static IContainer CellStyle(IContainer container) =>
                                        container.Padding(2).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                                });

                                // show NOP count for this day
                                var key = $"{div}-{day}";
                                if (nopCountPerDay.ContainsKey(key) && nopCountPerDay[key] > 0)
                                {
                                    col.Item().Text($"NOP = {nopCountPerDay[key]} slots (more teachers required)")
                                              .FontSize(12).Italic().FontColor(Colors.Red.Medium);
                                }

                                col.Item().PaddingBottom(10);
                            }

                            col.Item().PageBreak();
                        }
                    });
                });
            }).GeneratePdf(pdfStream);

            pdfStream.Position = 0;
            return File(pdfStream, "application/pdf", $"timetable_std{std}.pdf");
        }

        #region Helpers

        private static bool UserHasStd(UserDetails ud, int std)
        {
            if (string.IsNullOrWhiteSpace(ud.Stds)) return false;

            var parts = ud.Stds.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                if (int.TryParse(p.Trim(), out var sVal) && sVal == std)
                    return true;
            }
            return false;
        }

        private static List<string> ParseSubjects(string subjects)
        {
            if (string.IsNullOrWhiteSpace(subjects)) return new List<string>();
            return subjects
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }

        private static List<T> Shuffle<T>(List<T> list)
        {
            var rng = new Random();
            return list.OrderBy(_ => rng.Next()).ToList();
        }

        private static T RandomPick<T>(List<T> list)
        {
            var rng = new Random();
            return list[rng.Next(list.Count)];
        }

        #endregion
    }

    // --- Equal distribution generator ---
    public class EqualDistributionTimetableGenerator
    {
        private readonly int lectureDuration;
        private readonly TimeSpan startTime;
        private readonly TimeSpan endTime;
        private readonly TimeSpan lunchStart;
        private readonly int lunchDuration;
        private readonly int daysInWeek;
        private readonly List<string> subjects;

        public EqualDistributionTimetableGenerator(int lectureDuration, TimeSpan startTime,
            TimeSpan endTime, TimeSpan lunchStart, int lunchDuration, int daysInWeek, List<string> subjects)
        {
            this.lectureDuration = lectureDuration;
            this.startTime = startTime;
            this.endTime = endTime;
            this.lunchStart = lunchStart;
            this.lunchDuration = lunchDuration;
            this.daysInWeek = daysInWeek;
            this.subjects = subjects;
        }

        public Dictionary<string, List<Slot>> GetSchedule()
        {
            var schedule = new Dictionary<string, List<Slot>>(StringComparer.OrdinalIgnoreCase);

            int totalMinutes = (int)(endTime - startTime).TotalMinutes;
            int slotsPerDay = totalMinutes / lectureDuration;
            int lunchSlotIndex = (int)((lunchStart - startTime).TotalMinutes / lectureDuration);

            var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" }
                .Take(daysInWeek)
                .ToList();

            int subjectIndex = 0;

            foreach (var day in days)
            {
                var slots = new List<Slot>();
                var currentTime = startTime;

                for (int i = 0; i < slotsPerDay; i++)
                {
                    var slotEnd = currentTime.Add(TimeSpan.FromMinutes(lectureDuration));

                    if (i == lunchSlotIndex)
                    {
                        slots.Add(new Slot
                        {
                            Start = currentTime.ToString(@"hh\:mm"),
                            End = slotEnd.ToString(@"hh\:mm"),
                            Subject = "Lunch Break",
                            Teacher = null
                        });
                        currentTime = currentTime.Add(TimeSpan.FromMinutes(lunchDuration));
                        continue;
                    }

                    var subject = subjects[subjectIndex % subjects.Count];
                    subjectIndex++;

                    slots.Add(new Slot
                    {
                        Start = currentTime.ToString(@"hh\:mm"),
                        End = slotEnd.ToString(@"hh\:mm"),
                        Subject = subject,
                        Teacher = null
                    });

                    currentTime = slotEnd;
                }

                schedule[day] = slots;
            }

            return schedule;
        }
    }
}
