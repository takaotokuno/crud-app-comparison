using AspNetNextApp.Api.Services.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace AspNetNextApp.Api.Controllers.Shared
{
    internal static class AccountControllerResults
    {
        public static ActionResult<T> ToActionResult<T>(ControllerBase controller, AccountResult<T> result)
        {
            return result.IsSuccess ? controller.Ok(result.Value) : ToErrorActionResult(controller, result);
        }

        public static IActionResult ToActionResult<T>(ControllerBase controller, AccountResult<T> result, IActionResult successResult)
        {
            return result.IsSuccess ? successResult : ToErrorActionResult(controller, result);
        }

        private static ObjectResult ToErrorActionResult<T>(ControllerBase controller, AccountResult<T> result)
        {
            object error = new { message = result.Error };
            return result.ErrorType switch
            {
                AccountErrorType.NotFound => controller.NotFound(error),
                AccountErrorType.Conflict => controller.Conflict(error),
                AccountErrorType.Unauthorized => controller.Unauthorized(error),
                _ => controller.BadRequest(error),
            };
        }
    }
}
