using LibraryApp.Api.Extensions;
using LibraryApp.Application.Requests;
using LibraryApp.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

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

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserRequest request)
        {
            var requestUri = HttpContext.GetBaseUri();
            var result = await _userService.CreateAccount(request, requestUri);

            return Created(string.Empty, result);
        }

        [HttpGet("confirm/email")]
        public async Task<IActionResult> ConfirmUserEmailToken([FromQuery]string email, [FromQuery]string token)
        {
            await _userService.ConfirmEmail(email, token);

            return Ok();
        }
    }
}
