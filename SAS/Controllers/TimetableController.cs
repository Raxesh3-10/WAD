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

        public TimetableController(IRepository<User> userRepo, IUserDetailsRepository userDetailsRepo)
        {
            _userRepo = userRepo;
            _userDetailsRepo = userDetailsRepo;
        }

        [HttpPost]
        public IActionResult GenerateTimetable(TimetableViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var std = model.Stds.FirstOrDefault();
            if (std == 0) return BadRequest("STD is required.");

            var allUserDetails = _userDetailsRepo.GetAll();

            // --- Get divisions ---
            var divisions = allUserDetails
                .Where(ud => ud.Stds.Any(s => s.Std == std))
                .SelectMany(ud => ud.Stds)
                .Where(s => s.Std == std)
                .Select(s => s.Std.ToString()) // Using Std itself as string for division label
                .Distinct()
                .ToList();

            if (!divisions.Any()) return NotFound($"No divisions found for STD {std}");

            // --- Get teachers ---
            var teachers = allUserDetails
                .Where(ud => ud.Stds.Any(s => s.Std == std))
                .Select(ud => new
                {
                    Name = _userRepo.GetAll().FirstOrDefault(u => u.Id == ud.UserId)?.Name,
                    Subjects = ud.Subjects.Select(s => s.SubjectName).ToList()
                })
                .Where(t => t.Name != null && t.Subjects.Any())
                .ToList();

            if (!teachers.Any()) return NotFound($"No teachers found for STD {std}");

            // --- Map subjects to teachers ---
            var subjectTeacherMap = new Dictionary<string, List<string>>();
            foreach (var t in teachers)
            {
                foreach (var sub in t.Subjects)
                {
                    if (!subjectTeacherMap.ContainsKey(sub))
                        subjectTeacherMap[sub] = new List<string>();
                    subjectTeacherMap[sub].Add(t.Name!);
                }
            }

            // --- Prepare timetable data ---
            var teacherSchedule = teachers.ToDictionary(t => t.Name!, t => new Dictionary<string, List<string>>());
            var timetableData = new Dictionary<string, Dictionary<string, List<Slot>>>();

            foreach (var div in divisions)
            {
                var shuffledSubjects = Shuffle(subjectTeacherMap.Keys.ToList());

                var timetableGenerator = new TimetableGenerator(
                    model.LectureDuration,
                    model.StartTime,
                    model.EndTime,
                    model.LunchStart,
                    model.LunchDuration,
                    model.DaysInWeek,
                    shuffledSubjects
                );

                var schedule = timetableGenerator.GetSchedule();

                foreach (var day in schedule.Keys.ToList())
                {
                    var slots = schedule[day];
                    for (int i = 0; i < slots.Count; i++)
                    {
                        if (slots[i].Subject == "Lunch Break") continue;

                        var shuffledTeachers = Shuffle(subjectTeacherMap[slots[i].Subject]);
                        var availableTeachers = shuffledTeachers
                            .Where(tName =>
                            {
                                if (!teacherSchedule.ContainsKey(tName)) teacherSchedule[tName] = new Dictionary<string, List<string>>();
                                if (!teacherSchedule[tName].ContainsKey(day)) teacherSchedule[tName][day] = new List<string>();
                                return !teacherSchedule[tName][day].Contains($"{slots[i].Start}-{slots[i].End}");
                            }).ToList();

                        var assignedTeacher = availableTeachers.Any() ? RandomPick(availableTeachers) : "TBD";

                        if (!teacherSchedule.ContainsKey(assignedTeacher)) teacherSchedule[assignedTeacher] = new Dictionary<string, List<string>>();
                        if (!teacherSchedule[assignedTeacher].ContainsKey(day)) teacherSchedule[assignedTeacher][day] = new List<string>();
                        teacherSchedule[assignedTeacher][day].Add($"{slots[i].Start}-{slots[i].End}");

                        slots[i].Teacher = assignedTeacher;
                    }

                    schedule[day] = slots.OrderBy(s => TimeSpan.Parse(s.Start)).ToList();
                }

                timetableData[div] = schedule;
            }

            // --- Generate PDF ---
            var pdfStream = new System.IO.MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(12));
                    page.Header().Text($"Randomized Timetable for STD {std}")
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

                                    static IContainer CellStyle(IContainer container) => container.Padding(2).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                                });

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
}