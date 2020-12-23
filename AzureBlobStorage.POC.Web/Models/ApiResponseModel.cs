using System.Net;

namespace AzureBlobStorage.POC.Web.Models
{
    public class ApiResponseModel<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public T Result { get; set; }
    }
}
