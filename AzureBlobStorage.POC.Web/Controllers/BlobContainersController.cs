using AzureBlobStorage.POC.Dto.Request;
using AzureBlobStorage.POC.Web.DataAccess;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBlobStorage.POC.Web.Controllers
{
    public class BlobContainersController : Controller
    {
        private readonly IDABlobContainer _daBlobContainer;

        public BlobContainersController(IDABlobContainer daBlobContainer)
        {
            _daBlobContainer = daBlobContainer;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _daBlobContainer.GetContainersAsync();
            return View(data);
        }

        public async Task<IActionResult> AddBlobContainer(string containerName)
        {
            //await _daBlobContainer.AddContainerAsync(new AddBlobContainerRequest { Name = containerName });
            var apiResponse = await _daBlobContainer.AddContainerAsync(containerName);
            ViewBag.Message = apiResponse.Message;

            var data = await _daBlobContainer.GetContainersAsync();
            return View(nameof(Index), data);
        }

        public async Task<IActionResult> DeleteBlobContainer(string containerName)
        {
            var apiResponse = await _daBlobContainer.DeleteContainerAsync(containerName);
            ViewBag.Message = apiResponse.Message;

            var data = await _daBlobContainer.GetContainersAsync();
            return View(nameof(Index), data);
        }
    }
}
