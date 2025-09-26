using System.Net.Http;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SAS.Models;
using System.IO;

namespace SAS.Services
{
    public class IdCardService
    {
        private readonly HttpClient _httpClient;

        private const float CardWidth = 3.37f * 72;
        private const float CardHeight = 2.125f * 72;

        public IdCardService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private byte[]? LoadImageBytes(string url)
        {
            try
            {
                return _httpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();
            }
            catch
            {
                return null;
            }
        }

        private byte[]? LoadLogo()
        {
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "SAS.png");
            return File.Exists(logoPath) ? File.ReadAllBytes(logoPath) : null;
        }

        public void BuildStudentIdCard(PageDescriptor page, Student student)
        {
            var logoBytes = LoadLogo();

            page.Size(CardWidth, CardHeight);
            page.Margin(4);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

            page.Content().Border(1).BorderColor(Colors.Grey.Lighten2).Column(col =>
            {
                col.Item().Height(30).Background(Colors.Red.Medium).Row(header =>
                {
                    if (logoBytes != null)
                        header.ConstantItem(30).Height(30).AlignCenter().AlignMiddle()
                            .Image(logoBytes, ImageScaling.FitArea);
                    else
                        header.ConstantItem(30).Height(30).Background(Colors.Grey.Lighten3);

                    header.RelativeItem().AlignMiddle().AlignCenter()
                        .Text("School Administrative System")
                        .FontColor(Colors.White).Bold().FontSize(11);
                });

                col.Item().Padding(4).Row(row =>
                {
                    // Photo
                    if (!string.IsNullOrEmpty(student.PhotoUrl))
                    {
                        var photoBytes = LoadImageBytes(student.PhotoUrl);
                        if (photoBytes != null)
                            row.ConstantItem(50).Height(60).Border(1).BorderColor(Colors.Grey.Lighten1)
                                .Image(photoBytes, ImageScaling.FitArea);
                        else
                            row.ConstantItem(50).Height(60).Background(Colors.Grey.Lighten3);
                    }
                    else
                    {
                        row.ConstantItem(50).Height(60).Background(Colors.Grey.Lighten3);
                    }

                    // Details
                    row.RelativeItem().PaddingLeft(6).Column(details =>
                    {
                        details.Item().Text(student.StudentName).Bold();
                        details.Item().Text($"Std: {student.Std}   Div: {student.Div}");
                        details.Item().Text($"Roll No: {student.RollNo}");
                        details.Item().Text($"Phone: {student.PhoneNo}");
                    });
                });

                col.Item().AlignCenter().PaddingTop(2)
                    .Text("College: DDU")
                    .FontSize(8).FontColor(Colors.Grey.Medium);
            });
        }

        public void BuildUserIdCard(PageDescriptor page, User user)
        {
            var logoBytes = LoadLogo();

            page.Size(CardWidth, CardHeight);
            page.Margin(6);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

            page.Content().Border(1).BorderColor(Colors.Grey.Lighten2).Column(col =>
            {
                col.Item().Height(35).Background(Colors.Blue.Medium).Row(header =>
                {
                    if (logoBytes != null)
                        header.ConstantItem(35).Height(35).AlignCenter().AlignMiddle()
                            .Image(logoBytes, ImageScaling.FitArea);
                    else
                        header.ConstantItem(35).Height(35).Background(Colors.Grey.Lighten3);

                    header.RelativeItem().AlignMiddle().AlignCenter()
                        .Text("School Administrative System - DDU")
                        .FontColor(Colors.White).Bold().FontSize(13);
                });

                col.Item().PaddingVertical(4).Row(row =>
                {
                    // Photo
                    if (!string.IsNullOrEmpty(user.UserDetails?.Photo))
                    {
                        var photoBytes = LoadImageBytes(user.UserDetails.Photo);
                        if (photoBytes != null)
                            row.ConstantItem(60).Height(70).Border(1).BorderColor(Colors.Grey.Lighten1)
                                .Image(photoBytes, ImageScaling.FitArea);
                        else
                            row.ConstantItem(60).Height(70).Background(Colors.Grey.Lighten3);
                    }
                    else
                    {
                        row.ConstantItem(60).Height(70).Background(Colors.Grey.Lighten3);
                    }

                    // Details + Role
                    row.RelativeItem().PaddingLeft(6).Column(details =>
                    {
                        details.Item().Text(user.Role.ToString().ToUpper())
                            .FontSize(14).Bold().FontColor(Colors.Blue.Darken2);

                        details.Item().Text(user.Name)
                            .Bold().FontSize(12).FontColor(Colors.Blue.Darken3);

                        details.Item().Text($"Phone: {user.UserDetails?.Phone ?? "-"}").FontSize(9);

                        if (user.UserDetails != null)
                        {
                            details.Item().Text($"DOB: {user.UserDetails.Dob:dd/MM/yyyy}").FontSize(9);
                            details.Item().Text($"Joining: {user.UserDetails.JoiningDate:dd/MM/yyyy}").FontSize(9);
                        }
                    });
                });
                col.Item().AlignCenter().PaddingTop(2)
                .Text("College: DDU")
                .FontSize(8).FontColor(Colors.Grey.Medium);
            });
        }
    }
}