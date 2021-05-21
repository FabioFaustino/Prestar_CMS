using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prestar.Data;
using Prestar.Models;
using Prestar.Services;

namespace Prestar.Controllers
{
    /// <summary>
    /// This controller allows to add the necessary actions related to the proper functioning of the Terms and Conditions page.
    /// </summary>
    /// <see cref="TermsAndConditionsSection"/>
    public class TermsAndConditionsSectionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Constructor of the TermsAndConditionsSectionsController class that receives two parameters 
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
        public TermsAndConditionsSectionsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Allows consultation of the Terms and Conditions
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: TermsAndConditionsSections
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? pageNumber)
        {
            //ViewBag for bell icon on the nav menu
            if (User.Identity.IsAuthenticated)
            {
                var userLoggedIn = await _userManager.GetUserAsync(User);
                ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();
            }

            int pageSize = 6;
            var applicationDbContext = await _context.TermsAndConditionsSection.Include(t => t.User).ToListAsync();
            return View(await PaginatedList<TermsAndConditionsSection>.CreateAsync(applicationDbContext, pageNumber ?? 1, pageSize));
        }

        /// <summary>
        /// Allows to create a new section for the terms and conditions
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: TermsAndConditionsSections/Create
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Allows to create a new section for the terms and conditions
        /// </summary>
        /// <param name="termsAndConditionsSection">TermsAndConditionsSection object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: TermsAndConditionsSections/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create([Bind("TermsAndConditionsSectionID,Title,Content,LastUpdate,LastUpdateUserID")] TermsAndConditionsSection termsAndConditionsSection)
        {
            var user = await _userManager.GetUserAsync(User);
            termsAndConditionsSection.LastUpdate = DateTime.Now;
            termsAndConditionsSection.LastUpdateUserID = user.Id;
            termsAndConditionsSection.User = user;

            if (ModelState.IsValid)
            {
                _context.Add(termsAndConditionsSection);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(termsAndConditionsSection);
        }

        /// <summary>
        /// Allows to edit a section of terms and conditions
        /// </summary>
        /// <param name="id">TermsAndConditionsSection identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: TermsAndConditionsSections/Edit/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var termsAndConditionsSection = await _context.TermsAndConditionsSection.FindAsync(id);
            if (termsAndConditionsSection == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            return View(termsAndConditionsSection);
        }

        /// <summary>
        /// Allows to edit a section of terms and conditions
        /// </summary>
        /// <param name="id">TermsAndConditionsSection identification</param>
        /// <param name="termsAndConditionsSection">TermsAndConditionsSection object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: TermsAndConditionsSections/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id, [Bind("TermsAndConditionsSectionID,Title,Content,LastUpdate,LastUpdateUserID")] TermsAndConditionsSection termsAndConditionsSection)
        {
            if (id != termsAndConditionsSection.TermsAndConditionsSectionID)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var user = await _userManager.GetUserAsync(User);
            termsAndConditionsSection.LastUpdate = DateTime.Now;
            termsAndConditionsSection.LastUpdateUserID = user.Id;
            termsAndConditionsSection.User = user;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(termsAndConditionsSection);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TermsAndConditionsSectionExists(termsAndConditionsSection.TermsAndConditionsSectionID))
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

            return View(termsAndConditionsSection);
        }

        /// <summary>
        /// Allows to delete a section of the terms and conditions
        /// </summary>
        /// <param name="id">TermsAndConditionsSection identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: TermsAndConditionsSections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var termsAndConditionsSection = await _context.TermsAndConditionsSection.FindAsync(id);
            if (termsAndConditionsSection == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            _context.TermsAndConditionsSection.Remove(termsAndConditionsSection);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Checks if the section already exists
        /// </summary>
        /// <param name="id">TermsAndConditionsSection identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        private bool TermsAndConditionsSectionExists(int id)
        {
            return _context.TermsAndConditionsSection.Any(e => e.TermsAndConditionsSectionID == id);
        }
    }
}
