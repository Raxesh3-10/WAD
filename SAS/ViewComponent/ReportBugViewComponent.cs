using Microsoft.AspNetCore.Mvc;
using SAS.Services;
using SAS.ViewModels;
using System.Threading.Tasks;

namespace SAS.ViewComponents
{
    public class ReportBugViewComponent : ViewComponent
    {
        private readonly MailService _mailService;

        public ReportBugViewComponent(MailService mailService)
        {
            _mailService = mailService;
        }

        public IViewComponentResult Invoke(string name, string email)
        {
            var model = new ReportBugViewModel
            {
                Name = name,
                Email = email
            };
            return View(model);
        }
    }
}