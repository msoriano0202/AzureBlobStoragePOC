using AzureBlobStorage.POC.Dto.Request;
using AzureBlobStorage.POC.Dto.Response;
using AzureBlobStorage.POC.Web.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AzureBlobStorage.POC.Web.DataAccess
{
    public interface IDABlobContainer
    {
        Task<List<BlobContainerResponse>> GetContainersAsync();
        //Task AddContainerAsync(AddBlobContainerRequest request);
        Task<ApiResponseModel<object>> AddContainerAsync(string containerName);
        Task<ApiResponseModel<object>> DeleteContainerAsync(string containerName);
    }

    public class DABlobContainer : IDABlobContainer
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        
        public DABlobContainer(
            IConfiguration configuration,
            IHttpClientFactory factory
            )
        {
            _configuration = configuration;

            _httpClient = factory.CreateClient("httpClient");
            _httpClient.DefaultRequestHeaders.Add("ApiKey", _configuration["ApiKey"]);
        }

        public async Task<List<BlobContainerResponse>> GetContainersAsync()
        {
            var url = "AzureBlobContainer";

            var response = await _httpClient.GetAsync(url);
            var answer = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<BlobContainerResponse>>(answer).OrderBy(x => x.Name).ToList();
        }

        //public async Task AddContainerAsync(AddBlobContainerRequest request)
        //{
        //    var url = $"AzureBlobContainer";

        //    var req = JsonConvert.SerializeObject(request);
        //    var content = new StringContent(req, Encoding.UTF8, "application/json");

        //    await _httpClient.PostAsync(url, content);
        //}

        public async Task<ApiResponseModel<object>> AddContainerAsync(string containerName)
        {
            var url = $"AzureBlobContainer/{containerName}";
            var response = await _httpClient.PostAsync(url, null);
            var message = (await response.Content.ReadAsStringAsync()).Split("\n").FirstOrDefault();

            return new ApiResponseModel<object>
            { 
                StatusCode = response.StatusCode,
                Message = message
            };
        }

        public async Task<ApiResponseModel<object>> DeleteContainerAsync(string containerName)
        {
            var url = $"AzureBlobContainer/{containerName}";
            var response = await _httpClient.DeleteAsync(url);
            var message = (await response.Content.ReadAsStringAsync()).Split("\n").FirstOrDefault();

            return new ApiResponseModel<object>
            {
                StatusCode = response.StatusCode,
                Message = message
            };
        }
    }
}
