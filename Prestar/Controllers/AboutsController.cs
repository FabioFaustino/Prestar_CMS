using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prestar.Data;
using Prestar.Models;

namespace Prestar.Controllers
{
    /// <summary>
    /// About Sections Controller
    /// </summary>
    /// <see cref="About"/>
    public class AboutsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Constructor of the AboutsController class that receives two parameters 
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
        public AboutsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Allows you to consult the page About, displaying all sections of it
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: Abouts
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
           return View(await _context.About.Include(a => a.User).ToListAsync());
           
        }

        /// <summary>
        /// Allows to add a new section to About page
        /// </summary>
        /// <returns>Returns an IActionResult</returns>
        /// <seealso cref="IActionResult"/>
        // GET: Abouts/Create
        [Authorize(Roles="Administrador")]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Allows to add a new section to About page
        /// </summary>
        /// <param name="about">About instance</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: Abouts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create([Bind("AboutID,Title,Content,Illustration,AboutLastUpdate,LastUpdateUserID")] About about)
        {
            var user = await _userManager.GetUserAsync(User);
            about.AboutLastUpdate = DateTime.Now;
            about.LastUpdateUserID = user.Id;
            about.User = user;

            try
            {
                if (Request.Form.Files.Count > 0)
                {
                    IFormFile file = Request.Form.Files.FirstOrDefault();
                    using var dataStream = new MemoryStream();
                    await file.CopyToAsync(dataStream);
                    about.Illustration = dataStream.ToArray();
                }
            }
            catch (InvalidOperationException) { }

            _context.Add(about);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Allows to edit a section of the About page
        /// </summary>
        /// <param name="id"> About section identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: Abouts/Edit/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var about = await _context.About.FindAsync(id);
            if (about == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            return View(about);
        }

        /// <summary>
        /// Allows to edit a section of the About page
        /// </summary>
        /// <param name="id">About section identification</param>
        /// <param name="about">About instance</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: Abouts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id, [Bind("AboutID,Title,Content,Illustration,AboutLastUpdate,LastUpdateUserID")] About about)
        {
            if (id != about.AboutID)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var user = await _userManager.GetUserAsync(User);
            about.AboutLastUpdate = DateTime.Now;
            about.LastUpdateUserID = user.Id;
            about.User = user;

            try
            {
                if (Request.Form.Files.Count > 0)
                {
                    IFormFile file = Request.Form.Files.FirstOrDefault();
                    using var dataStream = new MemoryStream();
                    await file.CopyToAsync(dataStream);
                    about.Illustration = dataStream.ToArray();
                }
            }
            catch (InvalidOperationException) { }

            if (about.Illustration == null)
            {
                byte[] illustration = _context.Formation.Where(n => n.FormationID == about.AboutID).Select(n => n.Illustration).FirstOrDefault();
                about.Illustration = illustration;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(about);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AboutExists(about.AboutID))
                    {
                        return View("/Views/Shared/NotFound.cshtml");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(about);
        }

        /// <summary>
        /// Allows to remove a section from About page
        /// </summary>
        /// <param name="id">About section identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: Abouts/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var about = await _context.About.Include(a=>a.User).FirstOrDefaultAsync(m => m.AboutID == id);
            if (about == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            return View(about);
        }

        /// <summary>
        /// Allows to remove a section from About page
        /// </summary>
        /// <param name="id">About section identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: Abouts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var about = await _context.About.FindAsync(id);
            if (about == null)
                return View("/Views/Shared/NotFound.cshtml");

            _context.About.Remove(about);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Checks if an about section exists
        /// </summary>
        /// <param name="id">About section identification</param>
        /// <returns>bool</returns>
        private bool AboutExists(int id)
        {
            return _context.About.Any(e => e.AboutID == id);
        }
    }
}
