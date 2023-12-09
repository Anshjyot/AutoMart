using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using AutoMart.Data;
using AutoMart.Models;

namespace AutoMart.Controllers
{
    public class MainController : Controller
    {

        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<WebUser> _webUserManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<MainController> _logger;

        public MainController(
            ApplicationDbContext dbContext,
            UserManager<WebUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<MainController> logger
            )
        {
            _dbContext = dbContext;
            _webUserManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Vehicles");
            }

            ViewBag.Vehicles = _dbContext.Vehicles.OrderByDescending(v => v.Rating).Take(4).ToList();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult AppError()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
