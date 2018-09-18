using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.IO;
using InspectionReport.Models;
using Microsoft.AspNetCore.Http;
using ImageMagick;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace InspectionReport.Controllers
{
    [Route("api/Image")]
    public class ImageController : Controller
    {
        private const string ContainerName = "house";
        private readonly ReportContext _context;

        private readonly CloudBlobClient client;

        /// <summary>
        /// The constructor initialises the Blob Strage and the context.
        /// TODO: Remove connection string, and replace with Azure Key Vault once 
        /// Authentication is complete.
        /// </summary>
        /// <param name="context"></param>
        public ImageController(ReportContext context)
        {
            _context = context;
            String storageConnectionString =
                "DefaultEndpointsProtocol=https;AccountName=reportpictures;AccountKey=3cxwdbIYl0MBEy0Aaa0TCuUmBZ3KHmBjT2bogu/IUTsU2VPhxPo38Vi/AKXy+tQB//VKTm0VQZ7ewUqJHZGDbQ==;EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            client = storageAccount.CreateCloudBlobClient();
        }

        /// <summary>
        /// The GET method returns the links for the set of images within a particular feature.
        /// TODO: make the links accessible only to authorized personnel once authentication is done. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetImage")]
        public async Task<IActionResult> GetImage(long id)
        {
            ICollection<IActionResult> fileList = new List<IActionResult>();

            Feature feature = _context.Feature.Find(id);
            long house_id = GetHouseIdFromFeatureId(id);

            var container = client.GetContainerReference(ContainerName + house_id);
            if (!await container.ExistsAsync())
            {
                return NoContent();
            }

            List<string> imageNames = new List<string>();
            _context.Media.Where(m => m.Feature == feature).ToList().ForEach(m => imageNames.Add(m.MediaName));

            List<string> UriResults = new List<string>();
            foreach (string imgName in imageNames)
            {
                CloudBlockBlob image = container.GetBlockBlobReference(imgName);
                UriResults.Add(GetBlobSASUri(image));
            }

            return Ok(UriResults);
        }

        /// <summary>
        /// HTTP Post handles the posting of a particular media (image) type to the Azure
        /// Blob Storge. The request expects one or more Images (restricted to PNG and JPEG) files
        /// files to be included, along with a header with the key of: "feature-id" and the 
        /// corresponding value pair is the id of the feature to which the image must be uploaded.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PostImage()
        {
            IFormCollection requestForm = HttpContext.Request.Form;
            IHeaderDictionary header = HttpContext.Request.Headers;
            long feature_id;

            if (header.ContainsKey("feature-id"))
            {
                feature_id = Convert.ToInt64(header["feature-id"]);
            }
            else
            {
                return BadRequest("No feature-id found in the header.");
            }

            // Get feature based on ID supplied in Header.
            Feature feature = _context.Feature.Find(feature_id);
            if (feature == null)
            {
                NotFound();
            }

            long house_id = GetHouseIdFromFeatureId(feature_id);

            // Containers on Azure are named "House" + house_id. E.g. House1 is the container
            // for House with ID = 1.
            string container_name = ContainerName + house_id;
            var container = client.GetContainerReference(container_name);
            await container.CreateIfNotExistsAsync();

            // Check for any uploaded file  
            if (requestForm.Files.Count > 0)
            {
                //Loop through uploaded files  
                for (int i = 0; i < requestForm.Files.Count; i++)
                {
                    List<string> validTypes = new List<string>()
                    {
                        "image/png",
                        "image/jpeg",
                        "image/hevc",
                        "image/heif",
                        "image/heic"
                    };

                    IFormFile postedFile = requestForm.Files[i];
                    // verify file is of correct type.
                    if (validTypes.FindIndex(x => x.Equals(postedFile.ContentType,
                            StringComparison.OrdinalIgnoreCase)) == -1)
                    {
                        Console.Write("Invalid file type");
                        continue;
                    }

                    if (postedFile != null)
                    {
                        int fileNameStartLocation = postedFile.FileName.LastIndexOf("\\") + 1;
                        string fileName = postedFile.FileName.Substring(fileNameStartLocation);

                        CloudBlockBlob blockBlobImage = container.GetBlockBlobReference(fileName);

                        blockBlobImage.Metadata.Add("DateCreated", DateTime.UtcNow.ToLongDateString());
                        blockBlobImage.Metadata.Add("TimeCreated", DateTime.UtcNow.ToLongTimeString());

                        MemoryStream memoryStream = new MemoryStream();

                        blockBlobImage.Properties.ContentType = postedFile.ContentType;

                        MagickImage image = new MagickImage(postedFile.OpenReadStream());
                        image.AutoOrient();

                        await memoryStream.WriteAsync(image.ToByteArray(), 0, image.ToByteArray().Length);

                        memoryStream.Position = 0;

                        await blockBlobImage.UploadFromStreamAsync(memoryStream);

                        Media media = new Media
                        {
                            Feature = feature,
                            MediaName = fileName
                        };

                        _context.Media.Add(media);
                        _context.SaveChanges();
                    }
                }
            }
            else
            {
                return NoContent(); // No image uploaded.
            }

            return Ok();
        }


        /// <summary>
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}", Name = "DeleteImage")]
        public async Task<IActionResult> DeleteImage(long id)
        {
            ICollection<IActionResult> fileList = new List<IActionResult>();

            Feature feature = _context.Feature.Find(id);
            long house_id = GetHouseIdFromFeatureId(id);

            var container = client.GetContainerReference(ContainerName + house_id);
            if (!await container.ExistsAsync())
            {
                return NoContent();
            }

            List<string> imageNames = new List<string>();
            _context.Media.Where(m => m.Feature == feature).ToList().ForEach(m => imageNames.Add(m.MediaName));

            foreach (string imgName in imageNames)
            {
                CloudBlockBlob image = container.GetBlockBlobReference(imgName);
                image.DeleteIfExistsAsync();
            }

            return Ok();
        }


        /// <summary>
        /// Get HouseId corresponding to a feature.
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        private long GetHouseIdFromFeatureId(long id)
        {
            Feature feature = _context.Feature.Where(f => f.Id == id)
                .Include(x => x.Category).SingleOrDefault();
            long cat_id = feature.Category.Id;
            long house_id = _context.Categories.Where(x => x.Id == cat_id)
                .Include(h => h.House)
                .SingleOrDefault().House.Id;
            return house_id;
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
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddDays(1);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.List;
            string sasContainerToken = blob.GetSharedAccessSignature(sasConstraints);
            return blob.Uri.AbsoluteUri + sasContainerToken;
        }
    }
}