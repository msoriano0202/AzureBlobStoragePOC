using AzureBlobStorage.POC.Web.DataAccess;
using AzureBlobStorage.POC.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AzureBlobStorage.POC.Web.Controllers
{
    public class BlobItemsController : Controller
    {
        private readonly IDABlobItem _daBlobItem;

        public BlobItemsController(IDABlobItem daBlobItem)
        {
            _daBlobItem = daBlobItem;
        }

        public async Task<IActionResult> Index(string containerName)
        {
            var apiResponse = await _daBlobItem.GetBlobItemsAsync(containerName);

            var model = new BlobContainerViewModel 
            { 
                ContainerName = containerName,
                BlobItems = apiResponse.Result
            };

            if (apiResponse.StatusCode != HttpStatusCode.OK)
                ViewBag.Message = apiResponse.Message;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddBlobItem(AddBlobItemViewModel model)
        {
            if (ModelState.IsValid && model.NewFile != null)
            {
                var apiResponse = await _daBlobItem.AddBlobItemAsync(model.ContainerName, model.NewFile);

                if (apiResponse.StatusCode != HttpStatusCode.OK)
                {
                    ViewBag.Message = apiResponse.Message;
                    return View("Index", new BlobContainerViewModel { ContainerName = model.ContainerName } );
                }
            }

            return RedirectToAction(nameof(Index), new { containerName = model.ContainerName });
        }

        public async Task<IActionResult> DeleteBlobItem(string containerName, string fileName) 
        {
            var apiResponse = await _daBlobItem.DeleteBlobItemAsync(containerName, fileName);

            if (apiResponse.StatusCode != HttpStatusCode.OK)
            {
                ViewBag.Message = apiResponse.Message;
                return View("Index", new BlobContainerViewModel { ContainerName = containerName });
            }

            return RedirectToAction(nameof(Index), new { containerName = containerName });
        }
    }
}
