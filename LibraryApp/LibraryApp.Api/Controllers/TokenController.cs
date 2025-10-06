using LibraryApp.Application.Requests;
using LibraryApp.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ITokenAppService _tokenAppService;

        public TokenController(ITokenAppService tokenAppService)
        {
            _tokenAppService = tokenAppService;
        }

        /// <summary>
        /// Gera um novo access token e refresh token para autenticação
        /// </summary>
        /// <param name="request">refresh token fornecido no momento da autenticação do usuario</param>
        /// <returns>retorna um access token e um refresh token com a data de expiração</returns>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> GenerateRefreshToken([FromBody]RefreshTokenRequest request)
        {
            var result = await _tokenAppService.RefreshToken(request);

            return Ok(result);
        }
    }
}
