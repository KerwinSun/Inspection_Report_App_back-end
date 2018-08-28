using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.AspNetCore.Mvc;
using InspectionReport.Models;

namespace InspectionReport.Controllers
{
    [Route("api/DbSetup")]
    [ApiController]
    public class DbSetupController : ControllerBase
    {
        private readonly ReportContext _context;

        public DbSetupController(ReportContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Set up your database here for testing purpose.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Create()
        {
            User user = new User
            {
                Name = "Darius is a cat"
            };

            House house = new House
            {
                Address = "21 Darius Road",
                ConstructionType = "Wood",
                InspectionDate = DateTime.Today
            };

            HouseUser hu = new HouseUser
            {
                House = house,
                User = user
            };

            user.Inspected = new List<HouseUser> { hu };
            house.InspectedBy = new List<HouseUser> { hu };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok();
        }


    }
}