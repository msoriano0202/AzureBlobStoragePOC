using System.Collections.Generic;

namespace AzureBlobStorage.POC.Dto.Response
{
    public class BlobPricingResponse
    {
        public BlobPricingResponse()
        {
            Items = new List<BlobPricingItem>();
        }

        public List<BlobPricingItem> Items { get; set; }
    }

    public class BlobPricingItem
    {
        public string TierName { get; set; }
        public int NumberOfItems { get; set; }
        public double TotalSize { get; set; }
        public double Price { get; set; }
    }
}
