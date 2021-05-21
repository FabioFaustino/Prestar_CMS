using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prestar.Data;
using Prestar.Models;

namespace Prestar.Controllers
{
    /// <summary>
    /// Sections Controllers for User Manuals
    /// </summary>
    /// <see cref="Section"/>
    /// <seealso cref="UserManual"/>
    /// <seealso cref="UserManualsController"/>
    public class SectionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Constructor of the SectionsController class that receives two parameters 
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
        public SectionsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// This method shows the view to create a section to the user manual with the id passed .
        /// </summary>
        /// <param name="id">User Manual identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Moderador,Administrador")]
        public async Task<IActionResult> CreateAsync(int id)
        {
            ViewBag.UserManualID = id;
            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View();
        }

        /// <summary>
        /// This method allows to create a new user manual.
        /// </summary>
        /// <param name="section">New section object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Moderador,Administrador")]
        public async Task<IActionResult> Create([Bind("SectionID,Title,Content,UserManualID,Illustration")] Section section)
        {
            try
            {
                if (Request.Form.Files.Count > 0)
                {
                    IFormFile file = Request.Form.Files.FirstOrDefault();
                    using var dataStream = new MemoryStream();
                    await file.CopyToAsync(dataStream);
                    section.Illustration = dataStream.ToArray();
                }
            }
            catch (InvalidOperationException) { }

            var user = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {
                _context.Add(section);
                await _context.SaveChangesAsync();

                //Atualizar Manual De Utilizador
                var userManual = _context.UserManual.Find(section.UserManualID);
                userManual.LastUpdate = DateTime.Now;
                userManual.LastUpdateUserID = user.Id;
                _context.Update(userManual);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "UserManuals", section.UserManualID);
            }
            return View(section);
        }

        /// <summary>
        /// This method allows viewing the view that gives the possibility to edit the section with the id passed by parameter.
        /// </summary>
        /// <param name="id">Section identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Moderador,Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            var section = await _context.Section.FindAsync(id);
            if (section == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(section);
        }

        /// <summary>
        /// This method allows to update the sections with the id sent by parameter, through the new section sent also by parameter.
        /// </summary>
        /// <param name="id">Section identification</param>
        /// <param name="section">Section updated object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Moderador,Administrador")]
        public async Task<IActionResult> Edit(int id, [Bind("SectionID,Title,Content,UserManualID,Illustration")] Section section)
        {
            if (id != section.SectionID)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var user = await _userManager.GetUserAsync(User);
            try
            {
                if (Request.Form.Files.Count > 0)
                {
                    IFormFile file = Request.Form.Files.FirstOrDefault();
                    using var dataStream = new MemoryStream();
                    await file.CopyToAsync(dataStream);
                    section.Illustration = dataStream.ToArray();
                }
            }
            catch (InvalidOperationException) { }
            if (section.Illustration == null)
            {
                byte[] illustration = _context.Section.Where(n => n.SectionID == section.SectionID).Select(n => n.Illustration).FirstOrDefault();
                section.Illustration = illustration;
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(section);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SectionExists(section.SectionID))
                    {
                        return View("/Views/Shared/NotFound.cshtml");
                    }
                    else
                    {
                        throw;
                    }
                }
                //Atualizar Manual De Utilizador
                var userManual = _context.UserManual.Find(section.UserManualID);
                userManual.LastUpdate = DateTime.Now;
                userManual.LastUpdateUserID = user.Id;
                _context.Update(userManual);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "UserManuals", section.UserManualID);
            }
            return View(section);
        }

        /// <summary>
        /// This method allows to eliminate a section with id passed by parameter.
        /// </summary>
        /// <param name="id">Section identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Moderador,Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var section = await _context.Section.FindAsync(id);
            if (section == null)
                return View("/Views/Shared/NotFound.cshtml");

            _context.Section.Remove(section);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "UserManuals", section.UserManualID);
        }

        /// <summary>
        /// This method allows to know if there is a section with the id passed by parameter
        /// </summary>
        /// <param name="id">Section identification</param>
        /// <returns>bool</returns>
        private bool SectionExists(int id)
        {
            return _context.Section.Any(e => e.SectionID == id);
        }
    }
}
