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
    /// Formations Controller
    /// </summary>
    /// <see cref="Formation"/>
    public class FormationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Constructor of the FormationsController class that receives two parameters 
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
        public FormationsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// This method allows you to view all the formation
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize]
        public async Task<IActionResult> Index()
        {
            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            var applicationDbContext = _context.Formation.Include(f => f.Responsible).OrderByDescending(d => d.Date);
            return View(await applicationDbContext.ToListAsync());
        }

        /// <summary>
        /// This method shows a specific formation with the id passed by parameter. 
        /// </summary>
        /// <param name="id">Formation identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var formation = await _context.Formation.Include(f => f.Responsible).FirstOrDefaultAsync(m => m.FormationID == id);
            if (formation == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            var user = await _userManager.GetUserAsync(User);
            ViewBag.CanAlter = !(DateTime.Now.AddDays(2) >= formation.Date || formation.ResponsibleID != user.Id);
            ViewBag.CanRegister = (_context.Enrollment.Where(e => e.RegisteredID == user.Id && e.FormationID == id).FirstOrDefault() == null);

            //ViewBag for bell icon on the nav menu
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();

            return View(formation);
        }

        /// <summary>
        /// This method shows the view to create a new formation
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Moderador,Administrador")]
        public async Task<IActionResult> CreateAsync()
        {
            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View();
        }

        /// <summary>
        /// This method allows to create a new formation.
        /// </summary>
        /// <param name="formation">New Formation Object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [HttpPost]
        [Authorize(Roles = "Moderador,Administrador")]
        public async Task<IActionResult> Create([Bind("FormationID,NumberOfRegistrations,DurationMinutes,Date,Title,Content,MaxEnrollment,Local,ResponsibleID,Illustration")] Formation formation)
        {
            try
            {
                if (Request.Form.Files.Count > 0)
                {
                    IFormFile file = Request.Form.Files.FirstOrDefault();
                    using var dataStream = new MemoryStream();
                    await file.CopyToAsync(dataStream);
                    formation.Illustration = dataStream.ToArray();
                }
            }
            catch (InvalidOperationException) { }
            var user = await _userManager.GetUserAsync(User);
            formation.ResponsibleID = user.Id;
            if (formation.Date < DateTime.Now)
            {
                ModelState.AddModelError("Date", "Não pode escolher uma data anterior a hoje");
            }
            if (ModelState.IsValid)
            {
                _context.Add(formation);
                await _context.SaveChangesAsync();
                SendNotificationToAll(_context.Formation.OrderBy(f => f.FormationID).Last().FormationID);
                return RedirectToAction(nameof(Index));
            }
            return View(formation);
        }

        /// <summary>
        /// This method allows showing the view that makes it possible to make changes to the formation.
        /// </summary>
        /// <param name="id">Formation identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [HttpGet]
        [Authorize(Roles = "Moderador,Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            var formation = await _context.Formation.FindAsync(id);
            if (formation == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(formation);
        }

        /// <summary>
        /// This method allows to update the formation with the id passed by parameter.
        /// </summary>
        /// <param name="id">Formation identification</param>
        /// <param name="formation">Formation updated object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [HttpPost]
        [Authorize(Roles = "Moderador,Administrador")]
        public async Task<IActionResult> Edit(int id, [Bind("FormationID,NumberOfRegistrations,DurationMinutes,Date,Title,Content,MaxEnrollment,Local,ResponsibleID,Illustration")] Formation formation)
        {
            if (id != formation.FormationID)
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
                    formation.Illustration = dataStream.ToArray();
                }
            }
            catch (InvalidOperationException) { }
            if (formation.Illustration == null)
            {
                byte[] illustration = _context.Formation.Where(n => n.FormationID == formation.FormationID).Select(n => n.Illustration).FirstOrDefault();
                formation.Illustration = illustration;
            }
            if (formation.Date < DateTime.Now)
            {
                ModelState.AddModelError("Date", "Não pode escolher uma data anterior a hoje");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(formation);
                    await _context.SaveChangesAsync();
                    SendNotificationAlter(formation.FormationID);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FormationExists(formation.FormationID))
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
            return View(formation);
        }

        /// <summary>
        /// This method allows to eliminate formation with the id passed by parameter.
        /// </summary>
        /// <param name="id">Formation identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Moderador,Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!FormationExists(id))
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            var formation = await _context.Formation.FindAsync(id);
            _context.Formation.Remove(formation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// This method allows the enrollment of a participant in the enrollment.
        /// </summary>
        /// <param name="id">Formation identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> Enrollment(int id)
        {
            if(!FormationExists(id))
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            Formation formation = _context.Formation.Find(id);

            if (formation.MaxEnrollment > formation.NumberOfRegistrations) { 
                var user = await _userManager.GetUserAsync(User);
                Enrollment enrollment = new Enrollment { FormationID = id, RegisteredID = user.Id };
                _context.Add(enrollment);
                await _context.SaveChangesAsync();

                //Atualizar o Número de inscrições
                formation.NumberOfRegistrations = formation.NumberOfRegistrations + 1;
                _context.Update(formation);
                await _context.SaveChangesAsync();
            }
            else
            {
                ViewBag.Erro("Já não existem mais incrições disponíveis");
            }
            return RedirectToAction(nameof(Details), new { id = id });
        }

        /// <summary>
        /// This method allows the cancellation of the registration of a participant in the enrollment.
        /// </summary>
        /// <param name="id">formation identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> RemoveEnrollment(int id)
        {
            if (!FormationExists(id))
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            Formation formation = _context.Formation.Find(id);
            var user = await _userManager.GetUserAsync(User);
            Enrollment enrollment = _context.Enrollment.Where(e => e.FormationID == id && e.RegisteredID == user.Id).FirstOrDefault();
            if (enrollment == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            _context.Remove(enrollment);
            await _context.SaveChangesAsync();

            //Atualizar o Número de inscrições
            formation.NumberOfRegistrations -= 1;
            _context.Update(formation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = id });
        }

        /// <summary>
        /// This method allows to view the participants enrolled in the formation.
        /// </summary>
        /// <param name="id">formation identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> Participants(int id)
        {
            if (!FormationExists(id))
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            Formation formation = _context.Formation.Find(id);
            List<Enrollment> enrollments = await _context.Enrollment.Where(e => e.FormationID == id).Include(e => e.Registered).ToListAsync();

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();


            return View(enrollments);
        }

        /// <summary>
        /// This method allows to understand if the formation with id passed by parameter is in the database.
        /// </summary>
        /// <param name="id">Formation identification</param>
        /// <returns>bool</returns>
        private bool FormationExists(int id)
        {
            return _context.Formation.Any(e => e.FormationID == id);
        }

        /// <summary>
        /// This method returns id of the formation with the title passed by parameter
        /// </summary>
        /// <param name="title">Formation title</param>
        /// <returns>int</returns>
        [HttpGet]
        public int GetIDByTitle(string title)
        {
            return _context.Formation.Where(f => f.Title == title).FirstOrDefault().FormationID;
        }

        /// <summary>
        /// This method sends the notification to the participants.
        /// </summary>
        /// <param name="formationID">Formation identification</param>
        /// <returns>void</returns>
        private void SendNotificationAlter(int formationID)
        {
            NotificationsController notificationController = new NotificationsController(_context, _userManager);
            foreach(string idDestiny in _context.Enrollment.Where(f => f.FormationID == formationID).Select(f => f.RegisteredID).ToList())
            {
                Notification notification = new Notification
                {
                    DestinaryID = idDestiny,
                    Subject = "Alteração numa formação que está inscrito",
                    Content = "Foram feitas alterações a uma formação que está inscrito. Confira abaixo as informações.",
                    IsRead = false,
                    Action = "/Formations/Details/" + formationID,
                    CreationDate = DateTime.Now,
                };
                var result = notificationController.Create(notification).Result;
            }
        }

        /// <summary>
        /// This method sends notifications to all the users
        /// </summary>
        /// <param name="formationID"></param>
        private void SendNotificationToAll(int formationID)
        {
            NotificationsController notificationController = new NotificationsController(_context, _userManager);
            foreach (string idDestiny in _context.Users.Where(u => u.Id != _userManager.GetUserAsync(User).Result.Id).Select(u => u.Id).ToList())
            {
                Notification notification = new Notification
                {
                    DestinaryID = idDestiny,
                    Subject = "Nova Formação!",
                    Content = "Foi criada uma nova formação. Não perca porque vai ser uma excelente oportunidade.",
                    IsRead = false,
                    Action = "/Formations/Details/" + formationID,
                    CreationDate = DateTime.Now,
                };
                var result = notificationController.Create(notification).Result;
            }
        }
    }
}
