using AzureBlobStorage.POC.Web.DataAccess;
using AzureBlobStorage.POC.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AzureBlobStorage.POC.Web.Controllers
{
    public class ChangeTiersController : Controller
    {
        private readonly IDABlobContainer _daBlobContainer;
        private readonly IDABlobItem _daBlobItem;


        public ChangeTiersController(
            IDABlobContainer daBlobContainer,
            IDABlobItem daBlobItem)
        {
            _daBlobContainer = daBlobContainer;
            _daBlobItem = daBlobItem;
        }

        public async Task<IActionResult> ChangeTiers()
        {
            var model = new ChangeTierViewModel
            {
                Containers = await GetContainers(),
                Tiers = GetTiers()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeTiers(ChangeTierViewModel model)
        {
            if (ModelState.IsValid)
            {
                var apiResponse = await _daBlobItem.MoveToTierAsync(model.TierName, model.Days, model.ContainerName);

                if (apiResponse.StatusCode == HttpStatusCode.OK)
                    model.Result = apiResponse.Result;
                else
                    ViewBag.Message = apiResponse.Message;
            }
            
            model.Containers = await GetContainers();
            model.Tiers = GetTiers();

            return View(model);
        }

        private async Task<List<SelectListItem>> GetContainers()
        {
            var data = await _daBlobContainer.GetContainersAsync();

            var result = data.Select(x => new SelectListItem 
            { 
                Text = x.Name, 
                Value = x.Name 
            }).ToList();

            result.Insert(0, new SelectListItem { Text = "Select Container ->", Value = string.Empty });

            return result;
        }

        private List<SelectListItem> GetTiers()
        {
            return new List<SelectListItem> 
            {
                new SelectListItem { Text = "Select Tier ->", Value = string.Empty },
                new SelectListItem { Text = "Cool", Value = "Cool" },
                new SelectListItem { Text = "Archive", Value = "Archive" }
            };
        }
    }
}
