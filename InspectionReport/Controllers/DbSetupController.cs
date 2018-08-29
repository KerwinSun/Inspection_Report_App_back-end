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
                Name = "testA",
                Features = featureList
            };


            _context.Categories.Add(cat);
            _context.House.Add(house);

            _context.SaveChanges();

            return Ok();
        }



    }
}