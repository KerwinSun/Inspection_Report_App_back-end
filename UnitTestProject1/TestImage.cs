using InspectionReport.Controllers;
using InspectionReport.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace UnitTest
{
    class TestImage
    {
        public static readonly IList<string> userNames = new List<string> { "Darius 1", "Darius 2" };
        public static readonly IList<string> houseAddresses = new List<string> { "1 Mike Rd", "2 Mike Rd" };
        public static readonly IList<string> categoryNames = new List<string> { "bedroom1", "bedroom2", "bedroom3", "bedroom4" };
        public static readonly IList<string> featureNames = new List<string> { "door1", "door2", "door3", "door4" };
        public static readonly string featureNotes = "good";
        public static readonly DbContextOptions<InspectionReport.Models.ReportContext> options
            = new DbContextOptionsBuilder<ReportContext>()
                .UseInMemoryDatabase(databaseName: "testWithEntity")
                .Options;
        private const string ContainerName = "house";

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
                context.Users.RemoveRange(context.Users);

                context.SaveChanges();
            };
        }

        /// <summary>
        /// Test the post image method in image controller. 
        /// </summary>
        [TestMethod]
        public void TestPostImage()
        {
            using (var context = new ReportContext(options))
            {
                ImageController imageController = new ImageController(context);

                var request = (HttpWebRequest)HttpWebRequest.Create("http://localhost:62163/api/image");
                request.Method = "POST";
                var stream = request.GetRequestStream();
                var docFile = File.OpenRead("burndownchart.png");
                docFile.CopyTo(stream);
                docFile.Close();
                stream.Close();
                //var response = request.GetResponse();


                Task<IActionResult> result = imageController.PostImage() as Task<IActionResult>;
                Assert.AreEqual(200, result.Status);

                

                
            }
        }
    }
}
