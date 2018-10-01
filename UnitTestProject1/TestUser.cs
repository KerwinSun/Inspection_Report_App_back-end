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
    /// Test user controller.
    /// </summary>
    [TestClass]
    public class TestUser
    {
        /// <summary>
        /// Test the createUser method in UserController
        /// </summary>
        [TestMethod]
        public void TestCreateUsers()
        {
            var options = new DbContextOptionsBuilder<ReportContext>()
               .UseInMemoryDatabase(databaseName: "test create")
               .Options;

            //Create one user
            using (var context = new ReportContext(options))
            {
                UserController userController = new UserController(context);
                User newUser = new User
                {
                    Name = "Test User"
                };
                CreatedAtRouteResult returned = userController.CreateUser(newUser) as CreatedAtRouteResult;

                //Check that the correct status code is returned.
                Assert.IsNotNull(returned);
                Assert.AreEqual(201, returned.StatusCode);
            }

            //Inspect that single user
            using (var context = new ReportContext(options))
            {
                Assert.AreEqual(1, context.User.Count());
                Assert.AreEqual("Test User", context.User.Single().Name);
            }

            //Create more users
            using (var context = new ReportContext(options))
            {
                UserController userController = new UserController(context);
                User newUser2 = new User
                {
                    Name = "Test User 2"
                };
                userController.CreateUser(newUser2);
            }

            //Inspect both users are in
            using (var context = new ReportContext(options))
            {
                Assert.AreEqual(2, context.User.Count());
            }
        }

        /// <summary>
        /// Test the GetAll() method, by creating 10 records.
        /// </summary>
        [TestMethod]
        public void TestGetAll()
        {
            var options = new DbContextOptionsBuilder<ReportContext>()
                .UseInMemoryDatabase(databaseName: "testGetAll")
                .Options;
            using (var context = new ReportContext(options))
            {
                for (int i = 0; i < 10; i++)
                {
                    UserController userController = new UserController(context);
                    User newUser = new User
                    {
                        Name = "Test User"
                    };
                    userController.CreateUser(newUser);
                }
            }

            using (var context = new ReportContext(options))
            {
                UserController userController = new UserController(context);

                OkObjectResult result = userController.GetAll() as OkObjectResult;
                ICollection<User> users = result.Value as ICollection<User>;

                Assert.AreEqual(200, result.StatusCode);
                Assert.AreEqual(10, context.User.Count());
            }
        }

        /// <summary>
        /// Test the GetById() method, making sure all the eager-fetching
        /// fields exist. 
        /// </summary>
        [TestMethod]
        public void TestGetById()
        {
            
            var options = new DbContextOptionsBuilder<ReportContext>()
                .UseInMemoryDatabase(databaseName: "testGetById")
                .Options;

            //Set up the objects first.
            User user = new User
            {
                Name = "Darius is a cat"
            };

            string address1 = "21 Darius Road";
            string address2 = "22 Darius Road";

            House house = new House
            {
                Address = address1,
                ConstructionType = "Wood",
                InspectionDate = DateTime.Today
            };

            House house2 = new House
            {
                Address = address2,
                ConstructionType = "Wood",
                InspectionDate = DateTime.Today
            };

            HouseUser hu = new HouseUser
            {
                House = house,
                User = user
            };

            HouseUser hu2 = new HouseUser
            {
                House = house2,
                User = user
            };

            user.Inspected = new List<HouseUser> { hu, hu2 };

            //Set up the context.
            using (var context = new ReportContext(options))
            {
                context.Add(user);
                context.SaveChanges();
            }

            //Try retreiving 
            using (var context = new ReportContext(options))
            {
                //Find id of user!
                long userId = context.User.Single().Id;

                //Find id of house!
                long houseId1 = context.House.Where(h => h.Address == address1).Single().Id;
                long houseId2 = context.House.Where(h => h.Address == address2).Single().Id;

                UserController userController = new UserController(context);

                OkObjectResult result = userController.GetById(userId) as OkObjectResult;
                User userGot = result.Value as User;

                Assert.AreEqual(200, result.StatusCode);
                Assert.IsNotNull(userGot);
                Assert.AreEqual(userId, userGot.Id);
                Assert.AreEqual("Darius is a cat", user.Name);

                //Should include houseUser
                ICollection<HouseUser> houseUsers = userGot.Inspected;
                Assert.IsNotNull(houseUsers);
                Assert.AreEqual(2, houseUsers.Count);

                //Should then include house
                HouseUser firstHouseUser = houseUsers.Where(x => x.House.Address == address1).Single();
                HouseUser secondHouseUser = houseUsers.Where(x => x.House.Address == address2).Single();
                Assert.IsNotNull(firstHouseUser);
                Assert.IsNotNull(secondHouseUser);

                //Same user
                Assert.AreEqual(userId, firstHouseUser.User.Id);
                Assert.AreEqual(userId, secondHouseUser.User.Id);
                //Two houses
                Assert.AreEqual(houseId1, firstHouseUser.House.Id);
                Assert.AreEqual(houseId2, secondHouseUser.House.Id);
            }
        }
    }
}
