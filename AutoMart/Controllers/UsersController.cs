using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using AutoMart.Data;
using AutoMart.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AutoMart.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<WebUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            ApplicationDbContext context,
            UserManager<WebUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var userList = await _context.Users.OrderBy(u => u.UserName).ToListAsync();
            ViewBag.UsersList = userList;
            return View();
        }

        public async Task<IActionResult> Show(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            ViewBag.UserRoles = await _userManager.GetRolesAsync(user);
            return View(user);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            ViewBag.RolesForDropdown = await RoleSelectionItems();
            ViewBag.CurrentUserRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, WebUser updatedUserData, [FromForm] string newRole)
        {
            var userToUpdate = await _context.Users.FindAsync(id);
            if (userToUpdate == null) return NotFound();

            if (ModelState.IsValid)
            {
                userToUpdate.UserName = updatedUserData.UserName;
                userToUpdate.Email = updatedUserData.Email;
                userToUpdate.FirstName = updatedUserData.FirstName;
                userToUpdate.LastName = updatedUserData.LastName;
                userToUpdate.PhoneNumber = updatedUserData.PhoneNumber;

                await ReassignRoles(userToUpdate, newRole);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.RolesForDropdown = await RoleSelectionItems();
            return View(userToUpdate);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _context.Users
                                     .Include(u => u.Vehicles)
                                     .Include(u => u.Feedbacks)
                                     .Include(u => u.Carts)
                                     .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            _context.Vehicles.RemoveRange(user.Vehicles);
            _context.Feedbacks.RemoveRange(user.Feedbacks);
            _context.Carts.RemoveRange(user.Carts);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> RoleSelectionItems()
        {
            return await _context.Roles.Select(r => new SelectListItem
            {
                Value = r.Id,
                Text = r.Name
            }).ToListAsync();
        }

        private async Task ReassignRoles(WebUser user, string newRoleId)
        {
            var roles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, roles);

            var newRole = await _roleManager.FindByIdAsync(newRoleId);
            if (newRole != null)
            {
                await _userManager.AddToRoleAsync(user, newRole.Name);
            }
        }
    }
}
