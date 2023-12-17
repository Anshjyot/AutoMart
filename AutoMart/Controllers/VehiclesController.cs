using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using AutoMart.Data;
using AutoMart.Models;

namespace AutoMart.Controllers
{
    public class VehiclesController : Controller
    {
        private IWebHostEnvironment _env;
        private readonly ApplicationDbContext db;
        private readonly UserManager<WebUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public VehiclesController(
            ApplicationDbContext context,
            UserManager<WebUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment env
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }

        // Display all products from the database, along with their category:
       
        public IActionResult Index()
        {
            var vehicles = db.Vehicles.Include("Category");

            bool sortedAsc = false;
            bool sortedDesc = false;

            // Sorting
            if (User.IsInRole("Admin"))
            {
                if (Convert.ToString(HttpContext.Request.Query["sort"]) != null)
                {
                    string order = Convert.ToString(HttpContext.Request.Query["sort"]);
                    if (order == "crescator")
                    {
                        sortedAsc = true;
                        vehicles = vehicles.OrderBy(prod => prod.Approved);
                    }
                    else
                    {
                        sortedDesc = true;
                        vehicles = vehicles.OrderByDescending(prod => prod.Approved);
                    }
                }
            }
            else
            {
                if (Convert.ToString(HttpContext.Request.Query["sort"]) != null)
                {
                    string order = Convert.ToString(HttpContext.Request.Query["sort"]);
                    if (order == "crescator")
                    {
                        sortedAsc = true;
                        vehicles = vehicles.OrderBy(prod => prod.Price).ThenBy(prod => prod.Rating);
                    }
                    else
                    {
                        sortedDesc = true;
                        vehicles = vehicles.OrderByDescending(prod => prod.Price).ThenBy(prod => prod.Rating);
                    }
                }
            }

            ViewBag.Vehicles = vehicles;
            ViewBag.Asc = sortedAsc;
            ViewBag.Desc = sortedDesc;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            if (TempData.ContainsKey("messageCart"))
            {
                ViewBag.MessageCart = TempData["messageCart"];
            }

            var search = "";

            // search
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim(); 

                
                List<int> vehicleIds = db.Vehicles.Include("Category").Where
                (
                    at => at.Title.Contains(search)
                    || at.Description.Contains(search)
                    || at.Category.CategoryName.Contains(search)
                ).Select(a => a.VehicleId).ToList();

               
                List<int> VehicleIdsOfCommentsWithSearchString = db.Feedbacks
                .Where
                (
                    c => c.ReviewText.Contains(search)
                ).Select(c => (int)c.VehicleId).ToList();


                List<int> mergedIds = vehicleIds.Union(VehicleIdsOfCommentsWithSearchString).ToList();

                
                vehicles = db.Vehicles.Where(vehicle => mergedIds.Contains(vehicle.VehicleId))
                .Include("Category")
                .Include("User")
                .OrderBy(a => a.Title);
            }

            ViewBag.SearchString = search;

            // PAGINATED DISPLAY, 6 products per page
            int _perPage = 6;

         
            int totalItems = vehicles.Count();

       
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

           
            var offset = 0;

           
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedVehicles = vehicles.Skip(offset).Take(_perPage);

      
            ViewBag.last = Math.Ceiling((float)totalItems / (float)_perPage);

            
            ViewBag.Vehicles = paginatedVehicles;

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Vehicles/Index/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Vehicles/Index/?page";
            }

