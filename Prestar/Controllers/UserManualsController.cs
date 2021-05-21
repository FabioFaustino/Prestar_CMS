using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prestar.Data;
using Prestar.Models;

namespace Prestar.Controllers
{
    /// <summary>
    /// User Manuals Controller
    /// </summary>
    /// <see cref="UserManual"/>
    /// <seealso cref="Section"/>
    public class UserManualsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Constructor of the UserManualsController class that receives two parameters 
        /// and initializes them.
        /// </summary>
        /// <param name="userManager">
        /// Provides the APIs for managing user in a persistence store.
        /// <see cref="UserManager{TUser}"/>
        /// </param>
        /// <param name="context">
        /// Application database context
        /// <see cref="ApplicationDbContext"/>
        /// </param>
        public UserManualsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// This method allows viewing the user manual with the id passed by parameter as well as its respective sections
        /// </summary>
        /// <param name="id">User manual identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                if (_context.UserManual.Count() > 0)
                {
                    id = _context.UserManual.First().UserManualID;
                }
                else
                {
                    ViewBag.UserManuals = GetUserManuals();
                    return View(null);
                }
            }
            var userManual = await _context.UserManual.Include(u => u.User).FirstOrDefaultAsync(m => m.UserManualID == id);
            if (userManual == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            if(GetUserManuals() != null)
                ViewBag.UserManuals = GetUserManuals();
            
            ViewBag.Sections = _context.Section.Where(s => s.UserManualID == id).ToList();

            //ViewBag for bell icon on the nav menu
            if (User.Identity.IsAuthenticated)
            {
                var userLoggedIn = await _userManager.GetUserAsync(User);
                ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();
            }
            return View(userManual);
        }

        /// <summary>
        /// This method shows the view to create a user manual.
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create()
        {
            List<string> roles = new List<string>();
            roles.Add("Administrador");
            roles.Add("Moderador");
            roles.Add("Prestador");
            roles.Add("Cliente");
            await _context.UserManual.Select(u => u.Role).ForEachAsync(u => {
                if (roles.Contains(u))
                {
                    roles.Remove(u);
                }
            });
            ViewBag.Roles = roles;
            return View();
        }

        /// <summary>
        /// This method allows you to create a new user manual.
        /// </summary>
        /// <param name="userManual"></param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create([Bind("UserManualID,Role,LastUpdate,LastUpdateUserID")] UserManual userManual)
        {
            var user = await _userManager.GetUserAsync(User);
            userManual.LastUpdate = DateTime.Now;
            userManual.LastUpdateUserID = user.Id;

            if (ModelState.IsValid)
            {
                _context.Add(userManual);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details));
            }
            return View(userManual);
        }

        /// <summary>
        /// This method allows viewing the view that gives the possibility to edit the user manual.
        /// </summary>
        /// <param name="id">User manual identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            Debug.Write("\n\n--------\n\n");
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            var userManual = await _context.UserManual.FindAsync(id);
            if (userManual == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            ViewBag.Sections = _context.Section.Where(s => s.UserManualID == id).ToList();
            return View(userManual);
        }

        /// <summary>
        /// This method allows to eliminate user manual passed by parameter if there are no sections associated with it.
        /// </summary>
        /// <param name="id">User manual identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userManual = await _context.UserManual.FindAsync(id);

            if (userManual == null)
                return View("/Views/Shared/NotFound.cshtml");

            _context.UserManual.Remove(userManual);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), id);
        }

        /// <summary>
        /// This method allows to know if there is a user manual with the id passed by parameter
        /// </summary>
        /// <param name="id">User manual identification</param>
        /// <returns></returns>
        private bool UserManualExists(int id)
        {
            return _context.UserManual.Any(e => e.UserManualID == id);
        }

        /// <summary>
        /// This method allows you to return to the dictionary with the role and associated user manual.
        /// </summary>
        /// <returns>Dictionary<string, UserManual></returns>
        private Dictionary<string, UserManual> GetUserManuals()
        {
            Dictionary<string, UserManual> dict = new Dictionary<string, UserManual>();
            _context.UserManual.ToList().ForEach(u => {
                dict.Add(u.Role, u);
            });
            return dict;
        }

        /// <summary>
        /// This method allows to obtain the id of a user manual with the specific role.
        /// </summary>
        /// <param name="role">User role</param>
        /// <returns>int</returns>
        [HttpGet]
        public int GetIdByRole(string role)
        {
            return _context.UserManual.Where(m => m.Role == role).OrderBy(r => r.UserManualID).Last().UserManualID;
        }
    }
}