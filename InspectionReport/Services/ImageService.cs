﻿using ImageMagick;
using InspectionReport.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace InspectionReport.Services.Interfaces
{
    /// <summary>
    /// Business logic to do with CRUD of images. 
    /// </summary>
    public class ImageService : IImageService
    {
        private readonly ReportContext _context;
        private readonly IAuthorizeService _authorizeService;
        private const string ContainerName = "house";

        private readonly CloudBlobClient client;

        public ImageService(ReportContext reportContext, IAuthorizeService authorizeService)
        {
            _authorizeService = authorizeService;
            _context = reportContext;
            String storageConnectionString =
                "DefaultEndpointsProtocol=https;AccountName=reportpictures;AccountKey=3cxwdbIYl0MBEy0Aaa0TCuUmBZ3KHmBjT2bogu/IUTsU2VPhxPo38Vi/AKXy+tQB//VKTm0VQZ7ewUqJHZGDbQ==;EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            client = storageAccount.CreateCloudBlobClient();
        }

        /// <summary>
        /// Gets a list of image urls for a feature.
        /// 
        /// Possible HTTP status codes:
        /// 1. NotFound - no house found fo feature id, or feature id not found.
        /// 2. NoContent - no container for image
        /// 3. Unauthorized - user not belonging to house
        /// 4. Ok - image urls returned.
        /// 
        /// </summary>
        /// <param name="featureId"></param>
        /// <param name="outStatusCode"></param>
        /// <returns></returns>
        public List<string> GetUriResultsForFeature(long featureId, out HttpStatusCode outStatusCode, ClaimsPrincipal userClaim)
        {
            // Get feature based on ID supplied in Header.
            Feature feature = _context.Feature.Find(featureId);
            if (feature == null)
            {
                outStatusCode = HttpStatusCode.NotFound;
                return new List<string>();
            }

            long house_id = GetHouseIdFromFeatureId(featureId);
            if (house_id == 0)
            {
                outStatusCode = HttpStatusCode.NotFound;
                return new List<string>();
            }

            if (!_authorizeService.AuthorizeUserForHouse(house_id, userClaim))
            {
                outStatusCode = HttpStatusCode.Unauthorized;
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
                .Select(imgName => GetBlobSAsUri(container.GetBlockBlobReference(imgName)))
                .ToList();

            outStatusCode = HttpStatusCode.OK;
            return uriResults;
        }

        /// <summary>
        /// Upload images in form files to the azure storage.
        /// 
        /// Possible HTTP status codes:
        /// 1. NotFound - no house found fo feature id, or feature id not found.
        /// 2. BadRequest - no image files.
        /// 3. NoContent - upload is fine. 
        /// 
        /// </summary>
        /// <param name="featureId"></param>
        /// <param name="files"></param>
        /// <param name="outStatusCode"></param>
        public void PostImageForFeature(long featureId, IFormFileCollection files, out HttpStatusCode outStatusCode)
        {
            // Get feature based on ID supplied in Header.
            Feature feature = _context.Feature
                .Where(f => f.Id == featureId)
                .Include(f => f.ImageFileNames)
                .SingleOrDefault();

            if (feature == null)
            {
                outStatusCode = HttpStatusCode.NotFound;
                return;
            }

            long house_id = GetHouseIdFromFeatureId(featureId);
            if (house_id == 0)
            {
                outStatusCode = HttpStatusCode.NotFound;
                return;
            }

            // Containers on Azure are named "House" + house_id. E.g. House1 is the container
            // for House with ID = 1.
            string container_name = ContainerName + house_id;
            var container = client.GetContainerReference(container_name);
            bool containerCreated = container.CreateIfNotExistsAsync().Result;

            // Check for any uploaded file  
            if (files == null || files.Count == 0)
            {
                outStatusCode = HttpStatusCode.BadRequest; //No image uploaded.
                return;
            }

            List<Task> asyncTasks = new List<Task>();
            //Loop through uploaded files  
            foreach (IFormFile postedFile in files)
            {
                List<string> validTypes = new List<string>()
                    {
                        "image/png",
                        "image/jpeg",
                        "image/hevc",
                        "image/heif",
                        "image/heic"
                    };

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

                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US"); //need to change culture so that english is always appended.
                    blockBlobImage.Metadata.Add("DateCreated", DateTime.UtcNow.ToLongDateString());
                    blockBlobImage.Metadata.Add("TimeCreated", DateTime.UtcNow.ToLongTimeString());

                    MemoryStream memoryStream = new MemoryStream();
                    blockBlobImage.Properties.ContentType = postedFile.ContentType;
                    MagickImage image = new MagickImage(postedFile.OpenReadStream());
                    image.AutoOrient();

                    Task memTask = memoryStream.WriteAsync(image.ToByteArray(), 0, image.ToByteArray().Length);
                    asyncTasks.Add(memTask);

                    memoryStream.Position = 0;
                    Task uploadTask = blockBlobImage.UploadFromStreamAsync(memoryStream);
                    asyncTasks.Add(uploadTask);


                    // Check that media object doesn't already exist.
                    Media existingMedia = _context.Media
                        .Where(m => m.Feature == feature && m.MediaName == fileName)
                        .SingleOrDefault();
                    if (existingMedia == null)
                    {
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

            //Wait for all async tasks to finish.
            Task.WaitAll(asyncTasks.ToArray());
            UpdateNumOfImages(feature);

            outStatusCode = HttpStatusCode.NoContent; //All creation was successful
            return;
        }

        /// <summary>
        /// Deleting images belong to a feature.
        /// 
        /// Possible HTTP status codes:
        /// 1.Not found: feature id, house id (for feature), image name, or container doesn't exist
        /// 2.Unauthorized: user did not log in for houseId.
        /// 3.No content: delete suceessful.
        /// </summary>
        /// <param name="featureId"></param>
        /// <param name="outStatusCode"></param>
        public void DeleteImageForFeature(long featureId, string imageName, out HttpStatusCode outStatusCode, ClaimsPrincipal userClaim)
        {
            // Get feature based on ID supplied in Header.
            Feature feature = _context.Feature
                .Where(f => f.Id == featureId)
                .Include(f => f.ImageFileNames)
                .SingleOrDefault();


            if (feature == null)
            {
                outStatusCode = HttpStatusCode.NotFound;
                return;
            }

            long houseId = GetHouseIdFromFeatureId(featureId);
            if (houseId == 0)
            {
                outStatusCode = HttpStatusCode.NotFound;
                return;
            }

            if (!_authorizeService.AuthorizeUserForHouse(houseId, userClaim))
            {
                outStatusCode = HttpStatusCode.Unauthorized;
                return;
            }

            // Check if the container exists
            var container = client.GetContainerReference(ContainerName + houseId);
            if (!container.ExistsAsync().Result)
            {
                outStatusCode = HttpStatusCode.NotFound;
                return;
            }

            // Remove the media CloudBlockBlob record
            CloudBlockBlob image = container.GetBlockBlobReference(imageName);
            image.DeleteIfExistsAsync().Wait();

            // Remove the media record from the media table 
            if (!DeleteMediaFromTable(imageName, featureId))
            {
                outStatusCode = HttpStatusCode.NotFound;
                return;
            }

            UpdateNumOfImages(feature);
            outStatusCode = HttpStatusCode.NoContent;
            return;
        }

        /// <summary>
        /// Delete the corresponding media record in the table if it exists.
        /// retruns whether the table record existed.
        /// </summary>
        /// <param name="string image_name, long feature_id"></param>
        /// <returns>IActionResult for HTTP responses</returns>
        private bool DeleteMediaFromTable(string imageName, long featureId)
        {
            Media mediaToDelete = _context
                .Media
                .SingleOrDefault(m => m.MediaName == imageName && m.Feature.Id == featureId);

            if (mediaToDelete != null)
            {
                _context.Media.Remove(mediaToDelete);
                _context.SaveChanges();
                return true;
            }

            return false;
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

            long categoryId = feature.Category.Id;
            long houseId = _context.Categories.Where(x => x.Id == categoryId)
                .Include(h => h.House)
                .SingleOrDefault()
                .House.Id;
            return houseId;
        }

        /// <summary>
        /// Method is used to retreive a unique SAS link for a particular blob.
        /// Shared Access Blob Policy dictates the time period before the link expires
        /// and Permissions can be set to enable read/ write/ list the blob.
        /// </summary>
        /// <param name="blob"></param>
        /// <returns></returns>
        private string GetBlobSAsUri(CloudBlockBlob blob)
        {
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddDays(1),
                Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.List
            };
            string sasContainerToken = blob.GetSharedAccessSignature(sasConstraints);
            return blob.Uri.AbsoluteUri + sasContainerToken;
        }

        private void UpdateNumOfImages(Feature feature)
        {
            feature.NumOfImages = feature.ImageFileNames.Count;
            _context.SaveChanges();
        }
    }
}
