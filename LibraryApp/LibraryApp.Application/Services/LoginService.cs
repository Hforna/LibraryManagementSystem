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
    // Interface que define o contrato do serviço de login
    public interface ILoginService
    {
        public Task<LoginResponse> DoLoginByApplication(LoginRequest request); // Login tradicional com email e senha
        public Task<LoginResponse> HandleGoogleCallback(string userName, string email, string? pictureUrl); // Login via Google OAuth (callback após autenticação)
    }

    // Implementação do serviço de login
    public class LoginService : ILoginService
    {
        private readonly UserManager<User> _userManager;  // Gerencia usuários do ASP.NET Identity (CRUD, validações, etc)
        private readonly ITokenService _tokenService; // Serviço para gerar e validar tokens JWT
        private readonly IPasswordCryptography _passwordCryptography; // Serviço para criptografia de senhas (hash e comparação)
        private readonly IUnitOfWork _uow; // Gerencia transações do banco de dados
        private readonly ILogger<ILoginService> _logger; // Logger para registrar eventos e erros

        // Construtor ele recebe dependências por injeção
        public LoginService(UserManager<User> userManager, ITokenService tokenService, 
            IPasswordCryptography passwordCryptography, IUnitOfWork uow, ILogger<ILoginService> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _passwordCryptography = passwordCryptography;
            _uow = uow;
            _logger = logger;
        }

        // Método para login tradicional (email + senha)
        public async Task<LoginResponse> DoLoginByApplication(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email); // Busca o usuário pelo email fornecido

            // Se não encontrar o usuário, lança exceção genérica
            if (user is null)
                throw new RequestException("Usuário ou senha inválidos");

            // Verifica se o email foi confirmado
            if (!user.EmailConfirmed)
            {
                _logger.LogError($"User with e-mail: {user.Email} was not confirmed"); // Registra no log que houve tentativa de login sem confirmação

                throw new RequestException("Confirme seu e-mail antes de realizar o login");
            }

            
            var isHashValid = _passwordCryptography.CompareHash(request.Password, user.PasswordHash);  // Compara o hash da senha armazenada no banco com a senha fornecida

            // Se a senha estiver incorreta, lança exceção genérica
            if (!isHashValid)
                throw new RequestException("Usuário ou senha inválidos");
    
            var accessToken = _tokenService.GenerateAccessToken(new List<Claim>(), user.Id); // Gera um novo Access Token (JWT) para o usuário
            user.RefreshToken = _tokenService.GenerateRefreshToken(); // Gera um novo Refresh Token (usado para renovar o Access Token)
            user.RefreshTokenExpiration = _tokenService.GetTimeToRefreshExpires(); // Define quando o Refresh Token vai expirar

            _uow.GenericRepository.Update<User>(user); // Atualiza o usuário no banco com os novos tokens            
            await _uow.Commit(); //salva as alterações no banco de dados

            // Retorna a resposta com os tokens
            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = user.RefreshToken,
                RefreshTokenExpiration = (DateTime)user.RefreshTokenExpiration
            };
        }

        // Método para processar callback do Google OAuth
        public async Task<LoginResponse> HandleGoogleCallback(string userName, string email, string? pictureUrl)
        {
            var user = await _uow.UserRepository.GetUserByEmailAsync(email); // Busca se já existe um usuário com este email

            // Se o usuário não existe, cria um novo
            if (user is null)
            {                
                user = new User() // Cria novo usuário com dados do Google
                {
                    UserName = userName,
                    Email = email,
                    EmailConfirmed = true,
                    NormalizedEmail = email.ToUpper(),
                    PasswordHash = "-"
                };

                await _uow.GenericRepository.Add<User>(user); // Adiciona o novo usuário ao banco
                await _uow.Commit();
            }

            // Gera tokens para o usuário (novo ou existente)
            var accessToken = _tokenService.GenerateAccessToken(new List<Claim>(), user.Id); 
            user.RefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiration = _tokenService.GetTimeToRefreshExpires();

            // Atualiza o usuário com os novos tokens
            _uow.GenericRepository.Update<User>(user);  
            await _uow.Commit();

            // Retorna a resposta com os tokens
            return new LoginResponse()
            {
                AccessToken = accessToken,
                RefreshToken = user.RefreshToken,
                RefreshTokenExpiration = (DateTime)user.RefreshTokenExpiration
            };
        }
    }
}