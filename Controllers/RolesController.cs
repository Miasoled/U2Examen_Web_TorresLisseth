using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NorthwindApp.Models;

namespace NorthwindApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // GET: Roles
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        // GET: Roles/AssignRole
        public IActionResult AssignRole()
        {
            ViewBag.Users = _userManager.Users.ToList();
            ViewBag.Roles = _roleManager.Roles.ToList();
            return View();
        }

        // POST: Roles/AssignRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError("", "Usuario no encontrado.");
                ViewBag.Users = _userManager.Users.ToList();
                ViewBag.Roles = _roleManager.Roles.ToList();
                return View();
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                ModelState.AddModelError("", "El rol no existe.");
                ViewBag.Users = _userManager.Users.ToList();
                ViewBag.Roles = _roleManager.Roles.ToList();
                return View();
            }

            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                await _userManager.AddToRoleAsync(user, roleName);
                TempData["Success"] = $"Rol '{roleName}' asignado a '{user.Email}' correctamente.";
            }
            else
            {
                TempData["Info"] = $"El usuario '{user.Email}' ya tiene el rol '{roleName}'.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Roles/UserRoles
        public async Task<IActionResult> UserRoles()
        {
            var users = _userManager.Users.ToList();
            var model = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    Roles = roles.ToList()
                });
            }

            return View(model);
        }
    }
}
