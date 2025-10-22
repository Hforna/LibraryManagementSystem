using LibraryApp.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Application
{
    public static class DependencyInjection
    {
        public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            AddMapper(services);
            AddServices(services);
        }

        static void AddMapper(IServiceCollection services)
        {
            services.AddAutoMapper(d => d.AddProfile(new AutoMapperService()));
        }

        static void AddServices(IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<ITokenAppService, TokenAppService>();
            services.AddScoped<IBookService, BookService>();

            services.AddScoped<IFileService, FileService>();
        }
    }
}
