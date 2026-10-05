using LibraryApp.Domain.Constants;
using LibraryApp.Domain.Exceptions;
using LibraryApp.Domain.Repositories;
using LibraryApp.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LibraryApp.Api.Filters
{
    // Filtro que garante que somente usuarios autenticados com o papel de administrador acessem o recurso
    public class AdminAuthenticatedAttribute : TypeFilterAttribute
    {
        public AdminAuthenticatedAttribute() : base(typeof(AdminAuthenticatedFilter))
        {
        }
    }

    public class AdminAuthenticatedFilter : IAsyncAuthorizationFilter
    {
        private readonly IUnitOfWork _uow;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AdminAuthenticatedFilter> _logger;

        public AdminAuthenticatedFilter(IUnitOfWork uow, ITokenService tokenService, ILogger<AdminAuthenticatedFilter> logger)
        {
            _uow = uow;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var errorMessage = "Usuario deve estar autenticado para acessar esse recurso";

            try
            {
                // GetTokenClaims valida a assinatura e a expiração do token antes de retornar as claims
                var claims = _tokenService.GetTokenClaims();

                var userId = _tokenService.GetUserIdByToken();
                if (userId == null)
                    throw new RequestException(errorMessage);

                var user = await _uow.UserRepository.GetUserById((long)userId)
                    ?? throw new NotFoundException(errorMessage);

                var isAdmin = claims.Any(d => d.Type == System.Security.Claims.ClaimTypes.Role
                    && d.Value.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase));

                if (!isAdmin)
                    throw new ForbiddenException("Usuario não tem permissão de administrador para acessar esse recurso");
            }
            catch (RequestException)
            {
                throw;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (ForbiddenException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while trying to validate admin token");

                throw new UnexpectedErrorException("Um erro inesperado ocorreu enquanto tentava validar o token do usuário");
            }
        }
    }
}
