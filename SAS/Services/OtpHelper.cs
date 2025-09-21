using System;
using Microsoft.AspNetCore.Http;

namespace SAS.Services
{
    public static class OtpHelper
    {
        public static string GenerateOtp()
        {
            Random rnd = new Random();
            return rnd.Next(100000, 999999).ToString();
        }

        public static void StoreOtp(HttpContext context, string email, string otp)
        {
            context.Session.SetString("OTP_EMAIL", email);
            context.Session.SetString("OTP_CODE", otp);
            context.Session.SetString("OTP_TIMESTAMP", DateTime.UtcNow.ToString("o"));
        }

        public static bool VerifyOtp(HttpContext context, string email, string otp)
        {
            var storedEmail = context.Session.GetString("OTP_EMAIL");
            var storedOtp = context.Session.GetString("OTP_CODE");
            var storedTimeStr = context.Session.GetString("OTP_TIMESTAMP");

            if (storedEmail == null || storedOtp == null || storedTimeStr == null)
                return false;

            if (storedEmail != email || storedOtp != otp)
                return false;

            if (!DateTime.TryParse(storedTimeStr, out DateTime storedTime))
                return false;

            return DateTime.UtcNow - storedTime <= TimeSpan.FromMinutes(5);
        }

        public static void ClearOtp(HttpContext context)
        {
            context.Session.Remove("OTP_EMAIL");
            context.Session.Remove("OTP_CODE");
            context.Session.Remove("OTP_TIMESTAMP");
        }
    }
}
