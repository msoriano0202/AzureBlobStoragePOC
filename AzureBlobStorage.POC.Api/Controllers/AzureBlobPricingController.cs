using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureBlobStorage.POC.Api.Filters;
using AzureBlobStorage.POC.Api.Helpers;
using AzureBlobStorage.POC.Dto.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AzureBlobStorage.POC.Api.Controllers
{
    [ApiKeyAuth]
    [ApiController]
    [Route("api/[controller]")]
    public class AzureBlobPricingController : Controller
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;
        private readonly IStoragePricingHelper _storagePricingHelper;
        private readonly ILogger<AzureBlobPricingController> _logger;

        private double gigaInBytes = Math.Pow(1024, 3);

        public AzureBlobPricingController(
            IConfiguration configuration,
            IStoragePricingHelper storagePricingHelper,
            ILogger<AzureBlobPricingController> logger)
        {
            _configuration = configuration;
            _storagePricingHelper = storagePricingHelper;
            _logger = logger;

            _blobServiceClient = new BlobServiceClient(_configuration["StorageConnection"]);
        }

        [HttpGet()]
        public async Task<BlobPricingResponse> GetAsync()
        {
            _logger.LogInformation($"[API] AzureBlobPricingController: GetAsync()");

            var response = new BlobPricingResponse();

            var hotCounter = 0;
            var coolCounter = 0;
            var archiveCounter = 0;

            long hotSize = 0;
            long coolSize = 0;
            long archiveSize = 0;

            await foreach (BlobContainerItem container in _blobServiceClient.GetBlobContainersAsync())
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(container.Name);

                await foreach (BlobItem blob in containerClient.GetBlobsAsync())
                {
                    if (blob.Properties.AccessTier == AccessTier.Hot)
                    {
                        hotCounter++;
                        hotSize += blob.Properties.ContentLength.Value;
                    }
                    else if (blob.Properties.AccessTier == AccessTier.Cool)
                    {
                        coolCounter++;
                        coolSize += blob.Properties.ContentLength.Value;
                    }
                    else if (blob.Properties.AccessTier == AccessTier.Archive)
                    {
                        archiveCounter++;
                        archiveSize += blob.Properties.ContentLength.Value;
                    }
                }
            }

            response.Items.Add(new BlobPricingItem 
            { TierName = AccessTier.Hot.ToString(), NumberOfItems = hotCounter, TotalSize = Math.Round(hotSize/ gigaInBytes,5), Price = _storagePricingHelper.GetHotStoragePricing(hotSize) });
            
            response.Items.Add(new BlobPricingItem 
            { TierName = AccessTier.Cool.ToString(), NumberOfItems = coolCounter, TotalSize = Math.Round(coolSize/ gigaInBytes,5), Price = _storagePricingHelper.GetCoolStoragePricing(hotSize) });
            
            response.Items.Add(new BlobPricingItem 
            { TierName = AccessTier.Archive.ToString(), NumberOfItems = archiveCounter, TotalSize = Math.Round(archiveSize/ gigaInBytes,5), Price = _storagePricingHelper.GetArchiveStoragePricing(hotSize) });

            return response;
        }
    }
}
