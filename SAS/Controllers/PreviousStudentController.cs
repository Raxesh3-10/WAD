using Microsoft.AspNetCore.Mvc;
using SAS.Repositories;
using SAS.ViewModels;
using AutoMapper;
using System.Linq;

namespace SAS.Controllers
{
    public class PreviousStudentController : Controller
    {
        private readonly IPreviousStudentRepository _previousStudentRepo;
        private readonly IMapper _mapper;

        public PreviousStudentController(IPreviousStudentRepository previousStudentRepo, IMapper mapper)
        {
            _previousStudentRepo = previousStudentRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var previousStudents = _previousStudentRepo.GetAll().ToList();

            var studentVms = previousStudents
                .Select(s => _mapper.Map<PreviousStudentViewModel>(s))
                .ToList();

            var grouped = studentVms
                .GroupBy(s => s.PassingYear)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(s => s.Std)
                          .ToDictionary(
                              sg => sg.Key,
                              sg => sg.GroupBy(s => s.Div)
                                      .ToDictionary(
                                          dg => dg.Key,
                                          dg => dg.ToList()
                                      )
                          )
                );

            return View(grouped);
        }
    }
}