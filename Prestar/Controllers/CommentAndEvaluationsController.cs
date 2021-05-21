using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prestar.Data;
using Prestar.Models;

namespace Prestar.Controllers
{
    /// <summary>
    /// CommentAndEvaluations Controller
    /// </summary>
    /// <see cref="CommentAndEvaluation"/>
    public class CommentAndEvaluationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;

        /// <summary>
        /// Constructor of the CommentAndEvaluations class that receives two parameters 
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
        public CommentAndEvaluationsController(ApplicationDbContext context, UserManager<User> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Allows you to display every comment made on a Service
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: CommentAndEvaluations
        public async Task<IActionResult> Index()
        {
            var comments = await _context.CommentAndEvaluation.Include(c => c.Service)
                .Include(c => c.UserCommenting)
                .Where(c => c.IsRemoved == false)
                .OrderBy(c => c.CreationDate).ToListAsync();


            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();
             
            return View(comments);
        }

        /// <summary>
        /// Allows to add a new CommentAndEvaluation objetc a service
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: CommentAndEvaluations/Create
        public async Task<IActionResult> CreateAsync()
        {
            ViewData["ServiceID"] = new SelectList(_context.Service, "ServiceID", "Description");

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View();
        }

        /// /// <summary>
        /// On the creation of a comment and evaluation, we increment the providers points, according to what is
        /// defined on the gamification for the provider
        /// </summary>
        /// <param name="commentAndEvaluation">CommentAndEvaluation object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: CommentAndEvaluations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("CommentAndEvaluationID,CreationDate,UserCommentingID,ServiceID,Evaluation,Comment")] CommentAndEvaluation commentAndEvaluation)
        {
            var service = _context.Service.Find(commentAndEvaluation.ServiceID);
            commentAndEvaluation.UserCommenting = await _userManager.GetUserAsync(User);
            commentAndEvaluation.UserCommentingID = commentAndEvaluation.UserCommenting.Id;
            commentAndEvaluation.CreationDate = DateTime.Now;
            commentAndEvaluation.LastUpdate = DateTime.Now;
            commentAndEvaluation.Service = service;
            commentAndEvaluation.IsEdited = false;

            if (ModelState.IsValid)
            {
                _context.CommentAndEvaluation.Add(commentAndEvaluation);
               

                // Gamification Points increment
                UpdatePoints(commentAndEvaluation, service, "Increment");


                await _context.SaveChangesAsync();
                SendNotification(commentAndEvaluation, "Create");
                SendEmail(commentAndEvaluation, "Create");
                

                return RedirectToAction("Details", "Services", new { id = commentAndEvaluation.ServiceID });
            }

            ViewData["ServiceID"] = new SelectList(_context.Service, "ServiceID", "Description", commentAndEvaluation.ServiceID);
            ViewData["UserCommentingID"] = _userManager.GetUserAsync(User).Result.Id;
            return View("/Views/Shared/NotFound.cshtml");
        }

        /// <summary>
        /// Creates a form to comment and evaluate
        /// </summary>
        /// <param name="id">Service identification</param>
        /// <param name="fromRequisitions">
        /// Checks if it cames form a service requisition
        /// </param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: CommentAndEvaluations/Create/5
        public async Task<IActionResult> CreateForm(int? id, bool fromRequisitions)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            if (await _context.Service.FindAsync(id) == null)
                return View("/Views/Shared/NotFound.cshtml");

            ViewBag.ServiceID = id;

            if (fromRequisitions == false)
            {
                return PartialView("_CreateCommentsPartial");
            }
            else
            {
                return PartialView("_CreateCommentsPartialFromRequisitions");
            }
        }

