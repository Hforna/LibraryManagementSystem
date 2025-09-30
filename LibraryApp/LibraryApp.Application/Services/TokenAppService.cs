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
    public interface ITokenAppService
    {
        public Task<LoginResponse> RefreshToken(RefreshTokenRequest request);
    }

    public class TokenAppService : ITokenAppService
    {
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _uof;
        private readonly ILogger<ITokenAppService> _logger;

        public TokenAppService(ITokenService tokenService, IUnitOfWork uof, ILogger<ITokenAppService> logger)
        {
            _tokenService = tokenService;
            _uof = uof;
            _logger = logger;
        }

        public async Task<LoginResponse> RefreshToken(RefreshTokenRequest request)
        {
            var user = await _tokenService.GetUserByToken();
            var userRefreshToken = user.RefreshToken;

            if (userRefreshToken is null || request.RefreshToken != userRefreshToken)
                throw new RequestException("Refresh token invalido");

            user.RefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiration = _tokenService.GetTimeToRefreshExpires();

            _uof.GenericRepository.Update<User>(user);
            await _uof.Commit();

            _logger.LogInformation("New refresh token generated for user: {user.Id}", user.Id);

            var claims = _tokenService.GetTokenClaims();

            var accessToken = _tokenService.GenerateAccessToken(claims, user.Id);

            return new LoginResponse()
            {
                AccessToken = accessToken,
                RefreshToken = user.RefreshToken,
                RefreshTokenExpiration = (DateTime)user.RefreshTokenExpiration
            };
        }
    }
}
