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

        //afisarea tuturor produselor din baza de date, impreuna cu categoria din care fac parte:
        //HttpGet implicit
        //[Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Index()
        {
            var vehicles = db.Vehicles.Include("Category");

            bool sortedAsc = false;
            bool sortedDesc = false;

            //sortare

           if(User.IsInRole("Admin"))
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

            // MOTOR DE CAUTARE

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim(); // eliminam spatiile libere 

                // Cautare in articol (Title si Content)

                List<int> vehicleIds = db.Vehicles.Include("Category").Where
                                        (
                                         at => at.Title.Contains(search)
                                         || at.Description.Contains(search)
                                         || at.Category.CategoryName.Contains(search)
                                        ).Select(a => a.VehicleId).ToList();

                // Cautare in comentarii (Content)
                List<int> VehicleIdsOfCommentsWithSearchString = db.Reviews
                                        .Where
                                        (
                                         c => c.Text.Contains(search)
                                        ).Select(c => (int)c.VehicleId).ToList();

                // Se formeaza o singura lista formata din toate id-urile selectate anterior
                List<int> mergedIds = vehicleIds.Union(VehicleIdsOfCommentsWithSearchString).ToList();


                // Lista articolelor care contin cuvantul cautat
                // fie in articol -> Title si Content
                // fie in comentarii -> Content
                vehicles = db.Vehicles.Where(vehicle => mergedIds.Contains(vehicle.VehicleId))
                                      .Include("Category")
                                      .Include("User")
                                      .OrderBy(a => a.Title);

            }

            ViewBag.SearchString = search;

            //AFISARE PAGINATA
            // Alegem sa afisam 6 produse pe pagina
            int _perPage = 6;

            // Fiind un numar variabil de articole, verificam de fiecare data utilizand 
            // metoda Count()

            int totalItems = vehicles.Count();


            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page din ruta
            // /Vehicles/Index?page=valoare

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // Pentru prima pagina offsetul o sa fie zero
            // Pentru pagina 2 o sa fie 3 
            // Asadar offsetul este egal cu numarul de articole care au fost deja afisate pe paginile anterioare
            var offset = 0;

            // Se calculeaza offsetul in functie de numarul paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            // Se preiau articolele corespunzatoare pentru fiecare pagina la care ne aflam 
            // in functie de offset
            var paginatedVehicles = vehicles.Skip(offset).Take(_perPage);


            // Preluam numarul ultimei pagini

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            // Trimitem articolele cu ajutorul unui ViewBag catre View-ul corespunzator
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

        //afisarea unui singur produs, in functie de id:
        //HttpGet implicit
        //[Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show(int id)
        {
            Vehicle vehicle = db.Vehicles.Include("Category")
                                         .Include("Reviews")
                                         .Include("User")
                                         .Include("Reviews.User")
                                         .FirstOrDefault(prod => prod.VehicleId == id);

            if (vehicle == null)
            {
                // Handle the case where the vehicle is not found
                return NotFound();
            }

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            SetAccessRights();

            // Set ViewBag properties
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

        //adaugarea produsului in baza de date:
        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        public async Task<IActionResult> New(Vehicle vehicle, IFormFile VehicleImage)
        {
            vehicle.Reviews = null;
            vehicle.UserId = _userManager.GetUserId(User);
            var sanitizer = new HtmlSanitizer();

            if (User.IsInRole("Admin"))
                vehicle.Approved = true;
            else
                vehicle.Approved = false;

            if (VehicleImage.Length > 0)
            {
                // Generam calea de stocare a fisierului
                var storagePath = Path.Combine(
                _env.WebRootPath, // Luam calea folderului wwwroot
                "images", // Adaugam calea folderului images
                VehicleImage.FileName // Numele fisierului
                );
                // General calea de afisare a fisierului care va fi stocata in  baza de date
                var databaseFileName = "/images/" + VehicleImage.FileName;
                // Uploadam fisierul la calea de storage
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

        // se editeaza un produs existent in baza de date impreuna cu categoria din care face parte
        // categoria se selecteaza dintr-un dropdown
        // HttpGet implicit
        // se afiseaza formularul impreuna cu datele aferente articolului din baza de date
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
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui produs care nu va apartine";
                return RedirectToAction("Index");
            }
        }

        //se adauga articolul modificat in baza de date
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
                    // Generam calea de stocare a fisierului
                    var storagePath = Path.Combine(
                    _env.WebRootPath, // Luam calea folderului wwwroot
                    "images", // Adaugam calea folderului images
                    VehicleImage.FileName // Numele fisierului
                    );
                    // General calea de afisare a fisierului care va fi stocata in  baza de date
                    var databaseFileName = "/images/" + VehicleImage.FileName;
                    // Uploadam fisierul la calea de storage
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
                    TempData["message"] = "Produsul a fost modificat!";
                    db.SaveChanges();


                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui produs care nu va apartine";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return View(requestVehicle);
            }
        }

        //aprobarea cererii de adaugare a unui nou produs
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

        // se sterge un produs din baza de date 
        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            Vehicle vehicle = db.Vehicles.Include("Reviews").Where(prod => prod.VehicleId == id).First();

            if (vehicle.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Vehicles.Remove(vehicle);
                db.SaveChanges();
                if (User.IsInRole("Admin") && !vehicle.Approved)
                    TempData["message"] = "Cererea a fost respinsa!";
                else
                    TempData["message"] = "Produsul a fost sters!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un produs care nu va apartine!";
                return RedirectToAction("Index");
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllVehicleCategory()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.VehicleCategory
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryId.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }
            /* Sau se poate implementa astfel: 
             * 
            foreach (var category in categories)
            {
                var listItem = new SelectListItem();
                listItem.Value = category.Id.ToString();
                listItem.Text = category.CategoryName.ToString();

                selectList.Add(listItem);
             }*/


            // returnam lista de categorii
            return selectList;
        }

        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Editor"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.EsteUser = User.IsInRole("User");

            ViewBag.EsteEditor = User.IsInRole("Editor");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }
    }
}