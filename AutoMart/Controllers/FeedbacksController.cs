using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMart.Data;
using AutoMart.Models;
using System;
using System.Linq;

namespace AutoMart.Controllers
{
    public class FeedbacksController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<WebUser> _webUserManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<MainController> _logger;

        public FeedbacksController(ApplicationDbContext context, UserManager<WebUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _dbContext = context;
            _webUserManager = userManager;
            _roleManager = roleManager;
        }

        // Display form to create a new feedback entry
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult New(int vehicleId)
        {
            var feedback = new Feedback { VehicleId = vehicleId };
            return View("New", feedback);
        }

        // Process new feedback submission
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult New(int vehicleId, Feedback feedbackEntry)
        {
            if (ModelState.IsValid)
            {
                var vehicle =  _dbContext.Vehicles.Find(feedbackEntry.VehicleId);
                int totalFeedbacks =  _dbContext.Feedbacks.Count(r => r.VehicleId == feedbackEntry.VehicleId);

                vehicle.Rating = totalFeedbacks != 0
                    ? (vehicle.Rating * totalFeedbacks + feedbackEntry.Rating) / (totalFeedbacks + 1)
                    : feedbackEntry.Rating;

                feedbackEntry.DateSubmitted = DateTime.UtcNow;
                feedbackEntry.UserId = _webUserManager.GetUserId(User);
                 _dbContext.Feedbacks.Add(feedbackEntry);
                 _dbContext.SaveChanges();
                TempData["FeedbackMessage"] = "Your feedback has been added!";
                return RedirectToAction("Details", "Vehicles", new { id = vehicleId });
            }

            return View("New", feedbackEntry);
        }

        // Display form to edit an existing feedback entry
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int feedbackId)
        {
            var feedback =  _dbContext.Feedbacks.Find(feedbackId);
            if (feedback.UserId == _webUserManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View("EditFeedback", feedback);
            }

            TempData["AccessDenied"] = "You're not authorized to edit this feedback.";
            return RedirectToAction("Details", "Vehicles", new { id = feedback.VehicleId });
        }

        // Process feedback update submission
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int feedbackId, Feedback feedbackUpdate)
        {
            var existingFeedback =  _dbContext.Feedbacks.Find(feedbackId);

            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return View("EditFeedback", feedbackUpdate);
            }

            if (existingFeedback.UserId == _webUserManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                existingFeedback.ReviewText = feedbackUpdate.ReviewText;
                existingFeedback.Rating = feedbackUpdate.Rating;
                existingFeedback.DateSubmitted = DateTime.UtcNow;
                 _dbContext.SaveChanges();
                TempData["FeedbackMessage"] = "Your feedback has been updated!";
                return RedirectToAction("Details", "Vehicles", new { id = existingFeedback.VehicleId });
            }

            TempData["AccessDenied"] = "You're not authorized to edit this feedback.";
            return RedirectToAction("Details", "Vehicles", new { id = existingFeedback.VehicleId });
        }

        // Process feedback deletion
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Delete(int feedbackId)
        {
            var feedback =  _dbContext.Feedbacks.Find(feedbackId);
            if (feedback.UserId == _webUserManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                 _dbContext.Feedbacks.Remove(feedback);
                 _dbContext.SaveChanges();
                TempData["FeedbackMessage"] = "The feedback has been deleted.";
                return RedirectToAction("Details", "Vehicles", new { id = feedback.VehicleId });
            }

            TempData["AccessDenied"] = "You're not authorized to delete this feedback.";
            return RedirectToAction("Details", "Vehicles", new { id = feedback.VehicleId });
        }

        public Task New()
        {
            throw new NotImplementedException();
        }
    }
}
    