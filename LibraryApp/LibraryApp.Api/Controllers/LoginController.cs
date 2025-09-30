using LibraryApp.Application.Requests;
using LibraryApp.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private readonly ILoginService _loginService;

        public LoginController(ILogger<LoginController> logger, ILoginService loginService)
        {
            _logger = logger;
            _loginService = loginService;
        }

        [HttpPost]
        public async Task<IActionResult> LoginByApplication([FromBody]LoginRequest request)
        {
            var result = await _loginService.DoLoginByApplication(request);

            return Ok(result);
        }
    }
}
