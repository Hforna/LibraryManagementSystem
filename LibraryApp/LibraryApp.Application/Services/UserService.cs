using AutoMapper;
using LibraryApp.Application.Requests;
using LibraryApp.Application.Responses;
using LibraryApp.Domain.Dtos;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Exceptions;
using LibraryApp.Domain.Repositories;
using LibraryApp.Domain.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Application.Services
{
    public interface IUserService
    {
        public Task<UserResponse> CreateAccount(CreateUserRequest request, string uri);
    }

    public class UserService : IUserService
    {
        private readonly ILogger<IUserService> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IPasswordCryptography _cryptography;
        private readonly IEmailService _emailService;
        private readonly UserManager<User> _userManager;

        public UserService(ILogger<IUserService> logger, IMapper mapper, IUnitOfWork uow, IPasswordCryptography cryptography, 
            IEmailService emailService, UserManager<User> userManager)
        {
            _logger = logger;
            _mapper = mapper;
            _uow = uow;
            _cryptography = cryptography;
            _emailService = emailService;
            _userManager = userManager;
        }

        public async Task<UserResponse> CreateAccount(CreateUserRequest request, string uri)
        {
            var hasUserByEmail = await _uow.UserRepository.HasUserByEmailAsync(request.Email);

            if (hasUserByEmail)
            {
                _logger.LogError($"User with e-mail: {request.Email} already registered");
                throw new RequestException("Usuario com esse e-mail ja cadastrado");
            }

            var user = _mapper.Map<User>(request);
            user.NormalizedUserName = request.UserName.ToUpper();
            user.PasswordHash = _cryptography.Encrypt(request.Password);
            user.SecurityStamp = Guid.NewGuid().ToString();
            await _uow.GenericRepository.Add<User>(user);
            await _uow.Commit();

            await SendEmailConfirmation(user, uri);

            return _mapper.Map<UserResponse>(user);
        }

        private async Task SendEmailConfirmation(User user, string uri)
        {
            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var emailDto = new SendEmailDto(
                user.Email, 
                user.UserName, 
                "Confirme sua conta abaixo", $"Clique aqui para confirmar seu email: {uri}api/users/confirm-email?token{confirmationToken}&email={user.Email}");
            await _emailService.SendEmail(emailDto);
        }
    }
}
