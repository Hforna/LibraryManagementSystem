using LibraryApp.Api.Extensions;
using LibraryApp.Application.Requests;
using LibraryApp.Application.Responses;
using LibraryApp.Application.Services;
using LibraryApp.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Security.Authentication;

namespace LibraryApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IUserService _userService;

        public UsersController(ILogger<UsersController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        /// <summary>
        /// Cria um usuario com e-mail unico e manda verificação de email para o definido na requisição
        /// </summary>
        /// <param name="request">Nome de usuario com no minimo 4 digitos. Email deve ser unico e senha deve ter 8 digitos ou mais</param>
        /// <returns>retorna informações basicas sobre o usuario criado</returns>
        [HttpPost]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestException), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser(CreateUserRequest request)
        {
            var requestUri = HttpContext.GetBaseUri();
            var result = await _userService.CreateAccount(request, requestUri);

            return Created(string.Empty, result);
        }

        /// <summary>
        /// confirma a conta do usuario com o token recebido pelo e-mail
        /// </summary>
        /// <param name="email">email do usuario que esta confirmando</param>
        /// <param name="token">token fornecido no e-mail recebido</param>
        [HttpGet("confirm/email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestException), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UnauthorizedException), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ConfirmUserEmailToken([FromQuery]string email, [FromQuery]string token)
        {
            await _userService.ConfirmEmail(email, token);

            return Ok();
        }
    }
}
