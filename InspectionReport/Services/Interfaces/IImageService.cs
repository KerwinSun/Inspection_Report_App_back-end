using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace InspectionReport.Services.Interfaces
{
    public interface IImageService
    {
        List<string> GetUriResultsForFeature(long featureId, out HttpStatusCode outStatusCode);
    }
}
