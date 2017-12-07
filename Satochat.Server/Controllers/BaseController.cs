using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Satochat.Server.Controllers {
    [Authorize]
    public class BaseController : Controller {
        protected string getUserUuid() {
            Claim nameClaim = HttpContext.User.FindFirst(ClaimTypes.Name);
            if (nameClaim == null) {
                return null;
            }

            string userUuid = nameClaim.Value;
            if (String.IsNullOrEmpty(userUuid)) {
                return null;
            }

            return userUuid;
        }
    }
}
