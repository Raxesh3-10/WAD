using Microsoft.AspNetCore.Mvc;
using SAS.Repositories;
using SAS.ViewModels;
using AutoMapper;
using System.Linq;
using System.Threading.Tasks;
using SAS.Models;

namespace SAS.ViewComponents
{
    public class StudentViewComponent : ViewComponent
    {
        private readonly IRepository<Student> _studentRepo;
        private readonly IMapper _mapper;

        public StudentViewComponent(IRepository<Student> studentRepo, IMapper mapper)
        {
            _studentRepo = studentRepo;
            _mapper = mapper;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var students = _studentRepo.GetAll().ToList();
            var studentVms = students.Select(s => _mapper.Map<StudentViewModel>(s)).ToList();

            return View(studentVms);
        }
    }
}
