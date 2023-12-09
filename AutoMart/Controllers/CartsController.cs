using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using AutoMart.Data;
using AutoMart.Models;
using AutoMart.Data.Migrations;

namespace AutoMart.Controllers
{
    [Authorize]
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<WebUser> _webUserManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<CartsController> _logger;

        public CartsController(
            ApplicationDbContext context,
            UserManager<WebUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<CartsController> logger
            )
        {
            _dbContext = context;

            _webUserManager = userManager;

            _roleManager = roleManager;

            _logger = logger;
        }

        // Vehicle saved in shopping cart 
        

        [Authorize(Roles = "User")]
        public IActionResult Show()
        {
            var idUser = _webUserManager.GetUserId(User);
            _logger.LogInformation($"User Id: {idUser}");
            var cart = _dbContext.Users.Where(x => x.Id == idUser).Include("Carts").Include("Carts.Vehicle").FirstOrDefault();

            ViewBag.VehiclesCart = cart;
            

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

     
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public IActionResult AddToCart(Cart cartItem)
        {
            _dbContext.Carts.Add(cartItem);
            _dbContext.SaveChanges();
            TempData["cartItemMessage"] = "Product added to cart!";
            return Redirect($"/Vehicles/Show/{cartItem.VehicleId}");
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public IActionResult RemoveFromCart(Cart cartDetails)
        {
            var itemToRemove = _dbContext.Carts.First(c => c.Id == cartDetails.Id
                                                           && c.VehicleId == cartDetails.VehicleId
                                                           && c.UserId == cartDetails.UserId);
            _logger.LogInformation($"Removing cart item - ID: {cartDetails.Id}, VehicleID: {cartDetails.VehicleId}, UserID: {cartDetails.UserId}");

            _dbContext.Carts.Remove(itemToRemove);
            _dbContext.SaveChanges();
            TempData["cartRemovalMessage"] = "Product removed from cart";
            return RedirectToAction("Show");
        }
    }
}



