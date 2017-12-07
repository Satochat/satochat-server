using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Satochat.Server.Services;
using Satochat.Server.Models;
using Satochat.Server.ViewModels;

namespace Satochat.Server.Controllers {
    [Route("friend")]
    public class FriendController : BaseController {
        private readonly IUserService _userService;

        public FriendController(IUserService userService) {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Get() {
            User user = await _userService.FindUserByUuidWithFriendsAsync(getUserUuid());
            var friends = user.Friends.Select(e => new FriendViewModelAspnet.ViewResult(e.User.Uuid, e.AllowCommunication));
            return Ok(friends);
        }
    }
}
