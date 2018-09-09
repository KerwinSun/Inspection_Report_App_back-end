﻿using InspectionReport.Controllers;
using InspectionReport.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTest
{
    /// <summary>
    /// Test house controller
    /// </summary>
    [TestClass]
    public class TestHouse
    {
        public static readonly IList<string> userNames = new List<string> { "Darius 1", "Darius 2" };
        public static readonly IList<string> houseAddresses = new List<string> { "1 Mike Rd", "2 Mike Rd" };
        public static readonly IList<string> categoryNames = new List<string> { "bedroom1", "bedroom2", "bedroom3", "bedroom4" };
        public static readonly IList<string> featureNames = new List<string> { "door1", "door2", "door3", "door4" };
        public static readonly string featureNotes = "good";
        public static readonly DbContextOptions<ReportContext> options 
            = new DbContextOptionsBuilder<ReportContext>()
                .UseInMemoryDatabase(databaseName: "testWithEntity")
                .Options;

        /// <summary>
        /// Class initialize method that sets up the entities in the database.
        /// 
        /// Db name: testWithEntity
        /// Content:
        ///     Users:  2
        ///     Houses: 2
        ///     
        ///     House1:
        ///         InspectedBy: u1,u2
        ///         Categories: c1
        ///             Features: f1
        ///         Categories: c2
        ///             Features: f2
        ///     House2:
        ///         InspectedBy: u1,u2
        ///         Categories: c3
        ///             Features: f3
        ///         Categories: c4
        ///             Features: f4
        ///             
        /// </summary>
        /// <param name="testContext"></param>
        [TestInitialize]
        public void SetupDb()
        {
            using (var context = new ReportContext(options))
            {

                User user1 = new User
                {
                    Name = userNames[0]
                };
                User user2 = new User
                {
                    Name = userNames[1]
                };

                House house1 = new House
                {
                    Address = houseAddresses[0],
                    InspectionDate = DateTime.Today,
                    ConstructionType = "Wood"
                };

                House house2 = new House
                {
                    Address = houseAddresses[1],
                    InspectionDate = DateTime.Today,
                    ConstructionType = "Wood"
                };

                //Four house users, to simulate both houses inspected by both users
                HouseUser hu1 = new HouseUser
                {
                    House = house1,
                    User = user1
                };
                HouseUser hu2 = new HouseUser
                {
                    House = house1,
                    User = user2
                };
                HouseUser hu3 = new HouseUser
                {
                    House = house2,
                    User = user1,
                };
                HouseUser hu4 = new HouseUser
                {
                    House = house2,
                    User = user2
                };
                house1.InspectedBy = new List<HouseUser> { hu1, hu2 };
                house2.InspectedBy = new List<HouseUser> { hu3, hu4 };

                Category category1 = new Category
                {
                    Name = categoryNames[0],
                    House = house1,
                };
                Category category2 = new Category
                {
                    Name = categoryNames[1],
                    House = house1,
                };
                Category category3 = new Category
                {
                    Name = categoryNames[2],
                    House = house2,
                };
                Category category4 = new Category
                {
                    Name = categoryNames[3],
                    House = house2
                };
                Feature feature1 = new Feature
                {
                    Name = featureNames[0],
                    Category = category1,
                    Notes = featureNotes,
                };
                Feature feature2 = new Feature
                {
                    Name = featureNames[1],
                    Category = category2,
                    Notes = featureNotes,
                };
                Feature feature3 = new Feature
                {
                    Name = featureNames[2],
                    Category = category3,
                    Notes = featureNotes,
                };
                Feature feature4 = new Feature
                {
                    Name = featureNames[3],
                    Category = category4,
                    Notes = featureNotes,
                };

                context.Feature.Add(feature1);
                context.Feature.Add(feature2);
                context.Feature.Add(feature3);
                context.Feature.Add(feature4);
                context.SaveChanges();
            }
        }

        [TestCleanup]
        public void ClearDb()
        {
            using (var context = new ReportContext(options))
            {
                context.Feature.RemoveRange(context.Feature);
                context.Categories.RemoveRange(context.Categories);
                context.HouseUser.RemoveRange(context.HouseUser);
                context.House.RemoveRange(context.House);
                context.Users.RemoveRange(context.Users);

                context.SaveChanges();
            };
        }
        /// <summary>
        /// Test the get all method in house controller. 
        /// </summary>
        [TestMethod]
        public void TestGetAll()
        {
            //Create two users, two houses, four categories, four features.
            using (var context = new ReportContext(options))
            {
                HouseController houseController = new HouseController(context);
                OkObjectResult result = houseController.GetAll() as OkObjectResult;
                ICollection<House> housesGot = result.Value as ICollection<House>;

                // Check whether houses are returned
                Assert.AreEqual(200, result.StatusCode);
                Assert.AreEqual(2, context.House.Count());
                Assert.AreEqual(2, housesGot.Count);

                House house1 = housesGot.Where(x => x.Address == houseAddresses[0]).Single();
                House house2 = housesGot.Where(x => x.Address == houseAddresses[1]).Single();

                Assert.IsNotNull(house1);
                Assert.IsNotNull(house2);

                // Check categories for first house
                ICollection<Category> categories1 = house1.Categories;
                Assert.IsNotNull(categories1);
                Assert.AreEqual(2, categories1.Count);
                Assert.AreEqual(1, categories1.ElementAt(0).Features.Count);

                ICollection<Category> categories2 = house2.Categories;
                Assert.IsNotNull(categories2);
                Assert.AreEqual(2, categories2.Count);
                Assert.AreEqual(1, categories2.ElementAt(0).Features.Count);
            }
        }

        /// <summary>
        /// Test the get by id method in house controller. 
        /// </summary>
        [TestMethod]
        public void TestGetById()
        {
            //Get a house by a specified id
            using (var context = new ReportContext(options))
            {
                HouseController houseController = new HouseController(context);

                // Get houses with database-specified ids
                House selection1 = context.House.Where(x => x.Address == houseAddresses[0]).Single();
                House selection2 = context.House.Where(x => x.Address == houseAddresses[1]).Single();

                OkObjectResult result1 = houseController.GetById(selection1.Id) as OkObjectResult;
                House retrievedHouse1 = result1.Value as House;
                OkObjectResult result2 = houseController.GetById(selection1.Id) as OkObjectResult;
                House retrievedHouse2 = result1.Value as House;

                // Check if houses actually exist
                Assert.IsNotNull(retrievedHouse1);
                Assert.IsNotNull(retrievedHouse2);

                // Check if sub-elements are still there
                Assert.AreEqual(houseAddresses[0], retrievedHouse1.Address);
                Assert.AreEqual(2, retrievedHouse1.Categories.Count);
                Assert.AreEqual(2, retrievedHouse1.Categories.ElementAt(0).Features.Count);
                

            }
        }

        /// <summary>
        /// Test the delete method in house controller. 
        /// </summary>
        [TestMethod]
        public void TestDelete()
        {
            //Get a house by a specified id
            using (var context = new ReportContext(options))
            {
                HouseController houseController = new HouseController(context);

                // Get houses with database-specified ids
                House selection1 = context.House.Where(x => x.Address == houseAddresses[0]).Single();
                House selection2 = context.House.Where(x => x.Address == houseAddresses[1]).Single();

                long idToDelete = selection1.Id;
                long idToNotDelete = selection2.Id;

                OkObjectResult result1 = houseController.GetById(idToDelete) as OkObjectResult;
                House retrievedHouse1 = result1.Value as House;
                OkObjectResult result2 = houseController.GetById(idToNotDelete) as OkObjectResult;
                House retrievedHouse2 = result1.Value as House;

                // Check if houses actually exist
                Assert.IsNotNull(retrievedHouse1);
                Assert.IsNotNull(retrievedHouse2);

                // Delete selected house
                houseController.Delete(idToDelete);

                NotFoundResult deletedResult1 = houseController.GetById(idToDelete) as NotFoundResult;
                OkObjectResult notdeletedResult2 = houseController.GetById(idToNotDelete) as OkObjectResult;

                // Check if right house was deleted
                Assert.IsNotNull(deletedResult1);
                Assert.IsNotNull(notdeletedResult2);


            }
        }
    }
}
