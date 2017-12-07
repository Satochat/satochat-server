using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Satochat.Server.Helper
{
    public static class HttpContextHelper
    {
        public static IPAddress GetRemoteIpAddress(HttpContext httpContext) {
            // TODO: support reverse proxy
            return httpContext.Connection.RemoteIpAddress;
        }
    }
}
