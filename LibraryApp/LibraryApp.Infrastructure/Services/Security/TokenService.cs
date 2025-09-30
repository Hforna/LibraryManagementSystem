using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Exceptions;
using LibraryApp.Domain.Repositories;
using LibraryApp.Domain.Services;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryApp.Infrastructure.Services.Security
{
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

        public string GenerateRefreshToken() => Guid.NewGuid().ToString();

        public DateTime GetTimeToRefreshExpires() => DateTime.UtcNow.AddDays(_refreshExpiresAt);

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

        public Task<User?> GetUserByToken()
        {
            var token = _requestService.GetBearerToken()
               ?? throw new RequestException("Token não fornecido na requisição");

            var handler = new JwtSecurityTokenHandler();
            var read = handler.ReadJwtToken(token);
            var id = long.Parse(read.Claims.FirstOrDefault(d => d.Type == ClaimTypes.Sid)!.Value);
            var user = _uow.UserRepository.GetUserById(id);

            return user;
        }

        private SymmetricSecurityKey GenerateSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signKey));
        }
    }
}
