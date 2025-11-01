using AutoMapper;
using LibraryApp.Application.Requests;
using LibraryApp.Application.Responses;
using LibraryApp.Domain.Entities;
using Pagination.EntityFrameworkCore.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Application
{
    public class AutoMapperService : Profile
    {
        public AutoMapperService()
        {
            CreateMap<CreateUserRequest, User>()
                .ForMember(d => d.PasswordHash, f => f.Ignore())
                .ForMember(d => d.UserName, f => f.MapFrom(d => d.UserName));

            CreateMap<User, UserResponse>();

            CreateMap<Book,  BookResponse>();

            CreateMap<Comment, CommentResponse>();

            CreateMap<Pagination<Comment>, CommentsPaginatedResponse>();
        }
    }
}
