using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Satochat.Server.Models;
using Satochat.Server.Helper;
using Satochat.Server.Services;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Satochat.Server.ViewModels;

namespace Satochat.Server.Controllers {
    [Route("token")]
    public class AccessTokenController : BaseController {
        private IAuthenticationService _authenticationService;
        
        public AccessTokenController(IAuthenticationService authenticationService) {
            _authenticationService = authenticationService;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]AccessTokenViewModelAspnet.GetToken model) {
            IPAddress remoteIpAddress = HttpContextHelper.GetRemoteIpAddress(HttpContext);
            if (remoteIpAddress == null) {
                // TODO: log as error; we must get the remote IP
                return StatusCode(500);
            }

            User user = await _authenticationService.AuthenticateUserAsync(model.Username, model.Password);
            if (user == null) {
                return Unauthorized();
            }

            AccessToken accessToken = await _authenticationService.GetTokenAsync(user, remoteIpAddress);
            return Ok(new AccessTokenViewModelAspnet.GetTokenResult(accessToken.Token, accessToken.Expiry));
        }
    }
}
