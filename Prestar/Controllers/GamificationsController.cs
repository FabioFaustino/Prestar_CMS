using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prestar.Data;
using Prestar.Models;

namespace Prestar.Controllers
{
    /// <summary>
    /// Gamification Controller
    /// </summary>
    /// <see cref="Gamification"/>
    public class GamificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Constructor of the GamificationsController class that receives two parameters 
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
        public GamificationsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        /// <summary>
        /// This method sends to the view the current active gamification model and a ranking of users, ordered descending by totalpoints
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: Gamifications
        public async Task<IActionResult> Index()
        {
            var gamification = await _context.Gamification.Where(g => g.IsActive == true).FirstOrDefaultAsync();

            if(gamification == null)
            {
                gamification = await _context.Gamification.FirstOrDefaultAsync();
                if (gamification != null)
                {
                    gamification.IsActive = true;
                    _context.Gamification.Update(gamification);
                    _context.SaveChanges();
                }
            }

            var users = await _context.Users.Where(g => g.TotalPoints > 0).OrderByDescending(g => g.TotalPoints).ToListAsync();

            ViewBag.Users = users;

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(gamification);
        }

        /// <summary>
        /// Details of gamification criteria
        /// </summary>
        /// <param name="id">Gamification identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: Gamifications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var gamification = await _context.Gamification
                .FirstOrDefaultAsync(m => m.GamificationID == id);
            if (gamification == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(gamification);
        }

        /// <summary>
        /// To create a new gamification configuration
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: Gamifications/Create
        public async Task<IActionResult> CreateAsync()
        {
            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View();
        }

        /// <summary>
        /// This method creates a new gamification configuration. In case there is no active configuration, the new configuration will 
        /// be set to active, otherwise, it will be set as inactive
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: Gamifications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GamificationID,PointsPerComment,PointsPerEvaluation,PointsPerService,GamificationName")] Gamification gamification)
        {
            var activeConfiguration = await _context.Gamification.Where(g => g.IsActive == true).FirstOrDefaultAsync();
            
            if (ModelState.IsValid)
            {
                if (activeConfiguration == null)
                {
                    gamification.IsActive = true;
                }
                else
                {
                    gamification.IsActive = false;
                }
                _context.Add(gamification);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(gamification);
        }

        /// <summary>
        /// To edit a gamification configuration
        /// </summary>
        /// <param name="id">Gamification identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: Gamifications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var gamification = await _context.Gamification.FindAsync(id);
            if (gamification == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(gamification);
        }

        /// <summary>
        /// Edits gamification criteria
        /// </summary>
        /// <param name="id">Gamification Identification</param>
        /// <param name="gamification">Gamification Updated object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: Gamifications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GamificationID,PointsPerComment,PointsPerEvaluation,PointsPerService,GamificationName")] Gamification gamification)
        {
            if (id != gamification.GamificationID)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gamification);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GamificationExists(gamification.GamificationID))
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
            return View(gamification);
        }

        /// <summary>
        /// To delete a gamification configuration
        /// </summary>
        /// <param name="id">Gamification identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: Gamifications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var gamification = await _context.Gamification
                .FirstOrDefaultAsync(m => m.GamificationID == id);
            if (gamification == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            ViewBag.GamificationsCount = _context.Gamification.Count();

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(gamification);
        }

        /// <summary>
        /// Deletes a gamification configuration.
        /// In case the is only 1 gamification in the system, it will return not found, to prevent from working with no active gamification component
        /// </summary>
        /// <param name="id">Gamification identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: Gamifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            var gamification = await _context.Gamification.FindAsync(id);
            if (gamification == null)
                return View("/Views/Shared/NotFound.cshtml");

            if(_context.Gamification.Count() <= 1)
                return View("/Views/Shared/NotFound.cshtml");

            _context.Gamification.Remove(gamification);
            await _context.SaveChangesAsync();
            
            // Activates a random configuration in order to allways keep an active config
            var newConfiguration = await _context.Gamification.FirstOrDefaultAsync();
            newConfiguration.IsActive = true;
            _context.Gamification.Update(newConfiguration);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// This method displays a list of all configurations
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> Configurations()
        {
            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(await _context.Gamification.ToListAsync());
        }

        /// <summary>
        /// This Method displays the information about the configuration to activate, and asks for a confirmation on the activate action
        /// <param name="id">Gamification identification</param>>
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> Activate(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var gamification = await _context.Gamification.FirstOrDefaultAsync(m => m.GamificationID == id);
            if (gamification == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(gamification);
        }


        /// <summary>
        /// This Method confirms the activation of the chosen gamification configuration.
        /// After setting a configuration active, all other configurations are set to inactive.
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <param name="newConfiguration">Gamification configuration object to activate</param>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: Gamifications/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateConfirmed([Bind("GamificationID,PointsPerComment,PointsPerEvaluation,PointsPerService,GamificationName")] Gamification newConfiguration)
        {
            
            if (_context.Gamification.Find(newConfiguration.GamificationID) == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            //Deactivate current active configurations
            var currentConfigurations = await _context.Gamification.Where(c => c.IsActive == true).ToListAsync();
            foreach(var configuration in currentConfigurations)
            {
                configuration.IsActive = false;
                _context.Gamification.Update(configuration);
            }

            //Activate chosen configuration
           
            newConfiguration.IsActive = true;
            _context.Gamification.Update(newConfiguration);

            //Update Database
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Configurations));
        }

        /// <summary>
        /// Checks if a gamification configuration exists
        /// </summary>
        /// <param name="id">Gamification identification</param>
        /// <returns>bool</returns>
        private bool GamificationExists(int id)
        {
            return _context.Gamification.Any(e => e.GamificationID == id);
        }

    }
}
