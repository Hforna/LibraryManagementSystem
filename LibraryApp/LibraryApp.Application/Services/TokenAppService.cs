using LibraryApp.Application.Requests;
using LibraryApp.Application.Responses;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Exceptions;
using LibraryApp.Domain.Repositories;
using LibraryApp.Domain.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Application.Services
{
    // Interface que define o contrato do serviço de tokens
    public interface ITokenAppService
    {
        public Task<LoginResponse> RefreshToken(RefreshTokenRequest request); // Método para renovar o Access Token usando o Refresh Token
    }

    // Implementação do serviço de gerenciamento de tokens
    public class TokenAppService : ITokenAppService
    {
        private readonly ITokenService _tokenService; // Serviço para operações com tokens (gerar, validar, extrair claims)
        private readonly IUnitOfWork _uof; // Gerencia transações e repositórios
        private readonly ILogger<ITokenAppService> _logger; // Logger para registrar eventos

        // Construtor ele recebe dependências por injeção
        public TokenAppService(ITokenService tokenService, IUnitOfWork uof, ILogger<ITokenAppService> logger)
        {
            _tokenService = tokenService;
            _uof = uof;
            _logger = logger;
        }

        // Método para renovar tokens quando o Access Token expira
        public async Task<LoginResponse> RefreshToken(RefreshTokenRequest request)
        {
            var user = await _tokenService.GetUserByToken(); // Obtém o usuário através do Access Token atual (mesmo que expirado)
            var userRefreshToken = user.RefreshToken; // Pega o Refresh Token armazenado no banco de dados

            // Valida se o Refresh Token existe e se corresponde ao enviado na requisição
            if (userRefreshToken is null || request.RefreshToken != userRefreshToken)
                throw new RequestException("Refresh token invalido");

            user.RefreshToken = _tokenService.GenerateRefreshToken(); // Gera um novo Refresh Token
            user.RefreshTokenExpiration = _tokenService.GetTimeToRefreshExpires(); // Atualiza a data de expiração do novo Refresh Token

            _uof.GenericRepository.Update<User>(user); // Atualiza o usuário no banco com os novos dados
            await _uof.Commit(); // Persiste as alterações no banco de dados

            _logger.LogInformation("New refresh token generated for user: {user.Id}", user.Id); // Registra no log que um novo Refresh Token foi gerado

            var claims = _tokenService.GetTokenClaims(); // Extrai as claims (permissões, roles, etc) do token anterior

            var accessToken = _tokenService.GenerateAccessToken(claims, user.Id); // Gera um novo Access Token com as claims e ID do usuário

            // Retorna a resposta com os novos tokens
            return new LoginResponse()
            {
                AccessToken = accessToken,
                RefreshToken = user.RefreshToken,
                RefreshTokenExpiration = (DateTime)user.RefreshTokenExpiration
            };
        }
    }
}