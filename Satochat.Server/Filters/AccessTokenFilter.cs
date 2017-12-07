using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Satochat.Server.Helper;
using Satochat.Server.Models;
using Satochat.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Satochat.Server.Filters {
    public class AccessTokenFilter : IAsyncAuthorizationFilter {
        private readonly IAuthenticationService _authenticationService;

        public AccessTokenFilter(IAuthenticationService authenticationService) {
            _authenticationService = authenticationService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context) {
            var httpContext = context.HttpContext;

            string tokenString = getAccessTokenStringFromHttpContext(httpContext);
            if (tokenString == null) {
                if (!actionAllowsAnonymousAccess(context.ActionDescriptor as ControllerActionDescriptor)) {
                    context.Result = new UnauthorizedResult();
                }
                return;
            }

            AccessToken accessToken = await _authenticationService.GetTokenAsync(tokenString);;
            IPAddress remoteIpAddress = HttpContextHelper.GetRemoteIpAddress(httpContext);

            if (!_authenticationService.VerifyToken(accessToken, remoteIpAddress)) {
                context.Result = new UnauthorizedResult();
                return;
            }

            User user = accessToken.User;
            context.HttpContext.User = new GenericPrincipal(new GenericIdentity(user.Uuid), new string[0]);
        }

        private string getAccessTokenStringFromHttpContext(HttpContext httpContext) {
            StringValues authorizationHeader = httpContext.Request.Headers["Authorization"];
            if (authorizationHeader == StringValues.Empty) {
                return null;
            }

            string tokenHeaderValue = authorizationHeader.FirstOrDefault();
            if (String.IsNullOrEmpty(tokenHeaderValue)) {
                return null;
            }

            string[] authorizationParts = tokenHeaderValue.Split(" ", 2);
            if (authorizationParts.Length != 2 || authorizationParts[0] != "Bearer" || String.IsNullOrEmpty(authorizationParts[1])) {
                return null;
            }

            string token = authorizationParts[1];
            return token;
        }

        private bool actionAllowsAnonymousAccess(ControllerActionDescriptor actionDescriptor) {
            // Check if the controller action allows anonymous access
            
            if (actionDescriptor == null) {
                // Should not happen
                return false;
            }

            var attribute = actionDescriptor.MethodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).FirstOrDefault();
            if (attribute == null) {
                // Anonymous access not allowed
                return false;
            }

            // Allow this request
            return true;
        }
    }
}
