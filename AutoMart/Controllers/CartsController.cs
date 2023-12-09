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
        private readonly ApplicationDbContext db;
        private readonly UserManager<WebUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<CartsController> _logger;

        public CartsController(
            ApplicationDbContext context,
            UserManager<WebUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<CartsController> logger
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;

            _logger = logger;
        }

        // Vehicle saved in shopping cart 
        

        [Authorize(Roles = "User")]
        public IActionResult Show()
        {
            var idUser = _userManager.GetUserId(User);
            _logger.LogInformation($"User Id: {idUser}");
            var cart = db.Users.Where(x => x.Id == idUser).Include("Carts").Include("Carts.Vehicle").FirstOrDefault();

            ViewBag.VehiclesCart = cart;
            

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

     
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public ActionResult New(Cart cart)
        {
            db.Carts.Add(cart);
            db.SaveChanges();
            TempData["messageCart"] = "The product has been added to the cart!";
            return Redirect("/Vehicles/Show/" + cart.VehicleId);
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public ActionResult Delete(Cart aux)
        {
            var cart = db.Carts.Where(a => a.Id == aux.Id && a.VehicleId == aux.VehicleId && a.UserId == aux.UserId).First();
            _logger.LogInformation($"Deleting cart with Id: {aux.Id}, VehicleId: {aux.VehicleId}, UserId: {aux.UserId}");

            db.Carts.Remove(cart);
            db.SaveChanges();
            TempData["message"] = "The product has been removed from the cart";
            return RedirectToAction("Show");
        }
    }

}

