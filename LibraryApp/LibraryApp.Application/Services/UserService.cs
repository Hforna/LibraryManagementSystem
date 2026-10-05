using AutoMapper;
using LibraryApp.Application.Requests;
using LibraryApp.Application.Responses;
using LibraryApp.Domain.Constants;
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
        public Task ConfirmEmail(string email, string token); // Confirma o email do usuário através do token enviado
        public Task<UserResponse> CreateUserByAdmin(AdminCreateUserRequest request); // Cria a conta de um usuário (estudante ou administrador); uso exclusivo do administrador
        public Task ChangePassword(ChangePasswordRequest request); // Altera a senha do usuário autenticado, exigindo a senha atual
        public Task ForgotPassword(string email, string baseUri); // Envia por e-mail o link de redefinição de senha (se a conta existir)
        public Task ResetPassword(ResetPasswordRequest request); // Redefine a senha usando o token recebido por e-mail
    }

    // Implementação do serviço de usuários
    public class UserService : IUserService
    {
       
        private readonly ILogger<IUserService> _logger; // Logger para registrar eventos e erros
        private readonly IMapper _mapper; // AutoMapper para converter entre entidades e DTOs
        private readonly IUnitOfWork _uow; // Unit of Work para gerenciar transações             
        private readonly UserManager<User> _userManager; // UserManager do ASP.NET Identity para gerenciar usuários        
        private readonly IPasswordCryptography _passwordCryptography; // Serviço para criptografia de senhas
        private readonly ITokenService _tokenService; // Serviço para identificar o usuário autenticado pelo token
        private readonly IEmailService _emailService; // Serviço para envio de emails

        // Construtor ele recebe todas as dependências por injeção
        public UserService(
            ILogger<IUserService> logger,
            IMapper mapper,
            IUnitOfWork uow,
            UserManager<User> userManager,
            IPasswordCryptography passwordCryptography,
            ITokenService tokenService,
            IEmailService emailService)
        {
            _logger = logger;
            _mapper = mapper;
            _uow = uow;
            _userManager = userManager;
            _passwordCryptography = passwordCryptography;
            _tokenService = tokenService;
            _emailService = emailService;
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

        // Método para um administrador criar a conta de um usuário (estudante ou administrador).
        // Não existe auto-cadastro: toda conta do sistema é criada por um administrador.
        public async Task<UserResponse> CreateUserByAdmin(AdminCreateUserRequest request)
        {
            // Valida o papel solicitado (aceita "Student" ou "Admin", sem diferenciar maiúsculas/minúsculas)
            var role = new[] { Roles.Student, Roles.Admin }
                .FirstOrDefault(r => string.Equals(r, request.Role, StringComparison.OrdinalIgnoreCase));

            if (role is null)
                throw new RequestException($"Papel inválido. Use \"{Roles.Student}\" ou \"{Roles.Admin}\"");

            var hasUserByEmail = await _uow.UserRepository.HasUserByEmailAsync(request.Email); // Verifica se já existe um usuário com este email

            if (hasUserByEmail)
            {
                _logger.LogError($"User with e-mail: {request.Email} already registered");
                throw new RequestException("Usuario com esse e-mail ja cadastrado");
            }

            var user = _mapper.Map<CreateUserRequest, User>(request); // Mapeia os dados da requisição para a entidade User
            user.PasswordHash = _passwordCryptography.Encrypt(request.Password); // Criptografa a senha antes de salvar
            user.UserName = request.UserName;
            user.EmailConfirmed = true; // Conta criada diretamente por um administrador, não precisa de confirmação por email

            var result = await _userManager.CreateAsync(user); // Cria o usuário usando o UserManager

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError($"Erro ao criar usuário {request.Email}: {errors}");
                throw new RequestException("Não foi possível criar o usuário");
            }

            await _userManager.AddToRoleAsync(user, role); // Concede o papel escolhido pelo administrador

            return _mapper.Map<UserResponse>(user);
        }

        // Método para o usuário autenticado alterar a própria senha
        public async Task ChangePassword(ChangePasswordRequest request)
        {
            // Busca o usuário pelo UserManager (mesmo DbContext da requisição) usando apenas o id do token,
            // pois o TokenService usa um escopo próprio e a entidade dele não pode ser salva aqui
            var userId = _tokenService.GetUserIdByToken() ?? throw new UnauthorizedException("Usuario deve estar autenticado para acessar esse recurso");
            var user = await _userManager.FindByIdAsync(userId.ToString()) ?? throw new UnauthorizedException("Usuario deve estar autenticado para acessar esse recurso");

            // Contas criadas pelo Google não têm senha local (hash "-"): nesse caso a senha atual nunca confere
            var currentPasswordIsValid = user.PasswordHash.StartsWith("$2")
                && _passwordCryptography.CompareHash(request.CurrentPassword, user.PasswordHash);

            if (!currentPasswordIsValid)
                throw new RequestException("Senha atual incorreta");

            if (request.CurrentPassword == request.NewPassword)
                throw new RequestException("A nova senha deve ser diferente da senha atual");

            user.PasswordHash = _passwordCryptography.Encrypt(request.NewPassword); // Criptografa a nova senha antes de salvar
            await _userManager.UpdateSecurityStampAsync(user); // Salva o usuário e invalida links de redefinição de senha pendentes
        }

        // Método para enviar o link de redefinição de senha por e-mail.
        // Não informa se o e-mail existe ou não, para não permitir descobrir quais e-mails estão cadastrados.
        public async Task ForgotPassword(string email, string baseUri)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                _logger.LogWarning($"Password reset requested for unregistered e-mail: {email}");
                return;
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user); // Gera um token de uso único, com validade limitada
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken)); // Codifica o token em Base64URL para poder ser enviado na URL
            var resetLink = $"{baseUri}reset-password.html?email={Uri.EscapeDataString(user.Email!)}&token={encodedToken}";

            var emailDto = new SendEmailDto(
                user.Email,
                user.UserName,
                "Redefinição de senha",
                $"Recebemos um pedido para redefinir a sua senha. <a href=\"{resetLink}\">Clique aqui para criar uma nova senha</a>. " +
                "O link expira em 1 hora. Se você não fez esse pedido, ignore este e-mail: sua senha continua a mesma."
            );

            await _emailService.SendEmail(emailDto);
        }

        // Método para redefinir a senha usando o token recebido por e-mail
        public async Task ResetPassword(ResetPasswordRequest request)
        {
            var invalidLinkMessage = "Link de redefinição inválido ou expirado. Solicite um novo link";

            var user = await _userManager.FindByEmailAsync(request.Email) ?? throw new RequestException(invalidLinkMessage);

            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token)); // Decodifica o token que veio codificado em Base64URL
            }
            catch (FormatException)
            {
                throw new RequestException(invalidLinkMessage);
            }

            // As senhas são guardadas com BCrypt (fora do PasswordHasher do Identity), então apenas validamos o token pelo Identity
            var tokenIsValid = await _userManager.VerifyUserTokenAsync(user,
                _userManager.Options.Tokens.PasswordResetTokenProvider,
                UserManager<User>.ResetPasswordTokenPurpose,
                decodedToken);

            if (!tokenIsValid)
            {
                _logger.LogWarning($"Invalid or expired password reset token for e-mail: {request.Email}");
                throw new RequestException(invalidLinkMessage);
            }

            user.PasswordHash = _passwordCryptography.Encrypt(request.NewPassword); // Criptografa a nova senha antes de salvar
            user.RefreshToken = _tokenService.GenerateRefreshToken(); // Troca o refresh token por um que ninguém possui: encerra as sessões abertas
            await _userManager.UpdateSecurityStampAsync(user); // Salva o usuário e invalida o token (não pode ser reutilizado)
        }
    }
}