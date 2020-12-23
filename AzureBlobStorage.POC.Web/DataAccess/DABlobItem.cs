using AzureBlobStorage.POC.Dto.Response;
using AzureBlobStorage.POC.Web.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureBlobStorage.POC.Web.DataAccess
{
    public interface IDABlobItem
    {
        Task<ApiResponseModel<List<BlobItemResponse>>> GetBlobItemsAsync(string containerName);
        Task<ApiResponseModel<object>> AddBlobItemAsync(string containerName, IFormFile file);
        Task<ApiResponseModel<object>> DeleteBlobItemAsync(string containerName, string fileName);
        Task<ApiResponseModel<BlobCleaningResponse>> MoveToTierAsync(string tierName, int days, string containerName);
        Task<BlobPricingResponse> GetBlobPricingsAsync();
    }

    public class DABlobItem : IDABlobItem
    {
        private readonly HttpClient _httpClient;

        public DABlobItem(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("httpClient");
        }

        public async Task<ApiResponseModel<List<BlobItemResponse>>> GetBlobItemsAsync(string containerName)
        {
            var result = new ApiResponseModel<List<BlobItemResponse>>();
            
            var url = $"containers/{containerName}/AzureBlobItem";
            var response = await _httpClient.GetAsync(url);
            var answer = await response.Content.ReadAsStringAsync();

            result.StatusCode = response.StatusCode;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                result.Result = JsonConvert.DeserializeObject<List<BlobItemResponse>>(answer).OrderBy(x => x.CreatedOn).ToList();
            }
            else 
            {
                result.Message = answer.Split("\n").FirstOrDefault();
            }
                     
            return result;
        }

        public async Task<ApiResponseModel<object>> AddBlobItemAsync(string containerName, IFormFile file) 
        {
            var url = $"containers/{containerName}/AzureBlobItem";

            byte[] data;
            using (var br = new BinaryReader(file.OpenReadStream()))
                data = br.ReadBytes((int)file.OpenReadStream().Length);

            ByteArrayContent bytes = new ByteArrayContent(data);

            MultipartFormDataContent multiContent = new MultipartFormDataContent();
            multiContent.Add(bytes, "file", file.FileName);

            var response = await _httpClient.PostAsync(url, multiContent);
            var message = (await response.Content.ReadAsStringAsync()).Split("\n").FirstOrDefault();

            return new ApiResponseModel<object>
            {
                StatusCode = response.StatusCode,
                Message = message
            };
        }

        public async Task<ApiResponseModel<object>> DeleteBlobItemAsync(string containerName, string fileName)
        {
            var url = $"containers/{containerName}/AzureBlobItem/{fileName}";
            var response = await _httpClient.DeleteAsync(url);
            var message = (await response.Content.ReadAsStringAsync()).Split("\n").FirstOrDefault();

            return new ApiResponseModel<object>
            {
                StatusCode = response.StatusCode,
                Message = message
            };
        }

        public async Task<ApiResponseModel<BlobCleaningResponse>> MoveToTierAsync(string tierName, int days, string containerName)
        {
            var result = new ApiResponseModel<BlobCleaningResponse>();

            var url = $"azureblobcleaning/{tierName}/{days}" + 
                (!string.IsNullOrEmpty(containerName) ? $"?containerName={containerName}" : "");

            var response = await _httpClient.PostAsync(url, null);
            var answer = await response.Content.ReadAsStringAsync();

            result.StatusCode = response.StatusCode;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                result.Result = JsonConvert.DeserializeObject<BlobCleaningResponse>(answer);
            }
            else
            {
                result.Message = answer.Split("\n").FirstOrDefault();
            }

            return result;
        }

        public async Task<BlobPricingResponse> GetBlobPricingsAsync()
        {
            var url = $"AzureBlobPricing";

            var response = await _httpClient.GetAsync(url);
            var answer = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<BlobPricingResponse>(answer);
        }
    }
}
