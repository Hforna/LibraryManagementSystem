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
    // Interface que define o contrato do serviço de usuários
    public interface IUserService
    {        
        public Task<UserResponse> CreateAccount(CreateUserRequest request, string uri);  // Cria uma nova conta de usuário e envia email de confirmação        
        public Task ConfirmEmail(string email, string token); // Confirma o email do usuário através do token enviado
    }

    // Implementação do serviço de usuários
    public class UserService : IUserService
    {
       
        private readonly ILogger<IUserService> _logger; // Logger para registrar eventos e erros
        private readonly IMapper _mapper; // AutoMapper para converter entre entidades e DTOs
        private readonly IUnitOfWork _uow; // Unit of Work para gerenciar transações             
        private readonly IEmailService _emailService; // Serviço para envio de emails        
        private readonly UserManager<User> _userManager; // UserManager do ASP.NET Identity para gerenciar usuários        
        private readonly IPasswordCryptography _passwordCryptography; // Serviço para criptografia de senhas

        // Construtor ele recebe todas as dependências por injeção
        public UserService(
            ILogger<IUserService> logger,
            IMapper mapper,
            IUnitOfWork uow,
            IEmailService emailService,
            UserManager<User> userManager,
            IPasswordCryptography passwordCryptography)
        {
            _logger = logger;
            _mapper = mapper;
            _uow = uow;
            _emailService = emailService;
            _userManager = userManager;
            _passwordCryptography = passwordCryptography;
        }

        // Método para confirmar o email do usuário
        public async Task ConfirmEmail(string email, string token)
        {            
            var userByEmail = await _userManager.FindByEmailAsync(email); // Busca o usuário pelo email

            // Se não encontrar o usuário, lança exceção
            if (userByEmail is null)
                throw new RequestException("Usuario não foi encontrado");
                        
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)); // Decodifica o token que veio codificado em Base64URL
            var tokenIsValid = await _userManager.ConfirmEmailAsync(userByEmail, decodedToken); // Tenta confirmar o email usando o token decodificado

            // Se a validação falhar, loga o erro e lança exceção
            if (!tokenIsValid.Succeeded)
            {
                var exception = new UnauthorizedException("Token fornecido não é valido");
                _logger.LogError(message: $"Token {token} is not valid to email {email}", exception: exception);
                throw exception;
            }
        }

        // Método para criar uma nova conta de usuário
        public async Task<UserResponse> CreateAccount(CreateUserRequest request, string uri)
        {            
            var hasUserByEmail = await _uow.UserRepository.HasUserByEmailAsync(request.Email); // Verifica se já existe um usuário com este email

            // Se já existe, loga e lança exceção
            if (hasUserByEmail)
            {
                _logger.LogError($"User with e-mail: {request.Email} already registered");
                throw new RequestException("Usuario com esse e-mail ja cadastrado");
            }
            
            var user = _mapper.Map<User>(request); // Mapeia os dados da requisição para a entidade User            
            user.PasswordHash = _passwordCryptography.Encrypt(request.Password); // Criptografa a senha antes de salvar            
            user.UserName = request.UserName; // Define o nome de usuário            
            user.EmailConfirmed = false; // Email começa como não confirmado            
            var result = await _userManager.CreateAsync(user); // Cria o usuário usando o UserManager

            // Se a criação falhar (ex: senha fraca, email inválido)
            if (!result.Succeeded)
            {                
                var errors = string.Join(", ", result.Errors.Select(e => e.Description)); // Junta todas as mensagens de erro em uma string                
                _logger.LogError($"Erro ao criar usuário {request.Email}: {errors}"); // Loga os erros detalhados                
                throw new RequestException("Não foi possível criar o usuário"); // Lança exceção genérica para o cliente
            }
            
            await SendEmailConfirmation(user, uri); // Envia email de confirmação para o usuário
            
            return _mapper.Map<UserResponse>(user); // Retorna os dados do usuário criado (sem a senha)
        }

        // Método privado auxiliar para enviar email de confirmação
        private async Task SendEmailConfirmation(User user, string uri)
        {            
            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user); // Gera um token de confirmação de email único para este usuário            
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken)); // Codifica o token em Base64URL para poder ser enviado na URL            
            var emailDto = new SendEmailDto( // Cria o DTO com os dados do email
                user.Email, // Para quem vai o email                     
                user.UserName, // Nome do destinatário                  
                "Confirme sua conta abaixo", 
                $"Clique aqui para confirmar seu email: {uri}api/users/confirm/email?token={encodedToken}&email={user.Email}"
            );
            
            await _emailService.SendEmail(emailDto); // Envia o email usando o serviço de email
        }
    }
}