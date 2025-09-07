using Microsoft.AspNetCore.Mvc;
using SAS.Repositories;
using SAS.ViewModels;
using AutoMapper;
using System.Linq;
using SAS.Models;

namespace SAS.ViewComponents
{
    public class NoticeViewComponent : ViewComponent
    {
        private readonly IRepository<Notice> _noticeRepo;
        private readonly IMapper _mapper;

        public NoticeViewComponent(IRepository<Notice> noticeRepo, IMapper mapper)
        {
            _noticeRepo = noticeRepo;
            _mapper = mapper;
        }

        public IViewComponentResult Invoke()
        {
            var notices = _noticeRepo.GetAll().ToList();
            var noticeVms = notices.Select(n => _mapper.Map<NoticeViewModel>(n)).ToList();
            return View(noticeVms); // Views/Shared/Components/Notice/Default.cshtml
        }
    }
}
