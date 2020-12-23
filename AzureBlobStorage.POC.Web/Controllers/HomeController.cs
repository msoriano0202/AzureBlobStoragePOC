using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace AzureBlobStorage.POC.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "BlobContainers");
        }      

        public IActionResult Exception()
        {
            throw new Exception();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
