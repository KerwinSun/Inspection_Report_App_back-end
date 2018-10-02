using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InspectionReport.Services.Interfaces
{
    public interface IImageService
    {
        List<string> GetUriResultsForFeature(long featureId, out HttpStatusCode outStatusCode, ClaimsPrincipal userClaim );
        void PostImageForFeature(long featureId, IFormFileCollection files, out HttpStatusCode outStatusCode);
        void DeleteImageForFeature(long featureId, string fileName, out HttpStatusCode outStatusCode, ClaimsPrincipal userClaim);
    }
}
