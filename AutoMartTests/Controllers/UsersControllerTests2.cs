using AutoMart.Controllers;
using AutoMart.Data;
using AutoMart.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;


namespace AutoMart.Controllers.Tests2
{
    [TestClass]
    public class UsersControllerTests
    {
        private ApplicationDbContext context;
        private UsersController controller;

        [TestInitialize]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            context = new ApplicationDbContext(options);
            controller = new UsersController(context, null, null);
        }

        [TestMethod]
        public async Task Delete_UserExists_RedirectsToIndex()
        {
            // Arrange
            var user = new WebUser { Id = "1", UserName = "TestUser" };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.Delete("1") as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            var deletedUser = await context.Users.FindAsync("1");
            Assert.IsNull(deletedUser);
        }

        [TestMethod]
        public async Task Delete_UserDoesNotExist_ReturnsNotFound()
        {
            // Act
            var result = await controller.Delete("NonExistentId");

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}
