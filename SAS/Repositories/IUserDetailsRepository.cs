using Microsoft.AspNetCore.Http;
using SAS.Models;
using System;
using System.Collections.Generic;

namespace SAS.Repositories
{
    public interface IUserDetailsRepository
    {
        UserDetails? GetByUserId(Guid userId);
        UserDetails CreateEmptyDetails(Guid userId);
        bool UpdateDetails(Guid userId, UserDetails updatedDetails, IFormFile? photo, List<IFormFile>? documents);
        bool DeleteDetails(Guid userId);
        public IEnumerable<UserDetails> GetAll();
    }
}