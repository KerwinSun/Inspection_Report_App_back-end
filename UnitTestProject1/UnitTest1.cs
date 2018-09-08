using InspectionReport.Controllers;
using InspectionReport.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var options = new DbContextOptionsBuilder<ReportContext>()
               .UseInMemoryDatabase(databaseName: "report cotext test")
               .Options;

            // Run the test against one instance of the context
            using (var context = new ReportContext(options))
            {
                UserController userController = new UserController(context);
                User newUser = new User
                {
                    Name = "Test User"
                };
                userController.CreateUser(newUser);
            }

            // Use a separate instance of the context to verify correct data was saved to database
            using (var context = new ReportContext(options))
            {
                Assert.AreEqual(1, context.Users.Count());
                Assert.AreEqual("Test User", context.Users.Single().Name);
            }
        }
    }
}
