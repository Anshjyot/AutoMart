using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoMart.Controllers;
using AutoMart.Models;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Principal;
using System.Security.Claims;
using AutoMart.Data;
using Microsoft.EntityFrameworkCore;


namespace AutoMart.Controllers.Tests
{
    [TestClass]
    public class CartsControllerTests
    {
        private Mock<ApplicationDbContext> mockContext;
        private Mock<UserManager<WebUser>> mockUserManager;
        private CartsController controller;

        [TestInitialize]
        public void SetUp()
        {
           
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase") 
                .Options;

     
            var context = new ApplicationDbContext(options);

            // Mock the UserManager
            var userStoreMock = new Mock<IUserStore<WebUser>>();
            mockUserManager = new Mock<UserManager<WebUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);


            controller = new CartsController(context, mockUserManager.Object, null, null);

            // Mock User Identity
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, "userId"),
            }));

          
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }


        [TestMethod]
        public void AddToCartTest()
        {
            // Arrange
            var cartItem = new Cart { VehicleId = 1, UserId = "userId" };

            // Act
            var result = controller.AddToCart(cartItem);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectResult));
            var redirectResult = result as RedirectResult;
            Assert.AreEqual($"/Vehicles/Show/{cartItem.VehicleId}", redirectResult.Url);
        }
    }
}
