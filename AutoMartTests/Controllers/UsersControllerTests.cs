using AutoMart.Controllers;
using AutoMart.Data;
using AutoMart.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoMart.Controllers.Tests
{
    [TestClass()]
    public class UsersControllerTests
    {
        [TestMethod()]
        public void DeleteTest()
        {
            Assert.Fail();
        }
    }
}

namespace AutoMart.Tests.Controllers
{
    [TestClass]
public class UsersControllerTests
{
    private ApplicationDbContext context; // Use the real context
    private UsersController controller;

    [TestInitialize]
    public void SetUp()
    {
        // Set up the in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        context = new ApplicationDbContext(options); // Initialize the real context
        controller = new UsersController(context, null, null); // Inject the real context
    }

    [TestMethod]
    public async Task Show_UserExists_ReturnsViewWithUser()
    {
        // Arrange
        var user = new WebUser { Id = "1", UserName = "TestUser" };
        await context.Users.AddAsync(user); // Use the real context
        await context.SaveChangesAsync();

        // Act
        var result = await controller.Show("1") as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        var model = result.Model as WebUser;
        Assert.AreEqual("TestUser", model.UserName);
    }

    [TestMethod]
    public async Task Show_UserDoesNotExist_ReturnsNotFound()
    {
        // Act
        var result = await controller.Show("NonExistentId");

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }
}
}


