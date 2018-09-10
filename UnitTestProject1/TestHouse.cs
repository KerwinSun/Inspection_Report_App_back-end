using InspectionReport.Controllers;
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
            var options = new DbContextOptionsBuilder<ReportContext>()
                .UseInMemoryDatabase(databaseName: "testWithEntity")
                .Options;
            //Create two users, two houses, four categories, four features.
            using (var context = new ReportContext(options))
            {
                //HouseController houseController = new HouseController(context);

                ////Check that the correct status code is returned.
                //Assert.IsNotNull(returned);
                //Assert.AreEqual(201, returned.StatusCode);
            }

            //Inspect that single user
            using (var context = new ReportContext(options))
            {
                //Assert.AreEqual(1, context.Users.Count());
                //Assert.AreEqual("Test User", context.Users.Single().Name);
            }
        }

        /// <summary>
        /// Test posting a new house with
        ///     a new category
        ///         a new feature
        /// </summary>
        [TestMethod]
        public void TestCreateNewHouseWithNewCategoryAndNewFeature()
        {
            string featureName = "newFeature";
            string categoryName = "newCategory";
            string houseAddress = "newHouse";
            using (var context = new ReportContext(options))
            {
                HouseController houseController = new HouseController(context);

                Feature newFeature = new Feature
                {
                    Name = featureName,
                    Notes = "some notes",
                };
                Category newCategory = new Category
                {
                    Name = categoryName,
                    Features = new List<Feature> { newFeature },
                };
                House newHouse = new House
                {
                    Address = houseAddress,
                    ConstructionType = "Wood",
                    Categories = new List<Category> { newCategory },
                    InspectedBy = null,
                };

                CreatedAtRouteResult result =
                    houseController.CreateHouse(newHouse) as CreatedAtRouteResult;
                //Check that the correct status code is returned.
                Assert.IsNotNull(result);
                Assert.AreEqual(201, result.StatusCode);
            }

            using (var context = new ReportContext(options))
            {
                //Verify that a new house, a new category, a new feature is added
                Assert.AreEqual(3, context.House.Count());
                Assert.AreEqual(5, context.Categories.Count());
                Assert.AreEqual(5, context.Feature.Count());

                //Verify the house/cat/feat created are associated correctly;
                House house = context.House
                    .Where(h => h.Address == houseAddress)
                    .Include(h => h.Categories)
                        .ThenInclude(c => c.Features)
                    .Single();
                Assert.IsNotNull(house);

                Assert.AreEqual(1, house.Categories.Count());
                Category category = house.Categories.Single();
                Assert.AreEqual(categoryName, category.Name);

                Assert.AreEqual(1, category.Features.Count());
                Feature feature = category.Features.Single();
                Assert.AreEqual(featureName, feature.Name);
            }
        }

        /// <summary>
        /// Test posting an existing house (house1) with no changes
        ///     a new category
        ///         a new feature
        /// </summary>
        [TestMethod]
        public void TestUpdateHouseWithNewCategoryAndNewFeature()
        {
            string featureName = "newFeature";
            string categoryName = "newCategory";
            House existingHouse;

            //Retrieve the existingHouse1
            using (var context = new ReportContext(options))
            {
                existingHouse = context.House
                    .Where(h => h.Address == houseAddresses[0])
                    .Single();
            }

            using (var context = new ReportContext(options))
            {
                HouseController houseController = new HouseController(context);

                Feature newFeature = new Feature
                {
                    Name = featureName,
                    Notes = "some notes",
                };
                Category newCategory = new Category
                {
                    Name = categoryName,
                    Features = new List<Feature> { newFeature },
                };
                House newHouse = new House
                {
                    Id = existingHouse.Id,
                    Address = existingHouse.Address,
                    ConstructionType = existingHouse.ConstructionType,
                    Categories = new List<Category> { newCategory },
                    InspectedBy = null,
                };

                CreatedAtRouteResult result =
                    houseController.CreateHouse(newHouse) as CreatedAtRouteResult;
                //Check that the correct status code is returned.
                Assert.IsNotNull(result);
                Assert.AreEqual(201, result.StatusCode);
            }

            using (var context = new ReportContext(options))
            {
                //Verify that a new house, a new category, a new feature is added
                Assert.AreEqual(2, context.House.Count());
                Assert.AreEqual(5, context.Categories.Count());
                Assert.AreEqual(5, context.Feature.Count());

                //Verify the house/cat/feat created are associated correctly;
                House house = context.House
                    .Where(h => h.Id == existingHouse.Id)
                    .Include(h => h.Categories)
                        .ThenInclude(c => c.Features)
                    .Single();
                Assert.IsNotNull(house);

                Assert.AreEqual(3, house.Categories.Count());
                Category category = house.Categories
                    .Where(c => c.Name == categoryName)
                    .Single();
                Assert.IsNotNull(category);

                Assert.AreEqual(1, category.Features.Count());
                Feature feature = category.Features
                    .Where(f => f.Name == featureName)
                    .Single();
                Assert.IsNotNull(feature);
            }
        }

        /// <summary>
        /// Test posting an existing house (house1) with an address change
        ///     a new category
        ///         a new feature
        /// </summary>
        [TestMethod]
        public void TestUpdateHouseWithAddressChange()
        {
            string newAddress = "newAddress";
            House existingHouse;
            

            //Retrieve the existingHouse1
            using (var context = new ReportContext(options))
            {
                existingHouse = context.House
                    .Where(h => h.Address == houseAddresses[0])
                        .Include(h => h.Categories)
                            .ThenInclude(c => c.Features)
                    .Single();
            }

            using (var context = new ReportContext(options))
            {
                HouseController houseController = new HouseController(context);

                House newHouse = new House
                {
                    Id = existingHouse.Id,
                    Address = newAddress,
                    ConstructionType = existingHouse.ConstructionType,
                    Categories = existingHouse.Categories,
                    InspectedBy = null,
                };

                CreatedAtRouteResult result =
                    houseController.CreateHouse(newHouse) as CreatedAtRouteResult;
                //Check that the correct status code is returned.
                Assert.IsNotNull(result);
                Assert.AreEqual(201, result.StatusCode);
            }

            using (var context = new ReportContext(options))
            {
                //Verify that a new house, a new category, a new feature is added
                Assert.AreEqual(2, context.House.Count());
                Assert.AreEqual(4, context.Categories.Count());
                Assert.AreEqual(4, context.Feature.Count());

                //Verify all the categories and features are still the same
                House house = context.House
                    .Where(h => h.Id == existingHouse.Id)
                    .Include(h => h.Categories)
                        .ThenInclude(c => c.Features)
                    .Single();
                Assert.IsNotNull(house);
                //Verify the address is updated
                Assert.AreEqual(newAddress, house.Address);

                Assert.AreEqual(2, house.Categories.Count());
                ICollection<Category> categories = house.Categories.ToList();

                foreach (Category category in categories)
                {
                    Assert.AreEqual(1, category.Features.Count());
                }
            }
        }
    }
}
