using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMart.Data;
using AutoMart.Models;

namespace AutoMart.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VehicleCategoryController : Controller
    {
        private readonly ApplicationDbContext db;

        public VehicleCategoryController(ApplicationDbContext context)
        {
            db = context;
        }

        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var categories = db.VehicleCategory.OrderBy(c => c.CategoryName);
            ViewBag.VehicleCategory = categories;
            return View();
        }

        public ActionResult Show(int id)
        {
            var category = db.VehicleCategory.Find(id);
            return View(category);
        }

        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public ActionResult New(Category category)
        {
            if (ModelState.IsValid)
            {
                db.VehicleCategory.Add(category);
                db.SaveChanges();
                TempData["message"] = "The category has been added.";
                return RedirectToAction("Index");
            }
            else
            {
                return View(category);
            }
        }

        public ActionResult Edit(int id)
        {
            var category = db.VehicleCategory.Find(id);
            return View(category);
        }

        [HttpPost]
        public ActionResult Edit(int id, Category requestCategory)
        {
            var category = db.VehicleCategory.Find(id);

            if (ModelState.IsValid)
            {
                category.CategoryName = requestCategory.CategoryName;
                db.SaveChanges();
                TempData["message"] = "The category has been modified!";
                return RedirectToAction("Index");
            }
            else
            {
                return View(requestCategory);
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var category = db.VehicleCategory.Find(id);
            db.VehicleCategory.Remove(category);
            TempData["message"] = "The category has been deleted.";
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
