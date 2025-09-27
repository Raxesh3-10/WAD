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
using SAS.Config;

namespace SAS.Controllers
{
    public class TimetableController : Controller
    {
        private readonly IRepository<User> _userRepo;
        private readonly IUserDetailsRepository _userDetailsRepo;
        private readonly IStudentRepository _studentRepo;

        public TimetableController(IRepository<User> userRepo,
                                   IUserDetailsRepository userDetailsRepo,
                                   IStudentRepository studentRepo)
        {
            _userRepo = userRepo;
            _userDetailsRepo = userDetailsRepo;
            _studentRepo = studentRepo;
        }

        [HttpPost]
        public IActionResult GenerateTimetable(TimetableViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Shared/Components/Timetable/default.cshtml", model);

            if (model.Stds == null || !model.Stds.Any())
            {
                ModelState.AddModelError("", "At least one STD is required.");
                return View("~/Views/Shared/Components/Timetable/default.cshtml", model);
            }

            try
            {
                var pdfStream = new System.IO.MemoryStream();

                // 🔑 Keep teacherSchedule GLOBAL to avoid conflicts across STDs
                var teacherSchedule = new Dictionary<string, Dictionary<string, List<string>>>(StringComparer.OrdinalIgnoreCase);

                Document.Create(container =>
                {
                    foreach (var std in model.Stds)
                    {
                        // === Get teachers for STD ===
                        var usersForStd = _userDetailsRepo.GetAll()
                            .Where(ud => UserHasStd(ud, std))
                            .ToList();
                        if (!usersForStd.Any())
                            throw new InvalidOperationException($"No teachers found for STD {std}.");

                        // === Get divisions ===
                        var divisions = _studentRepo.GetAll()
                            .Where(s => s.Std == std && !string.IsNullOrWhiteSpace(s.Div))
                            .Select(s => s.Div.Trim().ToUpper())
                            .Distinct()
                            .OrderBy(d => d)
                            .ToList();
                        if (!divisions.Any())
                            throw new InvalidOperationException($"No students/divisions found for STD {std}.");

                        // === Get teachers with subjects ===
                        var teachers = usersForStd
                            .Select(ud => new
                            {
                                Name = _userRepo.GetAll().FirstOrDefault(u => u.Id == ud.UserId)?.Name,
                                Subjects = ParseSubjects(ud.Subjects)
                            })
                            .Where(t => !string.IsNullOrWhiteSpace(t.Name) && t.Subjects.Any())
                            .ToList();
                        if (!teachers.Any())
                            throw new InvalidOperationException($"No teachers with subjects found for STD {std}.");

                        // === Subject → Teacher mapping ===
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
                            throw new InvalidOperationException($"No subject-teacher mapping found for STD {std}.");

                        // === Generate timetable (merge all divisions in one table) ===
                        var timetableData = new Dictionary<string, Dictionary<string, List<Slot>>>();

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

                            foreach (var day in schedule.Keys.ToList())
                            {
                                var slots = schedule[day];
                                for (int i = 0; i < slots.Count; i++)
                                {
                                    var slot = slots[i];
                                    if (string.Equals(slot.Subject, "Lunch Break", StringComparison.OrdinalIgnoreCase))
                                        continue;

                                    if (!subjectTeacherMap.TryGetValue(slot.Subject, out var availableForSubject))
                                    {
                                        slot.Teacher = "NOP";
                                    }
                                    else
                                    {
                                        var availableTeachers = Shuffle(availableForSubject)
                                            .Where(tName =>
                                            {
                                                if (!teacherSchedule.ContainsKey(tName))
                                                    teacherSchedule[tName] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                                                if (!teacherSchedule[tName].ContainsKey(day))
                                                    teacherSchedule[tName][day] = new List<string>();
                                                return !teacherSchedule[tName][day].Contains($"{slot.Start}-{slot.End}");
                                            })
                                            .ToList();

                                        var assignedTeacher = availableTeachers.Any() ? RandomPick(availableTeachers) : "NOP";

                                        if (assignedTeacher != "NOP")
                                        {
                                            if (!teacherSchedule.ContainsKey(assignedTeacher))
                                                teacherSchedule[assignedTeacher] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                                            if (!teacherSchedule[assignedTeacher].ContainsKey(day))
                                                teacherSchedule[assignedTeacher][day] = new List<string>();
                                            teacherSchedule[assignedTeacher][day].Add($"{slot.Start}-{slot.End}");
                                        }

                                        slot.Teacher = assignedTeacher;
                                    }
                                }
                                schedule[day] = slots.OrderBy(s => TimeSpan.Parse(s.Start)).ToList();
                            }

                            timetableData[div] = schedule;
                        }

                        // === Single Page for this STD ===
                        container.Page(page =>
                        {
                            page.Margin(15);
                            page.Size(PageSizes.A4.Landscape());
                            page.DefaultTextStyle(x => x.FontSize(9));

                            page.Header().Text($"Timetable for STD {std}")
                                .FontSize(14).SemiBold().AlignCenter();

                            page.Content().Table(table =>
                            {
                                var firstDiv = timetableData.Keys.First();
                                var firstDay = timetableData[firstDiv].First().Value;

                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1); // Day
                                    foreach (var slot in firstDay)
                                        columns.RelativeColumn(2); // Time slots
                                });

                                // === Table Header ===
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellHeader).Text("Day / Division");
                                    foreach (var slot in firstDay)
                                    {
                                        header.Cell().Element(CellHeader)
                                            .Text($"{slot.Start}-{slot.End}");
                                    }
                                });

                                // === Rows: day × division ===
                                foreach (var day in timetableData[firstDiv].Keys)
                                {
                                    foreach (var div in divisions)
                                    {
                                        table.Cell().Element(CellHeader).Text($"{day} ({div})");
                                        foreach (var slot in timetableData[div][day])
                                        {
                                            var text = slot.Subject;
                                            if (!string.IsNullOrWhiteSpace(slot.Teacher))
                                                text += $"\n({slot.Teacher})";
                                            table.Cell().Element(CellBody).Text(text);
                                        }
                                    }
                                }
                            });
                        });
                    }
                }).GeneratePdf(pdfStream);

                pdfStream.Position = 0;
                return File(pdfStream, "application/pdf", "timetable_all.pdf");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("~/Views/Shared/Components/Timetable/default.cshtml", model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred while generating the timetable.");
                return View("~/Views/Shared/Components/Timetable/default.cshtml", model);
            }
        }

        // === Helpers ===
        private static IContainer CellHeader(IContainer container) =>
            container.Padding(3).Background(Colors.Grey.Lighten2)
                .Border(1).BorderColor(Colors.Grey.Medium).AlignCenter();

        private static IContainer CellBody(IContainer container) =>
            container.Padding(3).Border(1).BorderColor(Colors.Grey.Lighten2).AlignCenter();

        private static bool UserHasStd(UserDetails ud, int std)
        {
            if (string.IsNullOrWhiteSpace(ud.Stds)) return false;
            return ud.Stds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Any(p => int.TryParse(p.Trim(), out var sVal) && sVal == std);
        }

        private static List<string> ParseSubjects(string subjects) =>
            string.IsNullOrWhiteSpace(subjects)
                ? new List<string>()
                : subjects.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => s.Trim())
                          .Where(s => !string.IsNullOrEmpty(s))
                          .ToList();

        private static List<T> Shuffle<T>(List<T> list) =>
            list.OrderBy(_ => Guid.NewGuid()).ToList();

        private static T RandomPick<T>(List<T> list)
        {
            var rng = new Random();
            return list[rng.Next(list.Count)];
        }

        // === Slot Class ===
        private class Slot
        {
            public string Start { get; set; } = string.Empty;
            public string End { get; set; } = string.Empty;
            public string Subject { get; set; } = string.Empty;
            public string Teacher { get; set; }
        }

        // === Generator ===
        private class EqualDistributionTimetableGenerator
        {
            private readonly int lectureDuration;
            private readonly TimeSpan startTime;
            private readonly TimeSpan endTime;
            private readonly TimeSpan lunchStart;
            private readonly int lunchDuration;
            private readonly int daysInWeek;
            private readonly List<string> subjects;

            public EqualDistributionTimetableGenerator(
                int lectureDuration,
                TimeSpan startTime,
                TimeSpan endTime,
                TimeSpan lunchStart,
                int lunchDuration,
                int daysInWeek,
                List<string> subjects)
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
}