        /// <summary>
        /// Edits a comment and/or evaluation
        /// </summary>
        /// <param name="id">CommentAndEvaluation identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: CommentAndEvaluations/Edit/5
        [Authorize(Roles = "Prestador, Cliente, Moderador, Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
          
            var commentAndEvaluation = await _context.CommentAndEvaluation.FindAsync(id);
    


            if (commentAndEvaluation == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            ViewData["ServiceID"] = new SelectList(_context.Service, "ServiceID", "Description", commentAndEvaluation.ServiceID);
            ViewData["UserCommentingID"] = new SelectList(_context.Users, "Id", "Id", commentAndEvaluation.UserCommentingID);
            
           
            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return PartialView("_EditCommentsPartial", commentAndEvaluation);
        }

        /// <summary>
        /// Edits a comment and/or evaluation
        /// </summary>
        /// <param name="id">CommentAndEvaluation Identification</param>
        /// <param name="commentAndEvaluation">CommentAndEvaluation Updated Object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: CommentAndEvaluations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Prestador, Cliente, Moderador, Administrador")]
        public async Task<IActionResult> Edit(int id, [Bind("CommentAndEvaluationID,UserCommentingID,ServiceID,Evaluation,Comment,CreationDate")] CommentAndEvaluation commentAndEvaluation)
        {

            if (id != commentAndEvaluation.CommentAndEvaluationID)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            if (_userManager.GetUserAsync(User).Result.Id != commentAndEvaluation.UserCommentingID)
            {
                return Unauthorized();
            }
                       

            if (ModelState.IsValid)
            {
                try
                {
                    commentAndEvaluation.LastUpdate = DateTime.Now;
                    commentAndEvaluation.IsEdited = true;
                    
                    _context.Update(commentAndEvaluation);
                    await _context.SaveChangesAsync();
                    SendNotification(commentAndEvaluation, "Edit");
                    SendEmail(commentAndEvaluation, "Edit");
                    
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentAndEvaluationExists(commentAndEvaluation.CommentAndEvaluationID))
                    {
                        return View("/Views/Shared/NotFound.cshtml");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Services", new { id = commentAndEvaluation.ServiceID });
            }
            ViewData["ServiceID"] = new SelectList(_context.Service, "ServiceID", "Description", commentAndEvaluation.ServiceID);
            ViewData["UserCommentingID"] = new SelectList(_context.Users, "Id", "Id", commentAndEvaluation.UserCommentingID);
            return RedirectToAction("Details", "Services", new { id = commentAndEvaluation.ServiceID }); ;
        }

        /// <summary>
        /// Since we are using a Modal to confirm the deletion of a comment, we dont go through the Get phase
        /// When a comment is deleted, the user's points are subracted from his total points
        /// </summary>
        /// <param name="id">CommentAnd Evaluation identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: CommentAndEvaluations/Delete/5
        [Authorize(Roles = "Prestador, Cliente, Moderador, Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var commentAndEvaluation = await _context.CommentAndEvaluation
                .Include(c => c.Service)
                .Include(c => c.UserCommenting)
                .FirstOrDefaultAsync(m => m.CommentAndEvaluationID == id);
            if (commentAndEvaluation == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            _context.CommentAndEvaluation.Remove(commentAndEvaluation);

            //Gamification, decrease points
            UpdatePoints(commentAndEvaluation, commentAndEvaluation.Service, "Decrease");

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();


            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Services", new { id = commentAndEvaluation.ServiceID });
        }

        /// <summary>
        /// Since a provider will be allowed to remove comments on its own service, in order to not let it influence the evaluation made
        /// instead of removind it from database it will simply be set as removed, and not showed on comments list. 
        /// The provider gamificaion points are still decreased
        /// </summary>
        /// <param name="id">CommentAndEvaluation identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: CommentAndEvaluations/RemoveComment/5
        [Authorize(Roles = "Prestador, Cliente, Moderador, Administrador")]
        public async Task<IActionResult> RemoveComment(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var commentAndEvaluation = await _context.CommentAndEvaluation
                .Include(c => c.Service)
                .Include(c => c.UserCommenting)
                .FirstOrDefaultAsync(m => m.CommentAndEvaluationID == id);
            if (commentAndEvaluation == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            commentAndEvaluation.IsRemoved = true;
            _context.CommentAndEvaluation.Update(commentAndEvaluation);

            //Gamification, decrease points
            UpdatePoints(commentAndEvaluation, commentAndEvaluation.Service, "Decrease");

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();


            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Services", new { id = commentAndEvaluation.ServiceID });
        }

        /// <summary>
        /// Checks if a CommentAndEvalution exists
        /// </summary>
        /// <param name="id">CommentAndEvaluation identification</param>
        /// <returns>bool</returns>
        private bool CommentAndEvaluationExists(int id)
        {
            return _context.CommentAndEvaluation.Any(e => e.CommentAndEvaluationID == id);
        }


        /// <summary>
        /// This method increments or decreases the user total points, according to the action received in params
        /// </summary>
        /// <param name="commentAndEvaluation">CommentAndEvaluation object</param>
        /// <param name="service">Commented/Evaluated service object</param>
        /// <param name="action">action</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        private void UpdatePoints(CommentAndEvaluation commentAndEvaluation, Service service, String action)
        {
            var gamification = _context.Gamification.Where(g => g.IsActive == true).FirstOrDefault();

            if (gamification != null)
            {
                var points = 0;

                var user = _context.Users.Where(u => u.Id == service.UserID).FirstOrDefault();
                if (commentAndEvaluation.Evaluation > 0)
                    points += gamification.PointsPerEvaluation;
                if (commentAndEvaluation.Comment != null && commentAndEvaluation.Comment != " ")
                    points += gamification.PointsPerComment;
                if (points > 0)
                {
                    if (action == "Increment")
                    {
                        user.TotalPoints += points;
                    }
                    else
                    {
                        user.TotalPoints -= points;
                    }
                    _context.Users.Update(user);
                }
                return;
            }
            return;
        }

        /// <summary>
        /// This method returns all points of the user.
        /// </summary>
        /// <returns>int</returns>
        [HttpGet]
        public int GetPoints()
        {
            var user = _userManager.GetUserAsync(User).Result;
            return user.TotalPoints;
        }

        /// <summary>
        /// Sends email to the provider when a comment is created or edited on one of his services.
        /// </summary>
        /// <param name="serviceRequisition">Service Requisition object</param>
        /// <param name="service">Service object</param>
        private void SendEmail(CommentAndEvaluation commentAndEvaluation, String action)
        {
            try
            {
                String initialPhrase = action == "Create" ? "Recebeu um novo comentário no seu serviço " : "Um comentário foi editado no seu serviço ";
              
                var emailBody = string.Format("<p>"+ initialPhrase + ": <b>{0} </b></p><br><p><b>Feito por:</b> {1} </p><p><b><br>Avaliação:</b> {2} Estrelas." +
                    "<br> <b>Comentário:</b> {3} <br>",
                    commentAndEvaluation.Service.ServiceName,
                    commentAndEvaluation.UserCommenting.UserName,
                    commentAndEvaluation.Evaluation,
                    commentAndEvaluation.Comment);

                var emailSubject = action == "Create" ? "Existe um novo comentário num dos seus serviços" : "Um comentário foi atualizado no seu serviço";
                

                //Send mail to provider
                _emailSender.SendEmailAsync(commentAndEvaluation.Service.User.Email, emailSubject, emailBody);
               
            }
            catch (Exception e) { }
        }

        /// <summary>
        /// This method sends the notification to the service provider alerting that a comment has been created or edited.
        /// </summary>
        /// <param name="service">Service Requisition object</param>
        private void SendNotification(CommentAndEvaluation commentAndEvaluation, String action)
        {
            var service =  _context.Service.Where(s => s.ServiceID == commentAndEvaluation.ServiceID).FirstOrDefault();
            

            NotificationsController notificationController = new(_context, _userManager);
            Notification notification = new()
            {
                DestinaryID = commentAndEvaluation.Service.UserID,
                Subject = action == "Create" ? "Recebeu um novo comentário no seu serviço" : "Um comentário foi editado no seu serviço ",
                Content = "O utilizador " + commentAndEvaluation.UserCommenting.FirstName + " " + commentAndEvaluation.UserCommenting.LastName + " " +
                action == "Create" ? "comentou o seu serviço" : "editou um comentário no seu serviço" + " " + service.ServiceName + 
                ". Aconselhamos que consulte o mesmo para verificar a integridade do comentário.",
                IsRead = false,
                Action = "/Services/Details/" + commentAndEvaluation.ServiceID,
                CreationDate = DateTime.Now,
            };
            _ = notificationController.Create(notification).Result;
        }
    }
}
