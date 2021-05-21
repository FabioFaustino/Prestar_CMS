using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prestar.Data;
using Prestar.Models;
using Prestar.Services;

namespace Prestar.Controllers
{
    /// <summary>
    /// Home Controller that includes the actions for Privacy Policy
    /// </summary>
    /// <see cref="PrivacyPolicy"/>
    /// <seealso cref="PrivacyPolicySection"/>
    /// <seealso cref="PrivacyPolicySectionsController"/>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Constructor of the HomeController class that receives three parameters 
        /// and initializes them.
        /// </summary>
        /// <param name="userManager">
        /// Provides the APIs for managing user in a persistence store.
        /// <see cref="UserManager{TUser}"/>
        /// </param>
        /// <param name="logger">
        /// Represents a type used to perform logging.
        /// <see cref="ILogger"/>
        /// </param>
        /// <param name="context">
        /// Application database context
        /// <see cref="ApplicationDbContext"/>
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<User> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Show the principal page
        /// </summary>
        ///<returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                User user = _context.Users.Find(_userManager.GetUserId(User));
                ViewBag.UserEmail = user.Email;
                ViewBag.UserId = user.Id;

                //ViewBag for bell icon on the nav menu
                ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();
            }

            //Best Services
            List<Service> services = _context.Service
                .Include(s => s.User)
                .Include(s => s.ServiceCategory)
                .Include(s => s.CommentsAndEvaluations)
                .AsEnumerable()
                .Where(s => s.IsActive == true && s.IsBlocked == false && (s.User.LockoutEnd <= DateTimeOffset.Now || s.User.LockoutEnd == null))
                .ToList();

            ServicesController sc = new(_context, _userManager);

            ViewBag.BestServices = sc.Sort(services, "Descendente").Take(5);

            // Average evaluation
            Dictionary<int, double> averageEvaluations = new();
            // Adding values to the dictionary
            foreach (Service service in services)
            {
                if (!averageEvaluations.ContainsKey(service.ServiceID))
                    averageEvaluations.Add(service.ServiceID, sc.GetAverageEvaluation(service));
            }

            ViewBag.AverageEvaluations = averageEvaluations;


            ViewBag.News = await _context.New.Where(n => n.PrincipalNew == true).ToListAsync();

            return View(await _context.ServiceCategory.Include(sc => sc.ServiceCategories).OrderBy(sc => sc.Name).ToListAsync());
        }

        /// <summary>
        /// Shows the privacy policy page with all sections
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [AllowAnonymous]
        public async Task<IActionResult> Privacy(int? pageNumber)
        {
            //ViewBag for bell icon on the nav menu
            if (User.Identity.IsAuthenticated)
            {
                var userLoggedIn = await _userManager.GetUserAsync(User);
                ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();
            }

            int pageSize = 6;
            var applicationDbContext = await _context.PrivacyPolicySection.Include(p => p.User).ToListAsync();
            return View(await PaginatedList<PrivacyPolicySection>.CreateAsync(applicationDbContext, pageNumber ?? 1, pageSize));
        }

        /// <summary>
        /// This method shows the view to create a section in the privacy policy.
        /// </summary>
        /// <returns>IActionResult</returns>
        // GET: PrivacyPolicySections/Create
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CreateSection()
        {
            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();
            return View();
        }

        /// <summary>
        /// This method allows to create a new privacy policy section
        /// </summary>
        /// <param name="privacyPolicySection">Privacy Policy Section object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        /// <seealso cref="BindAttribute"/>
        // POST: PrivacyPolicySections/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="Administrador")]
        public async Task<IActionResult> CreateSection([Bind("PrivacyPolicySectionID,Title,Content,PrivacyPolicySectionLastUpdate,LastUpdateUserID")] PrivacyPolicySection privacyPolicySection)
        {
            var user = await _userManager.GetUserAsync(User);
            privacyPolicySection.PrivacyPolicySectionLastUpdate = DateTime.Now;
            privacyPolicySection.LastUpdateUserID = user.Id;
            privacyPolicySection.User = user;

            if (ModelState.IsValid)
            {
                _context.Add(privacyPolicySection);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Privacy));
            }
            return View(privacyPolicySection);
        }

        /// <summary>
        /// This method allows to see the view that gives the possibility to edit the section with the id passed by parameter.
        /// </summary>
        /// <param name="id">Privacy Policy Section identification</param>
        /// <returns></returns>
        // GET: PrivacyPolicySections/Edit/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EditSection(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var privacyPolicySection = await _context.PrivacyPolicySection.FindAsync(id);
            if (privacyPolicySection == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            return View(privacyPolicySection);
        }

        /// <summary>
        /// This method allows to update the sections with the id sent by parameter, through the new section sent also by parameter.
        /// </summary>
        /// <param name="id">Privacy policy section identification</param>
        /// <param name="privacyPolicySection">Privacy Policy Section updated</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        /// <seealso cref="BindAttribute"/>
        // POST: PrivacyPolicySections/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EditSection(int id, [Bind("PrivacyPolicySectionID,Title,Content,PrivacyPolicySectionLastUpdate,LastUpdateUserID")] PrivacyPolicySection privacyPolicySection)
        {
            if (id != privacyPolicySection.PrivacyPolicySectionID)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            var user = await _userManager.GetUserAsync(User);
            privacyPolicySection.PrivacyPolicySectionLastUpdate = DateTime.Now;
            privacyPolicySection.LastUpdateUserID = user.Id;
            privacyPolicySection.User = user;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(privacyPolicySection);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrivacyPolicySectionExists(privacyPolicySection.PrivacyPolicySectionID))
                    {
                        return View("/Views/Shared/NotFound.cshtml");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Privacy));
            }
            return View(privacyPolicySection);
        }

        /// <summary>
        /// This method allows to eliminate a section with id passed by parameter.
        /// </summary>
        /// <param name="id">Privacy Policy Section Identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: PrivacyPolicySections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var privacyPolicySection = await _context.PrivacyPolicySection.FindAsync(id);
            if (privacyPolicySection == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            _context.PrivacyPolicySection.Remove(privacyPolicySection);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Privacy));
        }

        /// <summary>
        /// Shows the contacts page
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [AllowAnonymous]
        public async Task<IActionResult> Contacts()
        {
            //ViewBag for bell icon on the nav menu
            if (User.Identity.IsAuthenticated)
            {
                var userLoggedIn = await _userManager.GetUserAsync(User);
                ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();
            }

            return View();
        }

        /// <summary>
        /// Shows error page
        /// </summary>
        /// <returns>Returns an IActionResult</returns>
        /// <seealso cref="IActionResult"/>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Checks if a privacy policy section exists in database
        /// </summary>
        /// <param name="id">Privacy policy section identification</param>
        /// <returns>bool value: true if exists, false otherwise</returns>
        private bool PrivacyPolicySectionExists(int id)
        {
            return _context.PrivacyPolicySection.Any(e => e.PrivacyPolicySectionID == id);
        }

    }
}
