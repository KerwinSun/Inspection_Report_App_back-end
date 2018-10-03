using InspectionReport.Controllers;
using InspectionReport.Models;
using InspectionReport.Services;
using InspectionReport.Services.Interfaces;
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
        public static readonly IAuthorizeService AUTH_SERVICE = new MockAuthorizeService();

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
                    Count = 1,
                    House = house1,
                };
                Category category2 = new Category
                {
                    Name = categoryNames[1],
                    Count = 1,
                    House = house1,
                };
                Category category3 = new Category
                {
                    Name = categoryNames[2],
                    Count = 1,
                    House = house2,
                };
                Category category4 = new Category
                {
                    Name = categoryNames[3],
                    Count = 1,
                    House = house2
                };
                Feature feature1 = new Feature
                {
                    Name = featureNames[0],
                    Category = category1,
                    Comments = featureNotes,
                };
                Feature feature2 = new Feature
                {
                    Name = featureNames[1],
                    Category = category2,
                    Comments = featureNotes,
                };
                Feature feature3 = new Feature
                {
                    Name = featureNames[2],
                    Category = category3,
                    Comments = featureNotes,
                };
                Feature feature4 = new Feature
                {
                    Name = featureNames[3],
                    Category = category4,
                    Comments = featureNotes,
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
                context.User.RemoveRange(context.User);

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
                HouseController houseController = new HouseController(context, AUTH_SERVICE);
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
                HouseController houseController = new HouseController(context, AUTH_SERVICE);

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

                // Check that houses contain the completed field
                Assert.IsNotNull(retrievedHouse1.Completed);
                Assert.IsNotNull(retrievedHouse2.Completed);

                // Check that house 1 feature in category 1 has a valid grade.
                Feature validFeature = new Feature
                {
                    Name = featureNames[3],
                    Comments = featureNotes,
                    Grade = 2,
                };

                Category newCategory = new Category
                {
                    Name = "random category",
                    Count = 1,
                    Features = new List<Feature> { validFeature },
                };

                retrievedHouse1.Categories.Add(newCategory);

                Assert.AreEqual(2, retrievedHouse1.Categories.ElementAt(2).Features.ElementAt(0).Grade);

                // Check if sub-elements are still there
                Assert.AreEqual(houseAddresses[0], retrievedHouse1.Address);
                Assert.AreEqual(3, retrievedHouse1.Categories.Count);
                Assert.AreEqual(1, retrievedHouse1.Categories.ElementAt(0).Features.Count);


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
                HouseController houseController = new HouseController(context, AUTH_SERVICE);

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
                HouseController houseController = new HouseController(context, null);

                Feature newFeature = new Feature
                {
                    Name = featureName,
                    Comments = "some notes",
                };
                Category newCategory = new Category
                {
                    Name = categoryName,
                    Count = 1,
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
                    houseController.CreateOrUpdateHouse(newHouse) as CreatedAtRouteResult;
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
                Assert.AreEqual(1, category.Count);

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
                HouseController houseController = new HouseController(context, AUTH_SERVICE);

                Feature newFeature = new Feature
                {
                    Name = featureName,
                    Comments = "some notes",
                };
                Category newCategory = new Category
                {
                    Name = categoryName,
                    Count = 1,
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
                    houseController.CreateOrUpdateHouse(newHouse) as CreatedAtRouteResult;
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
                HouseController houseController = new HouseController(context, AUTH_SERVICE);

                House newHouse = new House
                {
                    Id = existingHouse.Id,
                    Address = newAddress,
                    ConstructionType = existingHouse.ConstructionType,
                    Categories = existingHouse.Categories,
                    InspectedBy = null,
                };

                CreatedAtRouteResult result =
                    houseController.CreateOrUpdateHouse(newHouse) as CreatedAtRouteResult;
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

        /// <summary>
        /// Test posting an existing house (house1) with 
        ///     change in category
        ///         new feature for that category
        ///     new category
        /// </summary>
        [TestMethod]
        public void TestUpdateHouseWithCategoryChangeAndNewCategory()
        {
            string newAddress = "newAddress";
            House existingHouse;
            Category existingCategory1;
            Category existingCategory2;
            string newCategoryName = "newCategory";
            string changeCategoryName = "changeCategory";

            //Retrieve the existingHouse1
            using (var context = new ReportContext(options))
            {
                existingHouse = context.House
                    .Where(h => h.Address == houseAddresses[0])
                        .Include(h => h.Categories)
                            .ThenInclude(c => c.Features)
                    .Single();
                existingCategory1 = existingHouse.Categories.ToList().First();
                existingCategory2 = existingHouse.Categories.ToList().Last();
            }

            using (var context = new ReportContext(options))
            {
                HouseController houseController = new HouseController(context, AUTH_SERVICE);

                //Change one of the default category
                Category changedCategory = new Category
                {
                    Id = existingCategory1.Id,
                    Count = existingCategory1.Count,
                    Name = changeCategoryName,
                    Features = existingCategory1.Features,
                };
                //The other category remains the same
                Category remains = new Category
                {
                    Id = existingCategory2.Id,
                    Count = existingCategory2.Count,
                    Name = existingCategory2.Name,
                    Features = existingCategory2.Features
                };

                //Add a new category to existing
                Category newCategory = new Category
                {
                    Name = newCategoryName,
                    Count = 1,
                    Features = null,
                };
                List<Category> categoriesForBody
                    = new List<Category> { changedCategory, remains, newCategory };

                House newHouse = new House
                {
                    Id = existingHouse.Id,
                    Address = newAddress,
                    ConstructionType = existingHouse.ConstructionType,
                    Categories = categoriesForBody,
                    InspectedBy = null,
                };

                //PUT A BREAK POINT HERE TO OBSERVE THE NEW HOUSE.
                CreatedAtRouteResult result =
                    houseController.CreateOrUpdateHouse(newHouse) as CreatedAtRouteResult;
                //Check that the correct status code is returned.
                Assert.IsNotNull(result);
                Assert.AreEqual(201, result.StatusCode);
            }

            using (var context = new ReportContext(options))
            {
                //Verify that no new house, one new category, and no feature is added
                Assert.AreEqual(2, context.House.Count());
                Assert.AreEqual(5, context.Categories.Count());
                Assert.AreEqual(4, context.Feature.Count());

                House house = context.House
                    .Where(h => h.Id == existingHouse.Id)
                    .Include(h => h.Categories)
                        .ThenInclude(c => c.Features)
                    .Single();
                Assert.IsNotNull(house);

                //Verify this house has 3 categories
                Assert.AreEqual(3, house.Categories.Count());
                ICollection<Category> categories = house.Categories.ToList();

                //Verify that there is a changed category
                Category testChangedCategory = categories
                    .Where(c => c.Name == changeCategoryName)
                    .Single();
                Assert.IsNotNull(testChangedCategory);

                //Verify that there is a new category
                Category testNewCategory = categories
                    .Where(c => c.Name == newCategoryName)
                    .Single();
                Assert.IsNotNull(testNewCategory);
            }
        }

        /// <summary>
        /// Test posting an existing house (house1) with 
        ///     change in feature for first category
        ///         new feature for that category
        /// </summary>
        [TestMethod]
        public void TestUpdateHouseWithFeatureChangeAndNewFeature()
        {
            string newAddress = "newAddress";
            House existingHouse;
            Category existingCategory1;
            Feature existingFeature1;
            string newFeatureName = "newAddedFeature";
            string changeFeatureName = "changeFeature";


            //Retrieve the existingHouse1
            using (var context = new ReportContext(options))
            {
                existingHouse = context.House
                    .Where(h => h.Address == houseAddresses[0])
                        .Include(h => h.Categories)
                            .ThenInclude(c => c.Features)
                    .Single();
                existingCategory1 = existingHouse.Categories.ToList().First();
                existingFeature1 = existingCategory1.Features.First();
            }

            using (var context = new ReportContext(options))
            {
                HouseController houseController = new HouseController(context, AUTH_SERVICE);

                //Change the default feature
                Feature changedFeature = new Feature
                {
                    Id = existingFeature1.Id,
                    Name = changeFeatureName,
                };

                //Add a new feature to existing
                Feature newFeature = new Feature
                {
                    Name = newFeatureName,
                };

                List<Category> existingCategories
                    = new List<Category> { existingCategory1 };
                List<Feature> featuresForBody
                    = new List<Feature> { changedFeature, newFeature };

                existingCategory1.Features = featuresForBody;

                House newHouse = new House
                {
                    Id = existingHouse.Id,
                    Address = newAddress,
                    ConstructionType = existingHouse.ConstructionType,
                    Categories = existingCategories,
                    InspectedBy = null,
                };

                //PUT A BREAK POINT HERE TO OBSERVE THE NEW HOUSE.
                CreatedAtRouteResult result =
                    houseController.CreateOrUpdateHouse(newHouse) as CreatedAtRouteResult;
                //Check that the correct status code is returned.
                Assert.IsNotNull(result);
                Assert.AreEqual(201, result.StatusCode);
            }

            using (var context = new ReportContext(options))
            {

                //Verify that no new house, no new category and one new feature is added
                Assert.AreEqual(2, context.House.Count());
                Assert.AreEqual(4, context.Categories.Count());
                Assert.AreEqual(5, context.Feature.Count());

                House house = context.House
                    .Where(h => h.Id == existingHouse.Id)
                    .Include(h => h.Categories)
                        .ThenInclude(c => c.Features)
                    .Single();
                Assert.IsNotNull(house);

                // Verify this house has 2 categories
                Assert.AreEqual(2, house.Categories.Count());
                ICollection<Category> categories = house.Categories.ToList();

                // Get the category that was manipulated (with changed and new features)
                Category selectedCategory = categories
                    .Where(c => c.Id == existingCategory1.Id)
                    .SingleOrDefault();

                // Verify that two features are in this category
                Assert.IsNotNull(selectedCategory);
                Assert.AreEqual(2, selectedCategory.Features.Count);

                // Verify that there is a changed feature
                Feature testChangedFeature = selectedCategory.Features
                    .Where(f => f.Name == changeFeatureName)
                    .SingleOrDefault();
                Assert.IsNotNull(testChangedFeature);

                // Verify that there is a new feature
                Feature testNewFeature = selectedCategory.Features
                    .Where(f => f.Name == newFeatureName)
                    .SingleOrDefault();
                Assert.IsNotNull(testNewFeature);
            }
        }

        /// <summary>
        /// Testing post a house with a new house user assignment.
        /// </summary>
        [TestMethod]
        public void TestNewHouseWithHouseUser()
        {
            User firstUser = null;
            User secondUser = null;
            long houseId;

            //Create a house object with new house-user assignment
            using (var context = new ReportContext(options))
            {
                firstUser = context.User.Where(u => u.Name == userNames[0]).Single();
                secondUser = context.User.Where(u => u.Name == userNames[1]).Single();
                HouseController houseController = new HouseController(context, AUTH_SERVICE);

                HouseUser hu = new HouseUser
                {
                    UserId = firstUser.Id
                };
                HouseUser hu2 = new HouseUser
                {
                    UserId = secondUser.Id
                };
                ICollection<HouseUser> hus = new List<HouseUser>
                {
                    hu,
                    hu2
                };


                House house = new House
                {
                    Address = "TestNewHouseWithHouseUser",
                    InspectedBy = hus
                };

                CreatedAtRouteResult result =
                    houseController.CreateOrUpdateHouse(house) as CreatedAtRouteResult;
                Assert.IsNotNull(result);

                Assert.AreEqual(201, result.StatusCode);
                House houseRecieved = result.Value as House;
                houseId = houseRecieved.Id;
            };

            using (var context = new ReportContext(options))
            {
                HouseController houseController = new HouseController(context, AUTH_SERVICE);
                OkObjectResult okResult = houseController.GetById(houseId) as OkObjectResult;
                House houseAsPerAPI = okResult.Value as House;
                Assert.IsNotNull(houseAsPerAPI);

                ICollection<HouseUser> houseUsersAsPerAPI = houseAsPerAPI.InspectedBy;

                Assert.IsNotNull(houseUsersAsPerAPI);
                Assert.AreEqual(2, houseUsersAsPerAPI.Count);

                foreach (HouseUser hu in houseUsersAsPerAPI)
                {
                    Assert.IsTrue(hu.HouseId == houseId);
                    Assert.IsTrue(hu.UserId == firstUser.Id
                        || hu.UserId == secondUser.Id);
                }
            }
        }

        /// <summary>
        /// Testing post an old house with a new house user assignment.
        /// 
        /// All the old house user assignments should continue to exist
        /// A new house user assignment should be added. 
        /// </summary>
        [TestMethod]
        public void TestExistingHouseWithNewHouseUser()
        {
            House existing = null;
            User newUser = new User
            {
                Name = "new inspector",
            };
            string newAddressName = "TestExistingHouseWithHouseUser";

            //Add a new user
            using (var context = new ReportContext(options))
            {
                context.User.Add(newUser);
                context.SaveChanges();
            }

            //Create an existing house object (with an ID) with new house-user assignment
            using (var context = new ReportContext(options))
            {
                existing = context.House.Where(h => h.Address == houseAddresses[0]).Single();
                HouseController houseController = new HouseController(context, AUTH_SERVICE);

                HouseUser newHu = new HouseUser
                {
                    UserId = newUser.Id,
                };

                ICollection<HouseUser> hus = new List<HouseUser>
                {
                    newHu,
                };


                House house = new House
                {
                    Id = existing.Id,
                    Address = newAddressName,
                    InspectedBy = hus
                };

                CreatedAtRouteResult result =
                    houseController.CreateOrUpdateHouse(house) as CreatedAtRouteResult;
                Assert.IsNotNull(result);

                Assert.AreEqual(201, result.StatusCode);
            };

            //Should now have 3 users on house.
            //Address should also be updated.
            using (var context = new ReportContext(options))
            {
                HouseController houseController = new HouseController(context, AUTH_SERVICE);
                OkObjectResult okResult = houseController.GetById(existing.Id) as OkObjectResult;
                House houseAsPerAPI = okResult.Value as House;
                Assert.IsNotNull(houseAsPerAPI);

                Assert.AreEqual(newAddressName, houseAsPerAPI.Address);

                ICollection<HouseUser> houseUsersAsPerAPI = houseAsPerAPI.InspectedBy;

                Assert.IsNotNull(houseUsersAsPerAPI);
                Assert.AreEqual(3, houseUsersAsPerAPI.Count);

                //Can find the new user
                HouseUser newHouseUserFound = houseUsersAsPerAPI
                    .Where(hu => hu.UserId == newUser.Id).SingleOrDefault();
                Assert.IsNotNull(newHouseUserFound);
            }
        }

        /// <summary>
        /// Testing ordering of categories and features.
        /// Categories were sent as: order 1, 0.
        /// Feature were sent as: {1, 0, 2}, {0}.
        /// 
        /// The expected results:
        ///     First category:  order 0,
        ///         First feature: order 0.
        /// </summary>
        [TestMethod]
        public void TestCategoryAndFeatureOrder()
        {
            //====FEATURES=====
            //c1: ORDER 1
            //  f1-f3: ORDER: 1, 0, 2
            //c2: ORDER 0
            //  f4: ORDER: 0
            Feature f1 = new Feature
            {
                Name = "f1",
                Order = 1
            };

            Feature f2 = new Feature
            {
                Name = "f2",
                Order = 0
            };
            Feature f3 = new Feature
            {
                Name = "f3",
                Order = 2
            };

            Feature f4 = new Feature
            {
                Name = "f4",
                Order = 0
            };

            Category c1 = new Category
            {
                Name = "c1",
                Order = 1,
                Features = { f1, f2, f3 }
            };

            Category c2 = new Category
            {
                Name = "c2",
                Order = 0,
                Features = { f4 }
            };
            House house = new House
            {
                Address = "TestNewHouseWithHouseUser",
                Categories = { c1, c2 }
            };

            long houseId;
            long cat2Id;
            long feat2Id;

            //Create a house object with new house-user assignment
            using (var context = new ReportContext(options))
            {
                HouseController houseController = new HouseController(context, AUTH_SERVICE);

                CreatedAtRouteResult result =
                    houseController.CreateOrUpdateHouse(house) as CreatedAtRouteResult;
                Assert.IsNotNull(result);

                Assert.AreEqual(201, result.StatusCode);
                House houseRecieved = result.Value as House;
                houseId = houseRecieved.Id;
                Category firstCat = house.Categories.Where(c => c.Name == c2.Name).SingleOrDefault();
                cat2Id = firstCat.Id;

                Category secondCat = house.Categories.Where(c => c.Name == c1.Name).SingleOrDefault();
                feat2Id = secondCat.Features.Where(f => f.Name == f2.Name).SingleOrDefault().Id;
            };

            using (var context = new ReportContext(options))
            {
                HouseController houseController = new HouseController(context, AUTH_SERVICE);
                OkObjectResult okResult = houseController.GetById(houseId) as OkObjectResult;
                House houseAsPerAPI = okResult.Value as House;
                Assert.IsNotNull(houseAsPerAPI);

                Category firstCategory = houseAsPerAPI.Categories.FirstOrDefault();
                
                Assert.AreEqual(0, firstCategory.Order);
                Assert.AreEqual(cat2Id, firstCategory.Id); //Category2 was the first category

                Category secondCategory = houseAsPerAPI.Categories.LastOrDefault();
                Feature firstFeatureOfFirstCategory = secondCategory.Features.FirstOrDefault();

                Assert.AreEqual(0, firstFeatureOfFirstCategory.Order);
                Assert.AreEqual(feat2Id, firstFeatureOfFirstCategory.Id); //Feature2 was the first feature
            }

        }
    }
}
