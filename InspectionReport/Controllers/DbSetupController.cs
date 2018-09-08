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
            Category cat = new Category
            {
                Name = "testA"
            };

            List<Category> catList = new List<Category>();
            catList.Add(cat);

            House house = new House
            {
                Address = "",
                ConstructionType = "old",
                InspectionDate = new DateTime(2015, 1, 1),
                Categories = catList

            };

            // Test Feature
            Feature feature = new Feature
            {
                Name = "a feature name",
                Notes = "feature notes here",
                Category = catList[0]
            };

            List<Feature> featureList = new List<Feature>();
            featureList.Add(feature);

            Category cat2 = new Category
            {
                Name = "sadmike",
                Features = featureList
            };

            User user = new User
            {
                Name = "Pulkit Dirty Stuff",
            };

            House house2 = new House
            {
                Address = "",
                ConstructionType = "old",
                InspectionDate = new DateTime(2018, 1, 1),
                Categories = catList

            };

            HouseUser hu = new HouseUser
            {
                User = user,
                House = house2,

            };

            

            _context.Categories.Add(cat);
            _context.Categories.Add(cat2);

            _context.Feature.Add(feature);

            _context.House.Add(house);

            _context.Users.Add(user);
            _context.House.Add(house2);



            _context.SaveChanges();

            _context.HouseUser.Add(hu);

            _context.SaveChanges();

            return Ok();
        }



    }
}