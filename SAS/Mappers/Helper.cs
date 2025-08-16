using AutoMapper;
using SAS.Models;
using SAS.ViewModels;
using System;

namespace SAS.Mappers
{
    public class Helper : Profile
    {
        public Helper()
        {
            CreateMap<Notice, NoticeViewModel>().ReverseMap();
            CreateMap<Student, StudentViewModel>().ReverseMap();
            CreateMap<UserDetails, UserDetailsViewModel>().ReverseMap();
            CreateMap<User, UserViewModel>().ReverseMap();
        }
    }
}