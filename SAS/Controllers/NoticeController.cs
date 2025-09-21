using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAS.Models;
using SAS.Repositories;
using SAS.ViewModels;
using SAS.Services;
using AutoMapper;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SAS.Controllers
{
    public class NoticeController : Controller
    {
        private readonly INoticeRepository _noticeRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IMapper _mapper;
        private readonly MailService _mailService;
        private readonly CloudinaryService _cloudinary;

        public NoticeController(
            INoticeRepository noticeRepo,
            IRepository<User> userRepo,
            IMapper mapper,
            MailService mailService,
            CloudinaryService cloudinary)
        {
            _noticeRepo = noticeRepo;
            _userRepo = userRepo;
            _mapper = mapper;
            _mailService = mailService;
            _cloudinary = cloudinary;
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

            // Upload new docs
            var docUrls = new List<string>();
            if (noticeVm.NewDocuments != null)
            {
                foreach (var file in noticeVm.NewDocuments)
                {
                    var url = _cloudinary.UploadFile(file, "documents");
                    if (!string.IsNullOrEmpty(url))
                        docUrls.Add(url);
                }
            }
            notice.Documents = string.Join(",", docUrls);

            _noticeRepo.Add(notice);

            NotifyUsers("New Notice - SAS Platform", "A new notice has been circulated. Please check it.");

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

            var existing = _noticeRepo.GetById(noticeVm.NoticeId);
            if (existing == null) return NotFound();

            existing.Subject = noticeVm.Subject;
            existing.Message = noticeVm.Message;
            existing.Date = noticeVm.Date;
            existing.UserId = currentUser.Id;

            // Handle removals
            var existingDocs = (existing.Documents ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (noticeVm.RemoveDocIndexes != null && noticeVm.RemoveDocIndexes.Any())
            {
                foreach (var index in noticeVm.RemoveDocIndexes.OrderByDescending(i => i))
                {
                    if (index >= 0 && index < existingDocs.Count)
                    {
                        _cloudinary.DeleteFromCloudinary(existingDocs[index], false);
                        existingDocs.RemoveAt(index);
                    }
                }
            }

            // Handle new uploads
            if (noticeVm.NewDocuments != null)
            {
                foreach (var file in noticeVm.NewDocuments)
                {
                    var url = _cloudinary.UploadFile(file, "documents");
                    if (!string.IsNullOrEmpty(url))
                        existingDocs.Add(url);
                }
            }

            existing.Documents = string.Join(",", existingDocs);

            _noticeRepo.Update(existing);

            NotifyUsers("Notice Updated - SAS Platform", "A notice has been updated. Please check it.");

            return RedirectToAction("Dashboard", "Principal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteNotice(Guid id)
        {
            if (!IsAuthorized("principal", "trustee")) return Unauthorized();

            var existing = _noticeRepo.GetById(id);
            if (existing == null) return NotFound();

            if (!string.IsNullOrEmpty(existing.Documents))
            {
                var docs = existing.Documents.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var doc in docs)
                {
                    _cloudinary.DeleteFromCloudinary(doc, false);
                }
            }

            _noticeRepo.DeleteById(id);

            return RedirectToAction("Dashboard", "Principal");
        }

        [HttpGet]
        public IActionResult GetNotice(Guid id)
        {
            if (!IsAuthorized("teacher", "staff", "principal", "trustee")) return Unauthorized();

            var notice = _noticeRepo.GetById(id);
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

        private void NotifyUsers(string subject, string message)
        {
            var users = _userRepo.GetAll().ToList();
            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(user.Email))
                {
                    _mailService.SendEmail(subject, message, user.Email);
                }
            }
        }
    }
}