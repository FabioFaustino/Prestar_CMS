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
    /// News Controller
    /// </summary>
    /// <see cref="New"/>
    public class NewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Constructor of the NewsController class that receives two parameters 
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
        public NewsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// shows all the news
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> Index() 
        {
            var applicationDbContext = new List<New>();
            if (User.IsInRole("Moderador") || User.IsInRole("Administrador"))
            {
                applicationDbContext = await _context.New.Include(n => n.Writter).ToListAsync();
            }
            else
            {
                applicationDbContext = await _context.New.Include(n => n.Writter).Where(n => n.Visible == true || n.PrincipalNew == true).ToListAsync();
            }

            //ViewBag for bell icon on the nav menu
            if (User.Identity.IsAuthenticated)
            {
                var userLoggedIn = await _userManager.GetUserAsync(User);
                ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();
            }

            return View(applicationDbContext);
        }

        /// <summary>
        /// This method shows a specific new that has the id passed as a parameter.
        /// </summary>
        /// <param name="id">New identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            var newToView = await _context.New.Include(n => n.Writter).FirstOrDefaultAsync(m => m.NewsID == id);
            if (newToView == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            //ViewBag for bell icon on the nav menu
            if (User.Identity.IsAuthenticated)
            {
                var userLoggedIn = await _userManager.GetUserAsync(User);
                ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();
            }

            return View(newToView);
        }

        /// <summary>
        /// This method shows a view that allows the user to create a new.
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Administrador, Moderador")]
        public async Task<IActionResult> CreateAsync()
        {
            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View();
        }

        /// <summary>
        /// This method allows adding a new item (passed by parameter) to the database.
        /// </summary>
        /// <param name="newNew">New New object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador, Moderador")]
        public async Task<IActionResult> Create([Bind("NewsID,Title,SecondTitle,Description,Illustration,CreationDate,Visible,PrincipalNew,WriterID")] New newNew)
        {
            var user = await _userManager.GetUserAsync(User);
            newNew.WriterID = user.Id;
            newNew.CreationDate = DateTime.Now;
            newNew.Visible = false;
            newNew.PrincipalNew = false;

            try
            {
                if (Request.Form.Files.Count > 0)
                {
                    IFormFile file = Request.Form.Files.FirstOrDefault();
                    using var dataStream = new MemoryStream();
                    await file.CopyToAsync(dataStream);
                    newNew.Illustration = dataStream.ToArray();
                }
            }
            catch (InvalidOperationException) { }

            if (ModelState.IsValid)
            {
                _context.Add(newNew);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(newNew);
        }

        /// <summary>
        /// This method shows a view that allows the user to edit a new.
        /// </summary>
        /// <param name="id">New identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            var newToEdit = await _context.New.FindAsync(id);
            if (newToEdit == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(newToEdit);
        }

        /// <summary>
        /// This method allows to update the news with the id sent by parameter, through the new news sent also by parameter.
        /// </summary>
        /// <param name="id">New identification</param>
        /// <param name="newUpdate">New updated object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NewsID,Title,SecondTitle,Description,Illustration,CreationDate,Visible,PrincipalNew,WriterID")] New newUpdate)
        {
            if (id != newUpdate.NewsID)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            try
            {
                if (Request.Form.Files.Count > 0)
                {
                    IFormFile file = Request.Form.Files.FirstOrDefault();
                    using var dataStream = new MemoryStream();
                    await file.CopyToAsync(dataStream);
                    newUpdate.Illustration = dataStream.ToArray();
                }
            }
            catch (InvalidOperationException) { }

            int[] newPrincipal = _context.New.Where(n => n.PrincipalNew == true).Select(n => n.NewsID).ToArray();
            int[] newVisible = _context.New.Where(n => n.Visible == true).Select(n => n.NewsID).ToArray();

            if (!newPrincipal.Contains(id) && newUpdate.PrincipalNew == true)
            {
                if (newPrincipal.Count() == 3)
                {
                    ModelState.AddModelError("PrincipalNew", "Não pode ter mais do que 3 Noticias Principais");
                }
            }

            if (newUpdate.Illustration == null)
            {
                byte[] illustration = _context.New.Where(n => n.NewsID == newUpdate.NewsID).Select(n => n.Illustration).FirstOrDefault();
                newUpdate.Illustration = illustration;
            }

            //If Alter to True Send Notifications To All
            if (!newVisible.Contains(id) && newUpdate.Visible)
                SendNotificationToAll(newUpdate.NewsID);
            else if (!newPrincipal.Contains(id) && newUpdate.PrincipalNew)
                SendNotificationToAll(newUpdate.NewsID);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(newUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NewExists(newUpdate.NewsID))
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
            return View(newUpdate);
        }

        /// <summary>
        /// This method allows you to delete the news with the id passed by parameter.
        /// </summary>
        /// <param name="id">New identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            
            if (NewExists(id)) {
                var newToDelete = await _context.New.FindAsync(id);
                _context.New.Remove(newToDelete);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
        }

        /// <summary>
        /// This method checks if the news, with the id passed by parameter, exists.
        /// </summary>
        /// <param name="id">New identification</param>
        /// <returns>bool</returns>
        private bool NewExists(int id)
        {
            return _context.New.Any(e => e.NewsID == id);
        }

        /// <summary>
        /// This method sends notifications to all the users
        /// </summary>
        /// <param name="formationID"></param>
        private void SendNotificationToAll(int newID)
        {
            NotificationsController notificationController = new NotificationsController(_context, _userManager);
            foreach (string idDestiny in _context.Users.Where(u => u.Id != _userManager.GetUserAsync(User).Result.Id).Select(u => u.Id).ToList())
            {
                Notification notification = new Notification
                {
                    DestinaryID = idDestiny,
                    Subject = "Nova Notícia!",
                    Content = "Foi criada uma nova notícia. Confira em VER MAIS para ficar a par de todas as novidades.",
                    IsRead = false,
                    Action = "/News/Details/" + newID,
                    CreationDate = DateTime.Now,
                };
                var result = notificationController.Create(notification).Result;
            }
        }
    }
}
