using LibraryApp.Application.Requests;
using LibraryApp.Application.Responses;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Exceptions;
using LibraryApp.Domain.Repositories;
using LibraryApp.Domain.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace LibraryApp.Application.Services
{
    public interface ILoginService
    {
        public Task<LoginResponse> DoLoginByApplication(LoginRequest request);
    }

    public class LoginService : ILoginService
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IPasswordCryptography _passwordCryptography;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<ILoginService> _logger;

        public LoginService(UserManager<User> userManager, ITokenService tokenService, 
            IPasswordCryptography passwordCryptography, IUnitOfWork uow, ILogger<ILoginService> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _passwordCryptography = passwordCryptography;
            _uow = uow;
            _logger = logger;
        }

        public async Task<LoginResponse> DoLoginByApplication(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
                throw new RequestException("Usuário ou senha inválidos");

            if (!user.EmailConfirmed)
            {
                _logger.LogError($"User with e-mail: {user.Email} was not confirmed");

                throw new RequestException("Confirme seu e-mail antes de realizar o login");
            }

            //Compara o hash da senha do banco de dados com a senha da requisição para verificar ser é valida
            var isHashValid = _passwordCryptography.CompareHash(request.Password, user.PasswordHash);

            if (!isHashValid)
                throw new RequestException("Usuário ou senha inválidos");
    
            var accessToken = _tokenService.GenerateAccessToken(new List<Claim>(), user.Id);
            user.RefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiration = _tokenService.GetTimeToRefreshExpires();

            _uow.GenericRepository.Update<User>(user);
            //salva as alterações no banco de dados
            await _uow.Commit();

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = user.RefreshToken,
                RefreshTokenExpiration = (DateTime)user.RefreshTokenExpiration
            };
        }
    }
}
