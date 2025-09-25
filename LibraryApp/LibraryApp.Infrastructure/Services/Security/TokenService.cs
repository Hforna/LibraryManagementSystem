using LibraryApp.Domain.Entities;
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

        public TokenService(string signKey, int expiresAt, int refreshExpiresAt)
        {
            _signKey = signKey;
            _expiresAt = expiresAt;
            _refreshExpiresAt = refreshExpiresAt;
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

        public Task<User> GetUserByToken()
        {
            throw new NotImplementedException();
        }

        private SymmetricSecurityKey GenerateSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signKey));
        }
    }
}
