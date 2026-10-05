using LibraryApp.Api.Extensions;
using LibraryApp.Api.Filters;
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
        private readonly IConfiguration _configuration;

        public UsersController(ILogger<UsersController> logger, IUserService userService, IConfiguration configuration)
        {
            _logger = logger;
            _userService = userService;
            _configuration = configuration;
        }

        // Não existe auto-cadastro: contas são criadas apenas pelo administrador (POST api/admin/users)

        /// <summary>
        /// Altera a senha do usuário autenticado. É preciso informar a senha atual.
        /// </summary>
        /// <param name="request">Senha atual e nova senha (8 ou mais digitos)</param>
        /// <response code="204">Senha alterada.</response>
        /// <response code="400">Se a senha atual estiver incorreta ou a nova senha for inválida.</response>
        [HttpPut("me/password")]
        [UserAuthenticated]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(RequestException), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            await _userService.ChangePassword(request);

            return NoContent();
        }

        /// <summary>
        /// Envia para o e-mail informado um link de redefinição de senha, válido por 1 hora.
        /// A resposta é sempre a mesma, exista ou não uma conta com esse e-mail.
        /// </summary>
        /// <param name="request">E-mail da conta</param>
        /// <response code="200">Pedido recebido.</response>
        [HttpPost("password/forgot")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            // O link do e-mail usa a URL pública configurada, e não o cabeçalho Host da requisição (que pode ser forjado)
            var publicUrl = _configuration.GetValue<string>("App:PublicUrl");
            var baseUri = string.IsNullOrWhiteSpace(publicUrl) ? HttpContext.GetBaseUri() : publicUrl.TrimEnd('/') + "/";

            await _userService.ForgotPassword(request.Email, baseUri);

            return Ok();
        }

        /// <summary>
        /// Redefine a senha usando o token recebido no link do e-mail. O token só pode ser usado uma vez.
        /// </summary>
        /// <param name="request">E-mail, token do link e nova senha (8 ou mais digitos)</param>
        /// <response code="204">Senha redefinida.</response>
        /// <response code="400">Se o link for inválido ou estiver expirado.</response>
        [HttpPost("password/reset")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(RequestException), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            await _userService.ResetPassword(request);

            return NoContent();
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
