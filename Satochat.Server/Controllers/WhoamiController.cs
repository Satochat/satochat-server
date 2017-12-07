using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Satochat.Server.ViewModels;

namespace Satochat.Server.Controllers {
    [Route("whoami")]
    public class WhoamiController : BaseController {
        [HttpGet]
        public IActionResult Get() {
            return Ok(new WhoamiViewModelAspnet.ViewResult(getUserUuid()));
        }
    }
}
