using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InspectionReport.Services.Interfaces
{
    public interface IAuthorizeService
    {
        bool AuthorizeUserForHouse(long houseId, ClaimsPrincipal userClaim);
    }
}
