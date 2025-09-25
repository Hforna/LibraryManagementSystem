using LibraryApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace LibraryApp.Domain.Services
{
    public interface ITokenService
    {
        public string GenerateAccessToken(List<Claim> claims, long userId);
        public string GenerateRefreshToken();
        public DateTime GetTimeToRefreshExpires();
        public Task<User> GetUserByToken();
    }
}
