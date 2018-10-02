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
using Microsoft.AspNetCore.Authorization;
using InspectionReport.Services.Interfaces;
using System.Net;
using System.Globalization;
using System.Threading;

namespace InspectionReport.Controllers
{
    [Authorize]
    [Route("api/Image")]
    public class ImageController : Controller
    {
        private const string ContainerName = "house";
        private readonly ReportContext _context;

        private readonly CloudBlobClient client;
        private readonly IAuthorizeService _authorizeService;
        private readonly IImageService _imageService;

        /// <summary>
        /// The constructor initialises the Blob Strage and the context.
        /// TODO: Remove connection string, and replace with Azure Key Vault once 
        /// Authentication is complete.
        /// </summary>
        /// <param name="context"></param>
        public ImageController(ReportContext context, IAuthorizeService authorizeService, IImageService imageService)
        {
            _context = context;
            _authorizeService = authorizeService;
            _imageService = imageService;
            
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
        public IActionResult GetImage(long id)
        {
            List<string> uriResults = _imageService.GetUriResultsForFeature(id, out HttpStatusCode outStatus, HttpContext.User);
            switch (outStatus)
            {
                case HttpStatusCode.NotFound:
                    return NotFound();
                case HttpStatusCode.NoContent:
                    return NoContent();
                case HttpStatusCode.OK:
                    return Ok(uriResults);
                case HttpStatusCode.Unauthorized:
                    return Unauthorized();
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// HTTP Post handles the posting of a particular media (image) type to the Azure
        /// Blob Storge. The request expects one or more Images (restricted to PNG and JPEG) 
        /// files to be included, along with a header with the key of: "feature-id" and the 
        /// corresponding value pair is the id of the feature to which the image must be uploaded.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult PostImage()
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

            _imageService.PostImageForFeature(feature_id, requestForm?.Files, out HttpStatusCode statusCode);

            switch (statusCode)
            {
                case HttpStatusCode.NotFound:
                    return NotFound();
                case HttpStatusCode.BadRequest:
                    return BadRequest("No file found in form.");
                case HttpStatusCode.NoContent:
                    return NoContent();
                default:
                    throw new NotImplementedException();
            }
        }


        /// <summary>
        /// This HTTP Delete method handles the deletion of particular images inside the Azure
        /// Blob Storge's containers.
        /// This method deletes the corresponding database table's record as well.
        /// This method deletes the corresponding image in a one request one image manner. 
        /// 
        /// Note: A header with the key of: "image-name" is required!
        /// </summary>
        /// <param name="id">feature-id</param>
        /// <returns>Task<IActionResult> for HTTP response</returns>
        [HttpDelete("{id}", Name = "DeleteImage")]
        public async Task<IActionResult> DeleteImage(long id)
        {
            // Get Image name in request
            IHeaderDictionary header = HttpContext.Request.Headers;

            string image_name;

            if (header.ContainsKey("image-name"))
            {
                image_name = header["image-name"];
            }
            else
            {
                return BadRequest("No image-name found in the header.");
            }

            
            long house_id = this.GetHouseIdFromFeatureId(id);
            if (house_id == 0)
            {
                return NotFound();
            }
            if(!_authorizeService.AuthorizeUserForHouse(house_id, HttpContext.User))
            {
                return Unauthorized();
            }

            // Check if the container exists
            var container = client.GetContainerReference(ContainerName + house_id);
            if (!await container.ExistsAsync())
            {
                return NoContent();
            }

            // Remove the media CloudBlockBlob record
            CloudBlockBlob image = container.GetBlockBlobReference(image_name);
            await image.DeleteIfExistsAsync();

            // Remove the media record from the media table 
            IActionResult iActionResult = this.DeleteMediaFromTable(image_name, id);
            if (iActionResult.GetType() == typeof(NotFoundResult))
            {
                return NotFound();
            }

            return NoContent();
        }


        /// <summary>
        /// Delete the corresponding media record in the table if it exists.
        /// </summary>
        /// <param name="string image_name, long feature_id"></param>
        /// <returns>IActionResult for HTTP responses</returns>
        private IActionResult DeleteMediaFromTable(string image_name, long feature_id)
        {
            Media mediaToDelete = _context
                .Media
                .SingleOrDefault(m => m.MediaName == image_name && m.Feature.Id == feature_id);

            if (mediaToDelete == null)
            {
                return NotFound();
            }
            else
            {
                _context.Media.Remove(mediaToDelete);
                _context.SaveChanges();

                return NoContent();
            }
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
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddDays(1);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.List;
            string sasContainerToken = blob.GetSharedAccessSignature(sasConstraints);
            return blob.Uri.AbsoluteUri + sasContainerToken;
        }
    }
}