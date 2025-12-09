using LibraryApp.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Application
{
    // Classe para configuração de injeção de dependências da camada Application
    public static class DependencyInjection
    {
        // Permite chamar o services.AddApplication(configuration)
        public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            AddMapper(services); // Registra o AutoMapper
            AddServices(services); // Registra todos os serviços da aplicação
        }

        // Método privado para configurar o AutoMapper
        static void AddMapper(IServiceCollection services)
        {
            services.AddAutoMapper(d => d.AddProfile(new AutoMapperService())); // Adiciona o AutoMapper ao container de DI
        }

        // Método privado para registrar todos os serviços da camada Application
        static void AddServices(IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>(); // Registra serviço de usuários (criação de conta, confirmação de email)
            services.AddScoped<ILoginService, LoginService>(); // Registra serviço de login (autenticação)
            services.AddScoped<ITokenAppService, TokenAppService>(); // Registra serviço de tokens (refresh token, renovação)
            services.AddScoped<IBookService, BookService>(); // Registra serviço de livros (CRUD de livros)
            services.AddScoped<ICommentService, CommentService>(); // Registra serviço de comentários (criar, deletar, listar comentários)
            services.AddScoped<ICategoryService, CategoryService>(); // Registra serviço de categorias (gerenciamento de categorias de livros)
            services.AddScoped<IFileService, FileService>(); // Registra serviço de arquivos (upload, download de arquivos)
        }
    }
}