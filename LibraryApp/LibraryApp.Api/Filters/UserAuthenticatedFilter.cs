using LibraryApp.Domain.Exceptions;
using LibraryApp.Domain.Repositories;
using LibraryApp.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LibraryApp.Api.Filters
{
    public class UserAuthenticatedAttribute : TypeFilterAttribute
    {
        public UserAuthenticatedAttribute() : base(typeof(UserAuthenticatedFilter))
        {
        }
    }

    public class UserAuthenticatedFilter : IAsyncAuthorizationFilter
    {
        private readonly IUnitOfWork _uow;
        private readonly IRequestService _requestService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<UserAuthenticatedFilter> _logger;

        public UserAuthenticatedFilter(IUnitOfWork uow, IRequestService requestService, 
            ITokenService tokenService, ILogger<UserAuthenticatedFilter> logger)
        {
            _uow = uow;
            _requestService = requestService;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var token = _requestService.GetBearerToken();

            var errorMessage = "Usuario deve estar autenticado para acessar esse recurso";

            if (string.IsNullOrEmpty(token))
                throw new RequestException(errorMessage);

            try
            {
                var userId = _tokenService.GetUserIdByToken();
                if(userId == null)
                    throw new RequestException(errorMessage);

                var user = await _uow.UserRepository.GetUserById((long)userId) ?? throw new NotFoundException(errorMessage);
            }
            catch (RequestException)
            {
                throw;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while trying to validate user token");

                throw new UnexpectedErrorException("Um erro inesperado ocorreu enquanto tentava validar o token do usuário");
            }
        }
    }
}
