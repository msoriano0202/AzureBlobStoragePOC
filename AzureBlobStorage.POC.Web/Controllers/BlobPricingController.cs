using AzureBlobStorage.POC.Web.DataAccess;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBlobStorage.POC.Web.Controllers
{
    public class BlobPricingController : Controller
    {
        private readonly IDABlobItem _daBlobItem;

        public BlobPricingController(IDABlobItem daBlobItem)
        {
            _daBlobItem = daBlobItem;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _daBlobItem.GetBlobPricingsAsync();

            return View(data);
        }
    }
}
