using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureBlobStorage.POC.Api.Filters;
using AzureBlobStorage.POC.Dto.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureBlobStorage.POC.Api.Controllers
{
    [ApiKeyAuth]
    [ApiController]
    [Route("api/[controller]")]
    public class AzureBlobCleaningController : Controller
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureBlobCleaningController> _logger;

        public AzureBlobCleaningController(
            IConfiguration configuration,
            ILogger<AzureBlobCleaningController> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _blobServiceClient = new BlobServiceClient(_configuration["StorageConnection"]);
        }

        [HttpPost("{tierName}/{days}")]
        public async Task<IActionResult> PostAsync(string tierName, int days, [FromQuery] string containerName)
        {
            _logger.LogInformation($"[API] AzureBlobCleaningController: PostAsync({tierName}, {days}, {containerName})");

            var response = new BlobCleaningResponse();

            try
            {
                if (!string.IsNullOrEmpty(containerName))
                {
                    response = await MoveToTier(tierName, days, containerName);
                }
                else
                {
                    await foreach (BlobContainerItem container in _blobServiceClient.GetBlobContainersAsync())
                    {
                        var partialResponse = await MoveToTier(tierName, days, container.Name);

                        response.ItemsMoved.AddRange(partialResponse.ItemsMoved);
                        response.OperationCost += partialResponse.OperationCost;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API-ERROR] AzureBlobCleaningController: PostAsync({tierName}, {days}, {containerName}) / {ex.Message}");
                return BadRequest(ex.Message);
            }

            return Ok(response);
        }

        private async Task<BlobCleaningResponse> MoveToTier(string tierName, int days, string containerName)
        {
            var response = new BlobCleaningResponse();

            if (tierName.Equals(AccessTier.Cool.ToString(), StringComparison.OrdinalIgnoreCase))
                response = await MoveToCool(days, containerName);
            else if (tierName.Equals(AccessTier.Archive.ToString(), StringComparison.OrdinalIgnoreCase))
                response = await MoveToArchive(days, containerName);
            else
                throw new Exception("Tier Not Found");

            return response;
        }

        private async Task<BlobCleaningResponse> MoveToCool(int days, string containerName)
        {
            var today = DateTime.Now;
            var itemsMoved = new List<string>();
            var itemsCounter = 0;
            var priceWriteToCoolTier = Convert.ToDouble(_configuration["AzureWriteBlobPricing:ToCoolTier"]);
            double priceReadBlob = 0;

            var container = _blobServiceClient.GetBlobContainerClient(containerName);

            await foreach (BlobItem blob in container.GetBlobsAsync())
            {
                priceReadBlob += GetBlobReadingPrice(blob);

                if (
                    blob.Properties.AccessTier == AccessTier.Hot && 
                    ((today - blob.Properties.CreatedOn.Value.Date).TotalDays >= days)
                   )
                {
                    BlobClient blobClient = container.GetBlobClient(blob.Name);

                    try 
                    {
                        await blobClient.SetAccessTierAsync(AccessTier.Cool);
                        itemsMoved.Add($"/{containerName}/{blob.Name}");
                        itemsCounter++;
                    }
                    catch (Exception e)
                    {
                        itemsMoved.Add($"ERROR: /{containerName}/{blob.Name} --> {e.Message}");
                    }
                }
            }

            return new BlobCleaningResponse 
            {
                ItemsMoved = itemsMoved,
                OperationCost = Math.Round((itemsCounter * priceWriteToCoolTier) + priceReadBlob, 5)
            };
        }

        private async Task<BlobCleaningResponse> MoveToArchive(int days, string containerName)
        {
            var today = DateTime.Now;
            var itemsMoved = new List<string>();
            var itemsCounter = 0;
            var priceToArchiveTier = Convert.ToDouble(_configuration["AzureWriteBlobPricing:ToArchiveTier"]);
            double priceReadBlob = 0;

            var container = _blobServiceClient.GetBlobContainerClient(containerName);

            await foreach (BlobItem blob in container.GetBlobsAsync())
            {
                priceReadBlob += GetBlobReadingPrice(blob);

                if (
                    blob.Properties.AccessTier == AccessTier.Cool &&
                    ((today - blob.Properties.CreatedOn.Value.Date).TotalDays >= days)
                   )
                {
                    BlobClient blobClient = container.GetBlobClient(blob.Name);

                    try 
                    {
                        await blobClient.SetAccessTierAsync(AccessTier.Archive);
                        itemsMoved.Add($"/{containerName}/{blob.Name}");
                        itemsCounter++;
                    }
                    catch(Exception e) 
                    {
                        itemsMoved.Add($"ERROR: /{containerName}/{blob.Name} --> {e.Message}");
                    }
                }
            }

            return new BlobCleaningResponse
            {
                ItemsMoved = itemsMoved,
                OperationCost = Math.Round((itemsCounter * priceToArchiveTier) + priceReadBlob, 5)
            };
        }

        private double GetBlobReadingPrice(BlobItem blob)
        {
            double price = 0;
          
            if (blob.Properties.AccessTier == AccessTier.Hot)
                price = Convert.ToDouble(_configuration["AzureReadBlobPricing:FromHotTier"]);
            else if (blob.Properties.AccessTier == AccessTier.Cool)
                price = Convert.ToDouble(_configuration["AzureReadBlobPricing:FromCoolTier"]);
            else if (blob.Properties.AccessTier == AccessTier.Archive)
                price = Convert.ToDouble(_configuration["AzureReadBlobPricing:FromArchiveTier"]);

            return price;
        }
    }
}
