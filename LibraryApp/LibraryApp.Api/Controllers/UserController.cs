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
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;

        public UserController(ILogger<UserController> logger, IUserService userService)
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
    }
}
