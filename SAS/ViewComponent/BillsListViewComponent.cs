using Microsoft.AspNetCore.Mvc;
using SAS.Repositories;
using SAS.ViewModels;
using AutoMapper;
using System.Linq;

namespace SAS.ViewComponents
{
    public class BillsListViewComponent : ViewComponent
    {
        private readonly IBillRepository _billRepo;
        private readonly IMapper _mapper;

        public BillsListViewComponent(IBillRepository billRepo, IMapper mapper)
        {
            _billRepo = billRepo;
            _mapper = mapper;
        }

        public IViewComponentResult Invoke()
        {
            var bills = _billRepo.GetAll()
                .Select(b => _mapper.Map<BillViewModel>(b))
                .ToList();

            return View(bills);
        }
    }
}