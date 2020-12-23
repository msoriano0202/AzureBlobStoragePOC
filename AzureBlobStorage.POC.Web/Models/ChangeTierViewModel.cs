using AzureBlobStorage.POC.Dto.Response;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AzureBlobStorage.POC.Web.Models
{
    public class ChangeTierViewModel
    {
        public List<SelectListItem> Containers { get; set; }
        public List<SelectListItem> Tiers { get; set; }


        public string ContainerName { get; set; }

        [Required]
        public string TierName { get; set; }
        
        [Required]
        public int Days { get; set; }

        public BlobCleaningResponse Result { get; set; }
    }
}
