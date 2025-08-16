using Microsoft.AspNetCore.Http;
using SAS.Models;
using System.Collections.Generic;

namespace SAS.Repositories
{
    public interface IUserDetailsRepository
    {
        UserDetails GetByUserId(int userId);
        bool UpdateDetails(int userId, UserDetails updatedDetails, IFormFile? photo, List<IFormFile>? documents);
    }
}