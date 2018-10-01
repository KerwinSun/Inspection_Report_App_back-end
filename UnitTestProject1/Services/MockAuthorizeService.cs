using InspectionReport.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace UnitTest.Services
{
    public class MockAuthorizeService : IAuthorizeService
    {
        public bool AuthorizeUserForHouse(long houseId, ClaimsPrincipal userClaim)
        {
            return true;
        }
    }
}
