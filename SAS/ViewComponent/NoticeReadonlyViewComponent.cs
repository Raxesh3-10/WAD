using Microsoft.AspNetCore.Mvc;
using SAS.Repositories;
using SAS.ViewModels;
using AutoMapper;
using System.Linq;
using System.Threading.Tasks;
using SAS.Models;

namespace SAS.ViewComponents
{
    public class NoticeReadonlyViewComponent : ViewComponent
    {
        private readonly IRepository<Notice> _noticeRepo;
        private readonly IMapper _mapper;

        public NoticeReadonlyViewComponent(IRepository<Notice> noticeRepo, IMapper mapper)
        {
            _noticeRepo = noticeRepo;
            _mapper = mapper;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var notices = _noticeRepo.GetAll().ToList();
            var noticeVms = notices.Select(n => _mapper.Map<NoticeViewModel>(n)).ToList();
            return View(noticeVms);
        }
    }
}
