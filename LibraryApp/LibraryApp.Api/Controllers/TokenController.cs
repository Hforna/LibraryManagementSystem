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

        [HttpPost("refresh-token")]
        public async Task<IActionResult> GenerateRefreshToken([FromBody]RefreshTokenRequest request)
        {
            var result = await _tokenAppService.RefreshToken(request);

            return Ok(result);
        }
    }
}
