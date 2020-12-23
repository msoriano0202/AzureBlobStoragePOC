using AzureBlobStorage.POC.Web.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AzureBlobStorage.POC.Web.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsController : ControllerBase
    {
        private readonly ILogger<ErrorsController> _logger;
        public ErrorsController(
            ILogger<ErrorsController> logger)
        {
            _logger = logger;
        }

        [Route("error")]
        public IActionResult Error()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context.Error;
            var code = 500;

            if (exception is HttpStatusException httpException)
            {
                code = (int)httpException.Status;
            }

            Response.StatusCode = code;

            var ex = new MyErrorResponse(exception);

            _logger.LogError($"[WEB-ERROR] Type: {ex.Type} / Message: {ex.Message} / StackTrace: {ex.StackTrace})");

            return RedirectToAction("Error", "Home");
        }
    }
}
