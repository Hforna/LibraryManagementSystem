using AutoMapper;
using LibraryApp.Application.Requests;
using LibraryApp.Application.Responses;
using LibraryApp.Domain.Dtos;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Exceptions;
using LibraryApp.Domain.Repositories;
using LibraryApp.Domain.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Security.Authentication;
using System.Text;

namespace LibraryApp.Application.Services
{
    public interface IUserService
    {
        public Task<UserResponse> CreateAccount(CreateUserRequest request, string uri);
        public Task ConfirmEmail(string email, string token);
    }

    public class UserService : IUserService
    {
        private readonly ILogger<IUserService> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IEmailService _emailService;
        private readonly UserManager<User> _userManager;

        public UserService(
            ILogger<IUserService> logger,
            IMapper mapper,
            IUnitOfWork uow,
            IEmailService emailService,
            UserManager<User> userManager)
        {
            _logger = logger;
            _mapper = mapper;
            _uow = uow;
            _emailService = emailService;
            _userManager = userManager;
        }

        public async Task ConfirmEmail(string email, string token)
        {
            var userByEmail = await _userManager.FindByEmailAsync(email);

            if (userByEmail is null)
                throw new RequestException("Usuario não foi encontrado");

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

            var tokenIsValid = await _userManager.ConfirmEmailAsync(userByEmail, decodedToken);

            if (!tokenIsValid.Succeeded)
            {
                var exception = new AuthenticationException("Token fornecido não é valido");
                _logger.LogError(message: $"Token {token} is not valid to email {email}", exception: exception);
                throw exception;
            }
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
            user.UserName = request.UserName;
            user.EmailConfirmed = false;

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError($"Erro ao criar usuário {request.Email}: {errors}");
                throw new RequestException("Não foi possível criar o usuário");
            }

            await SendEmailConfirmation(user, uri);

            return _mapper.Map<UserResponse>(user);
        }

        private async Task SendEmailConfirmation(User user, string uri)
        {
            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken));

            var emailDto = new SendEmailDto(
                user.Email,
                user.UserName,
                "Confirme sua conta abaixo",
                $"Clique aqui para confirmar seu email: {uri}api/users/confirm/email?token={encodedToken}&email={user.Email}"
            );

            await _emailService.SendEmail(emailDto);
        }
    }
}
