using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Repositories;
using LibraryApp.Domain.Services;
using LibraryApp.Infrastructure.Context;
using LibraryApp.Infrastructure.Services;
using LibraryApp.Infrastructure.Services.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Infrastructure
{
    public static class DependenciesConfiguration
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            AddDbContext(services, configuration);
            AddRepositories(services);
            AddServices(services, configuration);
        }

        static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DataContext>(d => d.UseNpgsql(configuration.GetConnectionString("postgres")));

            services.AddDataProtection();

            services.AddIdentityCore<User>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
             .AddRoles<Role>()
             .AddEntityFrameworkStores<DataContext>()
             .AddDefaultTokenProviders()
             .AddUserManager<UserManager<User>>();
        }

        static void AddRepositories(IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IGenericRepository, GenericRepository>();
        }

        static void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IPasswordCryptography, PasswordCryptography>();
            services.AddSingleton<IEmailService, EmailService>();

            services.Configure<SmtpSettings>(d => configuration.GetSection("services:smtp"));
        }
    }
}
