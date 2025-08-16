using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAS.Models;
using SAS.Repositories;
using SAS.ViewModels;
using AutoMapper;
using System.Linq;

namespace SAS.Controllers
{
    public class NoticeController : Controller
    {
        private readonly IRepository<Notice> _noticeRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IMapper _mapper;

        public NoticeController(IRepository<Notice> noticeRepo, IRepository<User> userRepo, IMapper mapper)
        {
            _noticeRepo = noticeRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public IActionResult CreateNotice([FromBody] NoticeViewModel noticeVm)
        {
            if (!IsAuthorized("principal")) return Unauthorized();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var currentUser = GetCurrentUser();
            if (currentUser == null) return Unauthorized();

            var notice = _mapper.Map<Notice>(noticeVm);
            notice.UserId = currentUser.Id;

            _noticeRepo.Add(notice);

            var resultVm = _mapper.Map<NoticeViewModel>(notice);
            return Ok(new { message = "Notice created successfully", notice = resultVm });
        }

        public IActionResult EditNotice(int id, [FromBody] NoticeViewModel noticeVm)
        {
            if (!IsAuthorized("principal")) return Unauthorized();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var currentUser = GetCurrentUser();
            if (currentUser == null) return Unauthorized();

            var updatedNotice = _mapper.Map<Notice>(noticeVm);
            updatedNotice.NoticeId = id;
            updatedNotice.UserId = currentUser.Id;

            var success = _noticeRepo.Update(currentUser.Email, updatedNotice);
            if (!success) return NotFound(new { message = "Notice not found" });

            var resultVm = _mapper.Map<NoticeViewModel>(updatedNotice);
            return Ok(new { message = "Notice updated successfully", notice = resultVm });
        }

        public IActionResult DeleteNotice(int id)
        {
            if (!IsAuthorized("principal")) return Unauthorized();

            var success = (_noticeRepo as SQLNoticeRepository)?.DeleteById(id) ?? false;
            if (!success) return NotFound(new { message = "Notice not found" });

            return Ok(new { message = "Notice deleted successfully" });
        }

        public IActionResult GetNotice(int id)
        {
            if (!IsAuthorized("principal")) return Unauthorized();

            var notice = _noticeRepo.GetAll().FirstOrDefault(n => n.NoticeId == id);
            if (notice == null) return NotFound(new { message = "Notice not found" });

            var noticeVm = _mapper.Map<NoticeViewModel>(notice);
            return Ok(noticeVm);
        }

        private bool IsAuthorized(string role) =>
            HttpContext.Session.GetString("UserRole") == role;

        private User? GetCurrentUser()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            return _userRepo.GetByEmail(email ?? "");
        }
    }
}