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

namespace InspectionReport.Controllers
{
    [Route("api/Image")]
    public class ImageController : Controller
    {
        private readonly ReportContext _context;

        private readonly CloudBlobClient client;

        public ImageController(ReportContext context)
        {
            _context = context;
            String storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=reportpictures;AccountKey=3cxwdbIYl0MBEy0Aaa0TCuUmBZ3KHmBjT2bogu/IUTsU2VPhxPo38Vi/AKXy+tQB//VKTm0VQZ7ewUqJHZGDbQ==;EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            client = storageAccount.CreateCloudBlobClient();
        }

        [HttpGet]
        [HttpGet("{id}", Name = "GetImage")]
        public async Task<IActionResult> GetImage(long id)
        {

            var container = client.GetContainerReference("reportpictures");
            if (!await container.ExistsAsync())
            {
                return NoContent();
            }

            CloudBlockBlob image = container.GetBlockBlobReference("Capture.PNG0");
            await image.FetchAttributesAsync();
            long fileByteLength = image.Properties.Length;
            byte[] fileContent = new byte[fileByteLength];
            for (int i = 0; i < fileByteLength; i++)
            {
                fileContent[i] = 0x20;
            }
            await image.DownloadToByteArrayAsync(fileContent, 0);
            return File(fileContent, "image/jpeg");

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

            // One container for each house?
            var container = client.GetContainerReference("reportpictures");
            await container.CreateIfNotExistsAsync();

            var requestForm = HttpContext.Request.Form;

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

                        // Blob reference is the name of the file uploaded. 
                        // This should be named using a Guid which will be entered in the local context in combination
                        // with the ID of the feature/ category to which the image belongs.

                        CloudBlockBlob blockBlobImage = container.GetBlockBlobReference(fileName + i);
                        // await container.CreateIfNotExistsAsync();
                        //CloudBlockBlob blockBlobImage = container.GetBlobReference("img1");

                        blockBlobImage.Metadata.Add("DateCreated", DateTime.UtcNow.ToLongDateString());
                        blockBlobImage.Metadata.Add("TimeCreated", DateTime.UtcNow.ToLongTimeString());

                        MemoryStream memoryStream = new MemoryStream();

                        blockBlobImage.Properties.ContentType = postedFile.ContentType;

                        MagickImage image = new MagickImage(postedFile.OpenReadStream());
                        image.AutoOrient();

                        await memoryStream.WriteAsync(image.ToByteArray(), 0, image.ToByteArray().Length);

                        memoryStream.Position = 0;

                        await blockBlobImage.UploadFromStreamAsync(memoryStream);
                    }
                }
            } else
            {
                return NoContent(); // No image uploaded.
            }
            return Ok();
            // Return status code  
            //return Request.CreateResponse(HttpStatusCode.Created);
        }



    }
}


