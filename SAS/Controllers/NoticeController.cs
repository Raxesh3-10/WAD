using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAS.Models;
using SAS.Repositories;
using SAS.ViewModels;
using SAS.Services;
using AutoMapper;
using System;
using System.Linq;

namespace SAS.Controllers
{
    public class NoticeController : Controller
    {
        private readonly INoticeRepository _noticeRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IMapper _mapper;
        private readonly MailService _mailService;

        public NoticeController(
            INoticeRepository noticeRepo,
            IRepository<User> userRepo,
            IMapper mapper,
            MailService mailService)
        {
            _noticeRepo = noticeRepo;
            _userRepo = userRepo;
            _mapper = mapper;
            _mailService = mailService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateNotice(NoticeViewModel noticeVm)
        {
            if (!IsAuthorized("principal", "trustee")) return Unauthorized();
            if (!ModelState.IsValid) return ViewComponent("Notice");

            var currentUser = GetCurrentUser();
            if (currentUser == null) return Unauthorized();

            var notice = _mapper.Map<Notice>(noticeVm);
            notice.NoticeId = Guid.NewGuid();
            notice.UserId = currentUser.Id;

            _noticeRepo.Add(notice, noticeVm.NewDocuments);

            var users = _userRepo.GetAll().ToList();
            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(user.Email))
                {
                    _mailService.SendEmail(
                        "New Notice - SAS Platform",
                        "A new notice has been circulated. Please check it.",
                        user.Email
                    );
                }
            }

            TempData["SuccessMessage"] = "Notice created successfully.";
            return RedirectToAction("Dashboard", "Principal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditNotice(NoticeViewModel noticeVm)
        {
            if (!IsAuthorized("principal", "trustee")) return Unauthorized();
            if (!ModelState.IsValid) return ViewComponent("Notice");

            var currentUser = GetCurrentUser();
            if (currentUser == null) return Unauthorized();

            var existing = _noticeRepo.GetAll().FirstOrDefault(n => n.NoticeId == noticeVm.NoticeId);
            if (existing == null) return NotFound();

            existing.Subject = noticeVm.Subject;
            existing.Message = noticeVm.Message;
            existing.Date = noticeVm.Date;
            existing.UserId = currentUser.Id;

            if (!string.IsNullOrEmpty(existing.Documents) && noticeVm.RemoveDocIndexes != null && noticeVm.RemoveDocIndexes.Count > 0)
            {
                var docList = existing.Documents.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

                foreach (var index in noticeVm.RemoveDocIndexes.OrderByDescending(i => i))
                {
                    if (index >= 0 && index < docList.Count)
                        docList.RemoveAt(index);
                }

                existing.Documents = string.Join(",", docList);
            }

            _noticeRepo.Update(existing.User.Email, existing, noticeVm.NewDocuments);

            var users = _userRepo.GetAll().ToList();
            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(user.Email))
                {
                    _mailService.SendEmail(
                        "Notice Updated - SAS Platform",
                        "A notice has been updated. Please check it.",
                        user.Email
                    );
                }
            }

            TempData["SuccessMessage"] = "Notice updated successfully.";
            return RedirectToAction("Dashboard", "Principal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteNotice(Guid id)
        {
            if (!IsAuthorized("principal", "trustee")) return Unauthorized();

            var existing = _noticeRepo.GetAll().FirstOrDefault(n => n.NoticeId == id);
            if (existing == null) return NotFound();

            _noticeRepo.Delete(existing.User.Email);

            TempData["SuccessMessage"] = "Notice deleted successfully.";
            return RedirectToAction("Dashboard", "Principal");
        }

        [HttpGet]
        public IActionResult GetNotice(Guid id)
        {
            if (!IsAuthorized("teacher", "staff", "principal", "trustee")) return Unauthorized();

            var notice = _noticeRepo.GetAll().FirstOrDefault(n => n.NoticeId == id);
            if (notice == null) return NotFound();

            var vm = _mapper.Map<NoticeViewModel>(notice);
            vm.Documents = notice.Documents;

            return View(vm);
        }

        [HttpGet]
        public IActionResult GetAllNotices()
        {
            if (!IsAuthorized("teacher", "staff", "principal", "trustee")) return Unauthorized();

            var notices = _noticeRepo.GetAll().ToList();
            var vms = notices.Select(n => _mapper.Map<NoticeViewModel>(n)).ToList();

            return View(vms);
        }

        private bool IsAuthorized(params string[] roles)
        {
            var role = HttpContext.Session.GetString("UserRole");
            return roles.Contains(role?.ToLower());
        }

        private User? GetCurrentUser()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            return string.IsNullOrEmpty(email) ? null : _userRepo.GetByEmail(email);
        }
    }
}