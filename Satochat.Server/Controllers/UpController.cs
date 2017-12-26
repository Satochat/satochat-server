using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Satochat.Server.Controllers {
    [Route("up")]
    public class UpController : Controller {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Get() {
            return Ok();
        }
    }
}
