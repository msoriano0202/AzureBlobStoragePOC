using System;

namespace AzureBlobStorage.POC.Dto.Response
{
    public class BlobItemResponse
    {
        public string Name { get; set; }
        public string AbosluteUrl { get; set; }
        public string AccessTier { get; set; }
        public string BlobType { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedOnToString { get; set; }
    }
}
