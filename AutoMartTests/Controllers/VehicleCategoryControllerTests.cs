using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoMart.Controllers;
using AutoMart.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Http;
using AutoMart.Data;

namespace AutoMart.Tests.Controllers
{
    [TestClass]
    public class VehicleCategoryControllerTests
    {
        private VehicleCategoryController controller;
        private Mock<ApplicationDbContext> mockContext;

        [TestInitialize]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            var context = new ApplicationDbContext(options);
            controller = new VehicleCategoryController(context);

            // Mock the HttpContext to simulate ModelState
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [TestMethod]
        public void NewCategory_ValidCategory_RedirectsToIndex()
        {
            // Arrange
            var category = new Category { CategoryName = "Test Category" };
            controller.ModelState.Clear();

            // Act
            var result = controller.New(category) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }

        [TestMethod]
        public void NewCategory_InvalidCategory_ReturnsView()
        {
            // Arrange
            var category = new Category { CategoryName = "" }; // Invalid category
            controller.ModelState.AddModelError("CategoryName", "Required");

            // Act
            var result = controller.New(category) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(controller.ModelState.IsValid);
        }
    }
}
