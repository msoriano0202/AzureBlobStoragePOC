using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AzureBlobStorage.POC.Web.Models
{
    public class AddBlobItemViewModel
    {
        [Required]
        public string ContainerName { get; set; }

        [Required]
        public IFormFile NewFile { get; set; }

    }
}
