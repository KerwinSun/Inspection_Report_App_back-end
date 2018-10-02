using InspectionReport.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace InspectionReport.Services.Interfaces
{
    public class ImageService : IImageService
    {
        private readonly ReportContext _context;
        private const string ContainerName = "house";

        private readonly CloudBlobClient client;

        public ImageService(ReportContext reportContext)
        {
            _context = reportContext;
            String storageConnectionString =
                "DefaultEndpointsProtocol=https;AccountName=reportpictures;AccountKey=3cxwdbIYl0MBEy0Aaa0TCuUmBZ3KHmBjT2bogu/IUTsU2VPhxPo38Vi/AKXy+tQB//VKTm0VQZ7ewUqJHZGDbQ==;EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            client = storageAccount.CreateCloudBlobClient();
        }

        public List<string> GetUriResultsForFeature(long featureId, out HttpStatusCode outStatusCode)
        {
            Feature feature = _context.Feature.Find(featureId);
            long house_id = GetHouseIdFromFeatureId(featureId);
            if (house_id == 0)
            {
                outStatusCode = HttpStatusCode.NotFound;
                return new List<string>();
            }

            var container = client.GetContainerReference(ContainerName + house_id);
            if (!container.ExistsAsync().Result)
            {
                outStatusCode = HttpStatusCode.NoContent;
                return new List<string>();
            }

            List<string> imageNames = _context.Media
                .Where(m => m.Feature == feature)
                .Select(m => m.MediaName)
                .ToList();

            List<string> uriResults = imageNames
                .Select(imgName => GetBlobSASUri(container.GetBlockBlobReference(imgName)))
                .ToList();

            outStatusCode = HttpStatusCode.OK;
            return uriResults;
        }

        /// <summary>
        /// Get HouseId corresponding to a feature.
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        private long GetHouseIdFromFeatureId(long id)
        {
            Feature feature = _context.Feature
                .Where(f => f.Id == id)
                .Include(x => x.Category)
                .SingleOrDefault();

            if (feature == null)
            {
                return 0;
            }

            long CatId = feature.Category.Id;
            long HouseId = _context.Categories.Where(x => x.Id == CatId)
                .Include(h => h.House)
                .SingleOrDefault().House.Id;
            return HouseId;
        }

        /// <summary>
        /// Method is used to retreive a unique SAS link for a particular blob.
        /// Shared Access Blob Policy dictates the time period before the link expires
        /// and Permissions can be set to enable read/ write/ list the blob.
        /// </summary>
        /// <param name="blob"></param>
        /// <returns></returns>
        private string GetBlobSASUri(CloudBlockBlob blob)
        {
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddDays(1),
                Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.List
            };
            string sasContainerToken = blob.GetSharedAccessSignature(sasConstraints);
            return blob.Uri.AbsoluteUri + sasContainerToken;
        }
    }
}