            return View();
        }

        // Displaying a single product, based on id:
    
        public IActionResult Show(int id)
        {
            Vehicle vehicle = db.Vehicles.Include("Category")
                                         .Include("Feedbacks")
                                         .Include("User")
                                         .Include("Feedbacks.User")
                                         .FirstOrDefault(prod => prod.VehicleId == id);

            if (vehicle == null)
            {
                
                return NotFound();
            }

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            SetAccessRights();

       
            ViewBag.IsUser = User.IsInRole("User");
            ViewBag.IsAdmin = User.IsInRole("Admin");
            ViewBag.CurrentUser = _userManager.GetUserId(User);

            return View(vehicle);
        }

        [Authorize(Roles = "Editor,Admin")]
        public IActionResult New()
        {
            Vehicle vehicle = new Vehicle();

            vehicle.Categ = GetAllVehicleCategory();

            return View(vehicle);
        }

        
        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        public async Task<IActionResult> New(Vehicle vehicle, IFormFile VehicleImage)
        {
            vehicle.Feedbacks = null;
            vehicle.UserId = _userManager.GetUserId(User);
            var sanitizer = new HtmlSanitizer();

            if (User.IsInRole("Admin"))
                vehicle.Approved = true;
            else
                vehicle.Approved = false;

            if (VehicleImage.Length > 0)
            {
               
                var storagePath = Path.Combine(
                    _env.WebRootPath, 
                    "images",
                    VehicleImage.FileName 
                );
              
                var databaseFileName = "/images/" + VehicleImage.FileName;
                // Upload the file to the storage path
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await VehicleImage.CopyToAsync(fileStream);
                }
                vehicle.Photo = databaseFileName;
            }

            if (ModelState.IsValid)
            {
                vehicle.Description = sanitizer.Sanitize(vehicle.Description);

                db.Vehicles.Add(vehicle);
                db.SaveChanges();
                if (User.IsInRole("Admin"))
                    TempData["message"] = "The product has been added";
                else
                    TempData["message"] = "The add request has been sent.";
                return RedirectToAction("Index");
            }
            else
            {
                vehicle.Categ = GetAllVehicleCategory();
                return View(vehicle);
            }
        }

        // Editing an existing product in the database along with its category
    
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Edit(int id)
        {
            Vehicle vehicle = db.Vehicles.Include("Category").Where(prod => prod.VehicleId == id).First();

            vehicle.Categ = GetAllVehicleCategory();

            if (vehicle.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(vehicle);
            }
            else
            {
                TempData["message"] = "You do not have the right to make modifications to a product that does not belong to you";
                return RedirectToAction("Index");
            }
        }
      
        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Vehicle requestVehicle, IFormFile? VehicleImage = null)
        {
            Vehicle vehicle = db.Vehicles.Find(id);
            requestVehicle.Categ = GetAllVehicleCategory();
            var sanitizer = new HtmlSanitizer();

            if (VehicleImage != null)
            {
                if (VehicleImage.Length > 0)
                {
                    // Generate the file storage path
                    var storagePath = Path.Combine(
                        _env.WebRootPath, 
                        "images",
                        VehicleImage.FileName 
                    );
                   
                    var databaseFileName = "/images/" + VehicleImage.FileName;
                    
                    using (var fileStream = new FileStream(storagePath, FileMode.Create))
                    {
                        await VehicleImage.CopyToAsync(fileStream);
                    }
                    vehicle.Photo = databaseFileName;
                }
            }

            if (ModelState.IsValid)
            {
                if (vehicle.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    requestVehicle.Description = sanitizer.Sanitize(requestVehicle.Description);

                    vehicle.Title = requestVehicle.Title;
                    vehicle.Description = requestVehicle.Description;
                    vehicle.Photo = requestVehicle.Photo;
                    vehicle.CategoryId = requestVehicle.CategoryId;
                    vehicle.Price = requestVehicle.Price;
                    TempData["message"] = "The product has been modified!";
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "You do not have the right to make modifications to a product that does not belong to you";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return View(requestVehicle);
            }
        }

        // Approval of the request to add a new product
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Approve(int id)
        {
            Vehicle vehicle = db.Vehicles.Find(id);
            vehicle.Approved = true;
            db.SaveChanges();
            TempData["message"] = "The request has been approved!";
            return RedirectToAction("Index");
        }

        // Deleting a product from the database
        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            Vehicle vehicle = db.Vehicles.Include("Feedbacks").Where(prod => prod.VehicleId == id).First();

            if (vehicle.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Vehicles.Remove(vehicle);
                db.SaveChanges();
                if (User.IsInRole("Admin") && !vehicle.Approved)
                    TempData["message"] = "The request has been rejected!";
                else
                    TempData["message"] = "The product has been deleted!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "You do not have the right to delete a product that does not belong to you";
                return RedirectToAction("Index");
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllVehicleCategory()
        {
            // Generating a SelectListItem list without elements
            var selectList = new List<SelectListItem>();

            // Extract all categories from the database
            var categories = from cat in db.VehicleCategory
                             select cat;

        
            foreach (var category in categories)
            {
            
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryId.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }

            // Return the list of categories
            return selectList;
        }

        private void SetAccessRights()
        {
            ViewBag.DisplayButtons = false;

            if (User.IsInRole("Editor"))
            {
                ViewBag.DisplayButtons = true;
            }

            ViewBag.IsAdmin = User.IsInRole("Admin");
            ViewBag.IsUser = User.IsInRole("User");
            ViewBag.IsEditor = User.IsInRole("Editor");
            ViewBag.CurrentUser = _userManager.GetUserId(User);
        }

    }
}