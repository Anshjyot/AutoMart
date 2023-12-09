using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMart.Data;
using AutoMart.Models;

namespace AutoMart.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<WebUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ReviewController(ApplicationDbContext context, UserManager<WebUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Adding a review associated with a product from the database
        // Implicit HttpGet
        // displays the form in which the review will be entered along with the rating
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult New(int id)
        {
            Review review = new Review();
            review.VehicleId = id;
            return View(review);
        }

        // adding the review to the database:
        [Authorize(Roles = "User,Editor,Admin")]
        [HttpPost]
        public IActionResult New(int id, Review review)
        {
            if (ModelState.IsValid)
            {
                var vehicle = db.Vehicles.Find(review.VehicleId);
                int nrReviews = db.Reviews.Where(p => p.VehicleId == review.VehicleId).Count();

                if (nrReviews != 0)
                {
                    vehicle.Rating = ((int)(((vehicle.Rating * nrReviews) + review.Rating) / (nrReviews + 1) * 10)) / (float)10;
                }
                else
                {
                    vehicle.Rating = review.Rating;
                }

                review.Date = DateTime.Now;
                review.UserId = _userManager.GetUserId(User);
                db.Reviews.Add(review);
                db.SaveChanges();
                TempData["message"] = "The review was successfully added!";
                return Redirect("/Vehicles/Show/" + id);
            }
            else
            {
                return View(review);
            }
        }

        // editing a review associated with a product from the database
        // Implicit HttpGet
        // displays the form in which the review will be entered along with the rating
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id)
        {
            Review review = db.Reviews.Find(id);

            if (review.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(review);
            }
            else
            {
                TempData["message"] = "You do not have the right to modify the comment!";
                return Redirect("/Vehicles/Show/" + review.VehicleId);
            }

        }

        // adding the review to the database:
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id, Review requestReview)
        {
            Review review = db.Reviews.Find(id);

            if (ModelState.IsValid)
            {
                if (review.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    var vehicle = db.Vehicles.Find(review.VehicleId);
                    int nrReviews = db.Reviews.Where(p => p.VehicleId == review.VehicleId).Count();

                    vehicle.Rating = ((vehicle.Rating * nrReviews) + review.Rating) / (nrReviews + 1);

                    review.Text = requestReview.Text;
                    review.Rating = requestReview.Rating;
                    review.Date = DateTime.Now;
                    TempData["message"] = "The review has been modified!";
                    db.SaveChanges();

                    return Redirect("/Vehicles/Show/" + review.VehicleId);
                }
                else
                {
                    TempData["message"] = "You do not have the right to modify the comment!";
                    return RedirectToAction("/Vehicles/Show/" + review.VehicleId);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                ViewBag.Errors = errors;
                return View(requestReview);
            }
        }

        // deleting a review associated with a product from the database
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Delete(int id)
        {
            Review review = db.Reviews.Find(id);

            if (review.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Reviews.Remove(review);
                db.SaveChanges();
                return Redirect("/Vehicles/Show/" + review.VehicleId);
            }
            else
            {
                TempData["message"] = "You do not have the right to delete the comment!";
                return RedirectToAction("/Vehicles/Show/" + review.VehicleId);
            }
        }
    }
}
