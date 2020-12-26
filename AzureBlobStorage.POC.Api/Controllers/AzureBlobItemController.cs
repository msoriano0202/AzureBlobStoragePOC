using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureBlobStorage.POC.Api.Filters;
using AzureBlobStorage.POC.Dto.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AzureBlobStorage.POC.Api.Controllers
{
    [ApiKeyAuth]
    [ApiController]
    [Route("api/containers/{containerName}/[controller]")]
    public class AzureBlobItemController : Controller
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureBlobItemController> _logger;

        public AzureBlobItemController(
            IConfiguration configuration,
            ILogger<AzureBlobItemController> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _blobServiceClient = new BlobServiceClient(_configuration["StorageConnection"]);
        }

        private string GetDateTimeToString(DateTimeOffset? dateTimeOffset)
        {
            if (dateTimeOffset.HasValue)
            {
                return dateTimeOffset.Value.DateTime.ToString("yyyy-MM-dd");
            }

            return string.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(string containerName)
        {
            _logger.LogInformation($"[API] AzureBlobItemController: GetAsync({containerName})");

            var response = new List<BlobItemResponse>();

            try
            {
                var container = _blobServiceClient.GetBlobContainerClient(containerName);
                var uri = container.Uri.AbsoluteUri;

                await foreach (BlobItem blob in container.GetBlobsAsync())
                {
                    response.Add(new BlobItemResponse
                    {
                        Name = blob.Name,
                        AbosluteUrl = $"{uri}/{blob.Name}",
                        AccessTier = blob.Properties.AccessTier?.ToString(),
                        BlobType = blob.Properties.BlobType?.ToString(),
                        CreatedOn = blob.Properties.CreatedOn.Value.DateTime,
                        CreatedOnToString = blob.Properties.CreatedOn.Value.DateTime.ToString("yyyy-MM-dd"),
                        LastAccessedOnToString = GetDateTimeToString(blob.Properties.LastAccessedOn),
                        LastModifiedOnToString = GetDateTimeToString(blob.Properties.LastModified),
                        AccessTierChangedOnToString = GetDateTimeToString(blob.Properties.AccessTierChangedOn)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API-ERROR] AzureBlobItemController: GetAsync({containerName}) / {ex.Message}");
                return BadRequest(ex.Message);
            }

            return Ok(response);
        }

        [HttpGet("{fileName}")]
        public async Task<FileResult> GetAsync(string containerName, string fileName)
        {
            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            var blob = container.GetBlobClient(fileName);
            Response<BlobDownloadInfo> downloadInfo = await blob.DownloadAsync();

            return File(downloadInfo.Value.Content, downloadInfo.Value.ContentType, fileName);
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> PostAsync(string containerName, IFormFile file)
        {
            _logger.LogInformation($"[API] AzureBlobItemController: PostAsync({containerName}, {file.Name})");

            try
            {
                BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(containerName);
                await container.UploadBlobAsync(file.FileName, file.OpenReadStream());
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API-ERROR] - AzureBlobItemController: PostAsync({containerName}, {file.Name}) / {ex.Message}");
                return BadRequest(ex.Message);
            }

            return Ok("Blob Created");
        }

        [HttpDelete("{fileName}")]
        public async Task<IActionResult> DeleteAsync(string containerName, string fileName)
        {
            _logger.LogInformation($"[API] AzureBlobItemController: DeleteAsync({containerName}, {fileName})");

            try 
            {
                BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(containerName);
                BlobClient blob = container.GetBlobClient(fileName);
                await blob.DeleteAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError($"[API-ERROR] - AzureBlobItemController: DeleteAsync({containerName}, {fileName}) / {ex.Message}");
                return BadRequest(ex.Message);
            }

            return Ok("Blob Deleted");
        }
    }
}
