using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace SAS.Services
{
    public class MailService
    {
        private readonly IConfiguration _config;

        public MailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendEmail(string subject, string message, string toEmail)
        {
            var fromEmail = _config["MailSettings:FromEmail"];
            var appPassword = _config["MailSettings:AppPassword"];

            using var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(fromEmail, appPassword)
            };

            var mailMessage = new MailMessage(fromEmail, toEmail, subject, message);
            client.Send(mailMessage); // ✅ Synchronous send
        }

        public void SendOtp(string toEmail, string otp)
        {
            var subject = "Your OTP for SAS Platform";
            var body = $"Your OTP is: {otp}\nThis OTP is valid for 5 minutes.";
            SendEmail(subject, body, toEmail);
        }

        public void ReportBug(string userName, string userEmail, string message)
        {
            var devEmail = _config["MailSettings:DevEmail"];
            var subject = $"Bug Report from {userName} ({userEmail})";
            var body = $"Bug Message:\n{message}";
            SendEmail(subject, body, devEmail);
        }
    }
}
