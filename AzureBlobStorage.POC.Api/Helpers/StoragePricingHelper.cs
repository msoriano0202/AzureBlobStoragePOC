using Microsoft.Extensions.Configuration;
using System;

namespace AzureBlobStorage.POC.Api.Helpers
{
    public interface IStoragePricingHelper
    {
        double GetHotStoragePricing(long totalSize);
        double GetCoolStoragePricing(long totalSize);
        double GetArchiveStoragePricing(long totalSize);
    }
    public class StoragePricingHelper : IStoragePricingHelper
    {
        private readonly IConfiguration _configuration;
        private double gigaInBytes = Math.Pow(1024, 3);
        private double teraInBytes = Math.Pow(1024, 4);

        public StoragePricingHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public double GetHotStoragePricing(long totalSize) 
        {
            double total = 0;
            var hotFirst50TB = Convert.ToDouble(_configuration["AzureBlobStoragePricing:Hot:First50TB"]);
            var hotNext450TB = Convert.ToDouble(_configuration["AzureBlobStoragePricing:Hot:Next450TB"]);
            var hotMoreThan500TB = Convert.ToDouble(_configuration["AzureBlobStoragePricing:Hot:MoreThan500TB"]);

            double amount = (totalSize / gigaInBytes);

            if (totalSize <= 50 * teraInBytes)
                total = amount * hotFirst50TB;
            else if (totalSize <= 500 * teraInBytes)
                total = amount * hotNext450TB;
            else
                total = amount * hotMoreThan500TB;

            return Math.Round(total, 5);
        }

        public double GetCoolStoragePricing(long totalSize)
        {
            double total = 0;
            var coolFirst50TB = Convert.ToDouble(_configuration["AzureBlobStoragePricing:Cool:First50TB"]);
            var coolNext450TB = Convert.ToDouble(_configuration["AzureBlobStoragePricing:Cool:Next450TB"]);
            var coolMoreThan500TB = Convert.ToDouble(_configuration["AzureBlobStoragePricing:Cool:MoreThan500TB"]);

            double amount = (totalSize / gigaInBytes);

            if (totalSize <= 50 * teraInBytes)
                total = amount * coolFirst50TB;
            else if (totalSize <= 500 * teraInBytes)
                total = amount * coolNext450TB;
            else
                total = amount * coolMoreThan500TB;

            return Math.Round(total, 5);
        }

        public double GetArchiveStoragePricing(long totalSize)
        {
            double total = 0;
            var archiveFirst50TB = Convert.ToDouble(_configuration["AzureBlobStoragePricing:Archive:First50TB"]);
            var archiveNext450TB = Convert.ToDouble(_configuration["AzureBlobStoragePricing:Archive:Next450TB"]);
            var archiveMoreThan500TB = Convert.ToDouble(_configuration["AzureBlobStoragePricing:Archive:MoreThan500TB"]);

            double amount = (totalSize / gigaInBytes);

            if (totalSize <= 50 * teraInBytes)
                total = amount * archiveFirst50TB;
            else if (totalSize <= 500 * teraInBytes)
                total = amount * archiveNext450TB;
            else
                total = amount * archiveMoreThan500TB;

            return Math.Round(total, 5);
        }
    }
}
