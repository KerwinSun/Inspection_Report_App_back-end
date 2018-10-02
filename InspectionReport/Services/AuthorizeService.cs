using InspectionReport.Models;
using InspectionReport.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InspectionReport.Services
{
    /// <summary>
    /// Business logic to do with authorization of users in different controllers
    /// </summary>
    public class AuthorizeService : IAuthorizeService
    {
        private readonly ReportContext _context;
        private readonly UserManager<ApplicationUser> _userManager = null;

        public AuthorizeService(ReportContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// This method checks whether a user is authorized to access that house id.
        /// A user is authorized iff he/she inspected it.
        /// </summary>
        /// <param name="houseId"></param>
        /// <returns></returns>
        public bool AuthorizeUserForHouse(long houseId, ClaimsPrincipal userClaim)
        {
            ApplicationUser appUser = _userManager.GetUserAsync(userClaim).Result;
            User user = _context.User.Where(u => u.AppLoginUser == appUser)
                            .Include(u => u.Inspected)
                            .SingleOrDefault();

            return user == null ? false 
                : user.Inspected == null ? false 
                : user.Inspected.Select(h => h.HouseId).Contains(houseId);
        }
    }
}
