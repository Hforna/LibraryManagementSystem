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
    // Classe de configuração do AutoMapper
    public class AutoMapperService : Profile
    {
        // Construtor onde são definidos todos os mapeamentos
        public AutoMapperService()
        {
            CreateMap<CreateUserRequest, User>() // Converte dados da requisição de criação em entidade User
                .ForMember(d => d.PasswordHash, f => f.Ignore()) // Ignora o PasswordHash porque será criptografado separadamente
                .ForMember(d => d.UserName, f => f.MapFrom(d => d.UserName)); // Mapeia explicitamente UserName de origem para destino

            CreateMap<User, UserResponse>(); // Converte entidade User em DTO de resposta

            CreateMap<BookRequest, Book>(); // Converte requisição de livro em entidade Book

            CreateMap<Book, BookResponse>(); // Converte entidade Book em resposta completa

            CreateMap<Comment, CommentResponse>(); // Converte entidade Comment em resposta

            CreateMap<Pagination<Comment>, CommentsPaginatedResponse>(); // Converte resultado paginado de comentários

            CreateMap<Pagination<Book>, BooksPaginatedResponse>(); // Converte resultado paginado de livros

            CreateMap<Book, BookShortResponse>(); // Converte entidade Book em resposta resumida

            CreateMap<Category, CategoryResponse>(); // Converte entidade Category em resposta
        }
    }
}