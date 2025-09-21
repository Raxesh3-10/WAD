using Microsoft.AspNetCore.Mvc;
using SAS.Repositories;
using SAS.ViewModels;
using AutoMapper;
using System.Linq;
using SAS.Models;

namespace SAS.ViewComponents
{
    public class StudentViewComponent : ViewComponent
    {
        private readonly IStudentRepository _studentRepo;
        private readonly IMapper _mapper;

        public StudentViewComponent(IStudentRepository studentRepo, IMapper mapper)
        {
            _studentRepo = studentRepo;
            _mapper = mapper;
        }

        public IViewComponentResult Invoke()
        {
            var students = _studentRepo.GetAll().ToList();
            var studentVms = students.Select(s => _mapper.Map<StudentViewModel>(s)).ToList();

            return View(studentVms);
        }
    }
}
