using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Satochat.Server.Services;

namespace Satochat.Server.Filters {
    public class ServiceExceptionHandlingFilter : IActionFilter {
        public void OnActionExecuted(ActionExecutedContext context) {
            if (context.Exception is ServiceException ex) {
                int statusCode = 500;
                string msg = ex.Message;

                switch (ex.ErrorCode) {
                    case ServiceErrorCode.NotFound:
                        statusCode = 404;
                        break;
                    case ServiceErrorCode.Unauthorized:
                        statusCode = 403;
                        break;
                    default:
                        msg = null;
                        break;
                }

                context.Result = new StatusCodeResult(statusCode);
                context.ExceptionHandled = true;
            }
        }

        public void OnActionExecuting(ActionExecutingContext context) {
        }
    }
}
