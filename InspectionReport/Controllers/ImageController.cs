using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols;
using System.Threading.Tasks;
using System.IO;
using InspectionReport.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting.Internal;
using ImageMagick;
using System.Linq;
using System.Net.Http;
using System.Net;
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

        public ImageController(ReportContext context)
        {
            _context = context;
            String storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=reportpictures;AccountKey=3cxwdbIYl0MBEy0Aaa0TCuUmBZ3KHmBjT2bogu/IUTsU2VPhxPo38Vi/AKXy+tQB//VKTm0VQZ7ewUqJHZGDbQ==;EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            client = storageAccount.CreateCloudBlobClient();
        }

        [HttpGet("{id}", Name = "GetImage")]
        public async Task<IActionResult> GetImage(long id)
        {
            ICollection<FileContentResult> fileList = new List<FileContentResult>();

            Feature feature = _context.Feature.Find(id);
            long house_id = GetHouseIdFromFeatureId(id);

            var container = client.GetContainerReference(ContainerName + house_id);
            if (!await container.ExistsAsync())
            {
                //return fileList;
                return NoContent();
            }

            // Adapted from https://stackoverflow.com/questions/24312527/azure-blob-storage-downloadtobytearray-vs-downloadtostream
            List<string> imageNames = new List<string>();
            _context.Media.Where(m => m.Feature == feature).ToList().ForEach(m => imageNames.Add(m.MediaName));

            foreach (string imgName in imageNames)
            {
                CloudBlockBlob image = container.GetBlockBlobReference(imgName);
                await image.FetchAttributesAsync();
                long fileByteLength = image.Properties.Length;
                byte[] fileContent = new byte[fileByteLength];
                for (int i = 0; i < fileByteLength; i++)
                {
                    fileContent[i] = 0x20;
                }
                await image.DownloadToByteArrayAsync(fileContent, 0);
                return File(fileContent, "image/jpeg");
                //fileList.Add(File(fileContent, "image/jpeg"));
            }
            return Ok();
            //return fileList;
        }

        [HttpPost]
        public async Task<IActionResult> PostImage()
        {
            // Check appropriate file type
            // TODO: restrict to only certain file types.
            /*if (!_supportedMimeTypes.Contains(headers.ContentType.ToString().ToLower()))
            {
                throw new NotSupportedException("Only jpeg and png are supported");
            }*/
            
            IFormCollection requestForm = HttpContext.Request.Form;
            IHeaderDictionary header = HttpContext.Request.Headers;
            long feature_id;

            if (header.ContainsKey("feature-id"))
            {
                feature_id = Convert.ToInt64(header["feature-id"]);
            } else
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
                    IFormFile postedFile = requestForm.Files[i];
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
            } else
            {
                return NoContent(); // No image uploaded.
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


    }
}


