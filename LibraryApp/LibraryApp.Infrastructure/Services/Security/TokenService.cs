using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Exceptions;
using LibraryApp.Domain.Repositories;
using LibraryApp.Domain.Services;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApp.Infrastructure.Services.Security
{
    /// <summary>
    /// Serviço responsável pela geração, validação e leitura de tokens JWT.
    /// Gerencia também o refresh token e a recuperação de informações do usuário autenticado.
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly string _signKey;
        private readonly int _expiresAt;
        private readonly int _refreshExpiresAt;
        private readonly IUnitOfWork _uow;
        private readonly IRequestService _requestService;
        
        public TokenService(string signKey, int expiresAt, int refreshExpiresAt, IUnitOfWork uow, IRequestService requestService)
        {
            _signKey = signKey;
            _expiresAt = expiresAt;
            _refreshExpiresAt = refreshExpiresAt;
            _requestService = requestService;
            _uow = uow;
        }

        /// <summary>
        /// Gera um token JWT de acesso com as claims fornecidas.
        /// </summary>
        /// <param name="claims">Lista de claims que serão incluídas no token.</param>
        /// <param name="userId">Identificador do usuário autenticado.</param>
        /// <returns>Token JWT assinado e codificado.</returns>
        public string GenerateAccessToken(List<Claim> claims, long userId)
        {
            claims.Add(new Claim(ClaimTypes.Sid, userId.ToString()));

            var descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expiresAt),
                SigningCredentials = new SigningCredentials(GenerateSecurityKey(), SecurityAlgorithms.HmacSha256),
            };

            var handler = new JwtSecurityTokenHandler();
            var create = handler.CreateToken(descriptor);

            return handler.WriteToken(create);
        }

        /// <summary>
        /// Gera um refresh token aleatório.
        /// </summary>
        /// <returns>String única representando o refresh token.</returns>
        public string GenerateRefreshToken() => Guid.NewGuid().ToString();

        /// <summary>
        /// Obtém a data e hora de expiração do refresh token.
        /// </summary>
        /// <returns>Data e hora UTC em que o refresh token expira.</returns>
        public DateTime GetTimeToRefreshExpires() => DateTime.UtcNow.AddDays(_refreshExpiresAt);

        /// <summary>
        /// Obtém as claims do token JWT presente na requisição atual.
        /// </summary>
        /// <returns>Lista de claims extraídas do token.</returns>
        /// <exception cref="RequestException">Lançada quando o token não é fornecido.</exception>
        public List<Claim> GetTokenClaims()
        {
            var token = _requestService.GetBearerToken() 
                ?? throw new RequestException("Token não fornecido na requisição");

            var @params = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = GenerateSecurityKey(),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            var handler = new JwtSecurityTokenHandler();
            var result = handler.ValidateToken(token, @params, out SecurityToken validated);

            return result.Claims.ToList();
        }

        /// <summary>
        /// Obtém o usuário autenticado a partir do token presente na requisição.
        /// </summary>
        /// <returns>Usuário correspondente ao ID contido no token.</returns>
        /// <exception cref="RequestException">Lançada quando o token não é fornecido.</exception>
        public async Task<User?> GetUserByToken()
        {
            var token = _requestService.GetBearerToken();

            if (string.IsNullOrEmpty(token))
                return null!;

            var handler = new JwtSecurityTokenHandler();
            var read = handler.ReadJwtToken(token);
            var id = long.Parse(read.Claims.FirstOrDefault(d => d.Type == ClaimTypes.Sid)!.Value);
            var user = await _uow.UserRepository.GetUserById(id);

            return user;
        }

        public long? GetUserIdByToken()
        {
            var token = _requestService.GetBearerToken();

            if (token is null)
                return null;

            var handler = new JwtSecurityTokenHandler();
            var read = handler.ReadJwtToken(token);
            var id = long.Parse(read.Claims.FirstOrDefault(d => d.Type == ClaimTypes.Sid)!.Value);

            return id;
        }

        /// <summary>
        /// Gera a chave de segurança simétrica utilizada na assinatura e validação dos tokens JWT.
        /// </summary>
        /// <returns><see cref="SymmetricSecurityKey"/> gerada a partir da chave secreta configurada.</returns>
        private SymmetricSecurityKey GenerateSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signKey));
        }
    }
}
