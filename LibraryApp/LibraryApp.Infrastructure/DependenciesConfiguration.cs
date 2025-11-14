using Dropbox.Api;
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
using SendGrid;
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

        static void AddDbContext(IServiceCollection services, IConfiguration configuration)
        {
            //configura o serviços sobre o banco de dados e persistencia de dados

            services.AddDbContext<DataContext>(d => d.UseNpgsql(configuration.GetConnectionString("postgres")));

            services.AddDataProtection();

            //configuração de serviço para identity, uma biblioteca para gerenciar persistencia, autenticação e autorização de usuarios
            services.AddIdentityCore<User>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;

                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultProvider;
            })
             .AddRoles<Role>()
             .AddEntityFrameworkStores<DataContext>()
             .AddDefaultTokenProviders()
             .AddUserManager<UserManager<User>>();
        }

        static void AddRepositories(IServiceCollection services)
        {
            //configura os serviços como injecção de dependençia

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IGenericRepository, GenericRepository>();
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
        }

        static void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            //serviçp de armazenamento
            string dropBoxKey = configuration.GetValue<string>("services:storage:dropBox:apiKey")!;
            services.AddScoped<IStorageService, DropBoxStorageService>(d => new DropBoxStorageService(dropBoxKey));

            services.AddSingleton<IPasswordCryptography, PasswordCryptography>();

            services.AddScoped<IRequestService, RequestService>();

            //Configura o serviço de token para autenticação
            var signKey = configuration.GetValue<string>("services:jwt:signKey")!;
            var expiresAt = configuration.GetValue<int>("services:jwt:expiresAt");
            var refreshExpiresAt = configuration.GetValue<int>("services:jwt:refreshExpiresAt");
            services.AddScoped<ITokenService, TokenService>(d => new TokenService(
                signKey,
                expiresAt, 
                refreshExpiresAt, 
                d.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>(),
                d.CreateScope().ServiceProvider.GetRequiredService<IRequestService>()));

            var sendGridKey = configuration.GetValue<string>("services:sendgrid:apiKey");
            services.AddScoped<SendGridClient>(d => new SendGridClient(sendGridKey));

            services.AddSingleton<IEmailService, EmailService>(d =>
            {
                using var scope = d.CreateScope();

                return new EmailService(scope.ServiceProvider.GetRequiredService<SendGridClient>(), 
                    configuration.GetValue<string>("services:sendgrid:email")!, 
                    configuration.GetValue<string>("services:sendgrid:userName")!);
            });
        }
    }
}
