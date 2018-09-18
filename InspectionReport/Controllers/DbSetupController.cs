﻿using System;
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
			
			
			//Test categroy
			Category cat = new Category
            {
                Name = "testA"
            };

            List<Category> catList = new List<Category>
            {
                cat
            };

            House house2 = new House
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
                Comments = "feature notes here",
                Category = catList[0]
            };

            List<Feature> featureList = new List<Feature>
            {
                feature
            };

            Category cat2 = new Category
            {
                Name = "sadmike",
                Features = featureList
            };

            User user2 = new User
            {
                Name = "Pulkit Dirty Stuff",
            };

            House house3 = new House
            {
                Address = "",
                ConstructionType = "old",
                InspectionDate = new DateTime(2018, 1, 1),
                Categories = catList

            };

            HouseUser hu2 = new HouseUser
            {
                User = user2,
                House = house3,

            };

            user2.Inspected = new List<HouseUser> { hu2 };
            house3.InspectedBy = new List<HouseUser> { hu2 };
            _context.Users.Add(user2);
            _context.SaveChanges();


            _context.Categories.Add(cat);
            _context.Categories.Add(cat2);

            _context.Feature.Add(feature);

            _context.SaveChanges();

            return Ok();
        }

        [HttpPost("testing")]
        public IActionResult CreateTests()
        {
            User user = new User
            {
                Name = "Tonald Drump"
            };

            Feature feat = new Feature { Name = "Benchtop", Comments = "The benchtop looks very nice for eating and yeah" };
            Feature feat2 = new Feature { Name = "Taps", Comments = "The taps are not working :(" };

            List<Feature> featList = new List<Feature>
            {
                feat,
                feat2
            };

            //Test category
            Category cat = new Category { Name = "Kitchen", Features = featList };

            List<Category> catList = new List<Category>
            {
                cat
            };

            House house = new House
            {
                Address = "21 University of Auckland Road",
                ConstructionType = "Concrete",
                InspectionDate = DateTime.Today,
                Categories = catList
            };

            _context.House.Add(house);
            _context.SaveChanges();
            return Ok();
        }
    }
   
}