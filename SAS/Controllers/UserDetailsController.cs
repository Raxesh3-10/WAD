using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SAS.Models;
using SAS.Repositories;
using SAS.ViewModels;
using AutoMapper;
using System.Collections.Generic;

namespace SAS.Controllers
{
    public class UserDetailsController : ControllerBase
    {
        private readonly IUserDetailsRepository _repository;
        private readonly IMapper _mapper;

        public UserDetailsController(IUserDetailsRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public IActionResult GetDetails(int userId)
        {
            if (userId <= 0)
                return BadRequest(new { message = "Invalid user ID format" });

            var details = _repository.GetByUserId(userId);
            if (details == null)
                return NotFound(new { message = "Details not found" });

            var vm = _mapper.Map<UserDetailsViewModel>(details);
            return Ok(vm);
        }

        [Consumes("multipart/form-data")]
        public IActionResult UpdateDetails(
            int userId,
            [FromForm] UserDetailsViewModel updatedDetails,
            [FromForm] IFormFile? photo,
            [FromForm] List<IFormFile>? documents)
        {
            if (userId <= 0)
                return BadRequest(new { message = "Invalid user ID format" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = _mapper.Map<UserDetails>(updatedDetails);

            var success = _repository.UpdateDetails(userId, entity, photo, documents);

            if (!success)
                return NotFound(new { message = "User not found" });

            return Ok(new { message = "Details updated successfully" });
        }
    }
}