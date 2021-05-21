using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prestar.Data;
using Prestar.Models;

namespace Prestar.Controllers
{
    /// <summary>
    /// Notifications Controller
    /// </summary>
    /// <see cref="Notification"/>
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Constructor of the NotificationsController class that receives two parameters 
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
        public NotificationsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// This function allows you to obtain all notifications.
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize]
        public async Task<IActionResult> Index(string error)
        {
            var user = await _userManager.GetUserAsync(User);
            var notifications = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id).OrderByDescending(o => o.CreationDate);

            //ViewBag for bell icon on the nav menu
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();

            ViewBag.NotFound = (error.Equals("False") ? false : true);
            return View(await notifications.ToListAsync());
        }

        /// <summary>
        /// This function allows the notification passed by parameter to be placed in the database.
        /// </summary>
        /// <param name="notification">Notification new object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NotificationID,DestinaryID,Subject,Content,IsRead,Action,CreationDate")] Notification notification)
        {
            User user = _context.Users.Where(u => u.Id == notification.DestinaryID).First();
            if (ModelState.IsValid && user.ReceiveNotifications)
            {
                _context.Add(notification);
                var result = await _context.SaveChangesAsync(); 
                return RedirectToAction(nameof(Index));
            }
            ViewData["DestinaryID"] = new SelectList(_context.Users, "Id", "Id", notification.DestinaryID);

            return View(notification); ;
        }

        /// <summary>
        /// This function marks the notification (with the id passed by parameter) as seen.
        /// </summary>
        /// <param name="id">Notification identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize]
        public async Task<IActionResult> UpdateGiveSight(int id)
        {
            var notification = await _context.Notification.FindAsync(id);
            if(notification == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            notification.IsRead = true;
            _context.Notification.Update(notification);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// This function allows you to delete the notification (with the id passed by parameter).
        /// </summary>
        /// <param name="id">Notification identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize]
        public async Task<IActionResult> ClearNotification(int id)
        {
            var notification = await _context.Notification.FindAsync(id);
            if (notification == null)
                return View("/Views/Shared/NotFound.cshtml");
            _context.Notification.Remove(notification);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { error = "False"});
        }

        /// <summary>
        /// This function allows to mark as visible all the notifications of the current user.
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize]
        public async Task<IActionResult> AllRead()
        {
            var user = await _userManager.GetUserAsync(User);
            List<Notification> notifications = _context.Notification.Where(u => u.DestinaryID == user.Id).ToList();
            notifications.ForEach(notif => {
                notif.IsRead = true;
                _context.Notification.Update(notif);
            });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { error = "False" });
        }

        /// <summary>
        /// This method redirects you to the action.
        /// </summary>
        /// <param name="id">Notification identification</param>
        /// <param name="url">URL</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize]
        public async Task<IActionResult> ViewDetails(int id, string url)
        {
            var user = await _userManager.GetUserAsync(User);
            Notification notification = _context.Notification.Where(u => u.NotificationID == id && u.IsRead == false).FirstOrDefault();
            if (notification != null) { 
                notification.IsRead = true;
                _context.Notification.Update(notification);
                await _context.SaveChangesAsync();
            }

            //ViewBag for bell icon on the nav menu
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();

            try
            {
                string[] urlparse = url.Split("/");
                if (urlparse.Length >= 4)
                {
                    //See if id exists in database
                    if (IdExists(urlparse[1], Int32.Parse(urlparse[3]))){
                        return RedirectToAction(urlparse[2], urlparse[1], new { id = Int32.Parse(urlparse[3]) });
                    }
                    else
                    {
                        return RedirectToAction(nameof(Index), "Notifications", new { error = "True" });
                    }

                }
                else if (urlparse.Length >= 2)
                {
                    return RedirectToAction("Index", urlparse[1]);
                }
            }catch(Exception e)
            {
                return RedirectToAction(nameof(Index), "Notifications", new { error = "True" });
            }

            return RedirectToAction(nameof(Index), "Notifications", new { error = "True" });
        }

        /// <summary>
        /// This function allows you to see if there is a notification in the database with the same id.
        /// </summary>
        /// <param name="id">Notification identification</param>
        /// <returns>bool</returns>
        private bool NotificationExists(int id)
        {
            return _context.Notification.Any(e => e.NotificationID == id);
        }

        /// <summary>
        /// This method checks if the id to access exists
        /// </summary>
        /// <param name="controller">Controller in string format</param>
        /// <param name="id">Id to the object to access</param>
        /// <returns>bool</returns>
        private bool IdExists(string controller, int id)
        {
            bool exists = false;
            switch (controller)
            {
                case "Abouts":
                    exists = _context.About.Find(id) == null ? false : true;
                    break;
                case "CommentAndEvaluation":
                    exists = _context.CommentAndEvaluation.Find(id) == null ? false : true;
                    break;
                case "Complaints":
                    exists = _context.Complaint.Find(id) == null ? false : true;
                    break;
                case "Formations":
                    exists = _context.Formation.Find(id) == null ? false : true;
                    break;
                case "News":
                    exists = _context.New.Find(id) == null ? false : true;
                    break;
                case "Requests":
                    exists = _context.Request.Find(id) == null ? false : true;
                    break;
                case "ServiceRequisitions":
                    exists = _context.ServiceRequisition.Find(id) == null ? false : true;
                    break;
                case "Services":
                    exists = _context.Service.Find(id) == null ? false : true;
                    break;
                default:
                    exists = false;
                    break;
            }
            return exists;
        }
    }
}
