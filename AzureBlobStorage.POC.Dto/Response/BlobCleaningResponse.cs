using System.Collections.Generic;

namespace AzureBlobStorage.POC.Dto.Response
{
    public class BlobCleaningResponse
    {
        public BlobCleaningResponse()
        {
            ItemsMoved = new List<string>();
        }

        public List<string> ItemsMoved { get; set; }
        public double OperationCost { get; set; }
    }
}
