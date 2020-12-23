using AzureBlobStorage.POC.Dto.Response;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AzureBlobStorage.POC.Web.Models
{
    public class BlobContainerViewModel
    {
        [Required]
        public string ContainerName { get; set; }

        [Required]
        public IFormFile NewFile { get; set; }

        public List<BlobItemResponse> BlobItems { get; set; }
    }
}
