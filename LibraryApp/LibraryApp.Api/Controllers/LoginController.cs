using LibraryApp.Application.Requests;
using LibraryApp.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        [HttpGet("google")]
        public IActionResult LoginWithGoogle()
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback))
            };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var authenticate = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (IsNotAuthenticated(authenticate))
                return Unauthorized();

            var userName = authenticate.Principal.FindFirstValue(ClaimTypes.Name);
            var email = authenticate.Principal.FindFirstValue(ClaimTypes.Email);
            var pictureUrl = authenticate.Principal.FindFirstValue("picture");

            var result = await _loginService.HandleGoogleCallback(userName, email, pictureUrl);

            return Ok(result);
        }


        protected static bool IsNotAuthenticated(AuthenticateResult result)
        {
            return result.Succeeded.Equals(false)
                || result.Principal is null
                || result.Principal.Identities.Any(d => d.IsAuthenticated) == false;
        }
    }
}
