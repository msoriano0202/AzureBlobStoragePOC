using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureBlobStorage.POC.Api.Filters;
using AzureBlobStorage.POC.Dto.Request;
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
    public class AzureBlobContainerController : Controller
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureBlobContainerController> _logger;

        public AzureBlobContainerController(
            IConfiguration configuration,
            ILogger<AzureBlobContainerController> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _blobServiceClient = new BlobServiceClient(_configuration["StorageConnection"]);
        }

        [HttpGet]
        public async IAsyncEnumerable<BlobContainerResponse> GetAsync()
        {
            _logger.LogInformation("[API] AzureBlobContainerController: GetAsync()");

            await foreach (BlobContainerItem container in _blobServiceClient.GetBlobContainersAsync())
            {
                yield return new BlobContainerResponse
                {
                    Name = container.Name,
                    PublicAccess = container.Properties.PublicAccess?.ToString()
                };
            }
        }

        //[HttpPost]
        //public async Task PostAsync(AddBlobContainerRequest request)
        //{
        //    await _blobServiceClient.CreateBlobContainerAsync(request.Name, PublicAccessType.Blob);
        //}

        [HttpPost("{name}")]
        public async Task<IActionResult> PostAsync(string name)
        {
            _logger.LogInformation($"[API] AzureBlobContainerController: PostAsync({name})");

            try
            {
                await _blobServiceClient.CreateBlobContainerAsync(name, PublicAccessType.Blob);
            }
            catch(Exception ex)
            {
                _logger.LogError($"[API-ERROR] AzureBlobContainerController: PostAsync({name}) / {ex.Message}");
                return BadRequest(ex.Message);
            }

            return Ok("Container Created");
        }

        [HttpDelete("{name}")]
        public async Task<IActionResult> DeleteAsync(string name)
        {
            _logger.LogInformation($"[API] AzureBlobContainerController: DeleteAsync({name})");

            try
            {    
                await _blobServiceClient.DeleteBlobContainerAsync(name);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API-ERROR] - AzureBlobContainerController: DeleteAsync({name}) / {ex.Message}");
                return BadRequest(ex.Message);
            }

            return Ok("Container Deleted");
        }
    }
}
