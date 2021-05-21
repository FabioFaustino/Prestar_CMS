using System;
using System.Collections.Generic;
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
    /// Resquests Controller
    /// </summary>
    /// <see cref="Request"/>
    [Authorize]
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Constructor of the RequestsController class that receives three parameters 
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
        /// <param name="roleManager">
        /// Provides the APIs for managing roles in a persistence store.
        /// <see cref="RoleManager{TRole}"/>
        /// </param>
        public RequestsController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Shows all the requests in the database
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: Requests
        [Authorize(Roles = "Moderador,Administrador")]
        public async Task<IActionResult> Index(int? pageNumber)
        {
            int pageSize = 20;
            var applicationDbContext = _context.Request.Include(r => r.Requisitioner).Include(h => h.Handler);

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(await PaginatedList<Request>.CreateAsync(applicationDbContext.ToList(), pageNumber ?? 1, pageSize));
        }

        /// <summary>
        /// This method allows us to show the request that the user want to delete (search by id)
        /// </summary>
        /// <param name="id">Request identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: Requests/Delete/5
        [Authorize(Roles = "Administrador, Moderador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var request = await _context.Request
                .Include(r => r.Requisitioner)
                .FirstOrDefaultAsync(m => m.RequestID == id);
            if (request == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();


            return View(request);
        }

        /// <summary>
        /// This method allows us to remove a request from the database (search by id)
        /// </summary>
        /// <param name="id">Request identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: Requests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador, Moderador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (RequestExists(id))
            {
                var request = await _context.Request.FindAsync(id);
                _context.Request.Remove(request);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
        }
        /// <summary>
        /// Checks if a request exists
        /// </summary>
        /// <param name="id">Request identification</param>
        /// <returns>bool</returns>
        public bool RequestExists(int id)
        {
            return _context.Request.Any(e => e.RequestID == id);
        }

        /// <summary>
        /// Alter the RequestStatus to aprove and add another important information to the request with the id passed by parameter (id).
        /// This method also alters the role of the user with the id passed by parameter (idUser)
        /// </summary>
        /// <param name="idUser">User identification</param>
        /// <param name="id">Request identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: Requests/AproveProvideServices/5
        [HttpGet]
        [Authorize(Roles = "Administrador, Moderador")]
        public async Task<IActionResult> AproveProvideServices(string idUser, int id)
        {
            if (RequestExists(id) && _context.Request.Find(id).RequisitionerID == idUser &&  User.IsInRole("Moderador"))
            {
                Request request = await _context.Request.FindAsync(id);
                request.RequestHandlerID = _userManager.GetUserId(User);
                request.HandleDateTime = DateTime.Now;
                request.RequestStatus = RequestStatus.Aproved;

                try
                {
                    User user = await _context.Users.FindAsync(idUser);
                    await _userManager.RemoveFromRoleAsync(user, "Cliente");
                    await _userManager.AddToRoleAsync(user, "Prestador");
                    _context.Update(request);
                    await _context.SaveChangesAsync();
                    var subject = "Pedido Para Alteração de Conta Aprovado";
                    var content = "O seu pedido foi analisado e aprovado. Já é um prestador de serviço! 👏👏";
                    var action = "/Identity/Account/Manage";
                    SendNotification(request, subject, content, action);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }

                //ViewBag for bell icon on the nav menu
                var userLoggedIn = await _userManager.GetUserAsync(User);
                ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();


                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
        }

        /// <summary>
        /// This method show the information of the request that the user is reproving.
        /// </summary>
        /// <param name="id">Request identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [HttpGet]
        [Authorize(Roles = "Administrador, Moderador")]
        public async Task<IActionResult> ReproveProvideServicesAsync(int id)
        {
            if (RequestExists(id) &&  User.IsInRole("Moderador"))
            {
                Request request = _context.Request.Include(r => r.Requisitioner).Where(r => r.RequestID == id).First();

                //ViewBag for bell icon on the nav menu
                var userLoggedIn = await _userManager.GetUserAsync(User);
                ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

                return View(request);
            }
            else
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
        }

        /// <summary>
        /// This method allows a category request to be sent to the moderator.
        /// </summary>
        /// <param name="categoria"></param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: Requests/ReproveProvideServices/5
        [HttpPost]
        public async Task<IActionResult> RequestCategory(string categoria)
        {
            var user = await _userManager.GetUserAsync(User);
            Request request = new Request
            {
                RequisitionerID = user.Id,
                RequestType = RequestType.AddCategory,
                Description = "O prestador pede para que seja criado uma categoria com o nome " + categoria,
                CreationDateTime = DateTime.Now,
                RequestStatus = RequestStatus.WaitingApproval,
                RejectionMotive = ""
            };
            _context.Request.Add(request);
            await _context.SaveChangesAsync();

            SendNotificationModeradores();
            return RedirectToAction("Create", "Services", new { send = true });
        }

        /// <summary>
        /// This method allows you to create a category at the same time that the order is approved.
        /// </summary>
        /// <param name="id">Request identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [HttpGet]
        [Authorize(Roles = "Moderador")]
        public async Task<IActionResult> Categories(int id)
        {
            if (RequestExists(id))
            {
                Request request = await _context.Request.FindAsync(id);
                request.RequestHandlerID = _userManager.GetUserId(User);
                request.HandleDateTime = DateTime.Now;
                request.RequestStatus = RequestStatus.Aproved;


                //ViewBag for bell icon on the nav menu
                var userLoggedIn = await _userManager.GetUserAsync(User);
                ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

                _context.Update(request);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "ServiceCategories");
            }
            else
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
        }

        /// <summary>
        /// Rejest request for creating category
        /// </summary>
        /// <param name="id">Request identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [HttpGet]
        [Authorize(Roles = "Moderador")]
        public async Task<IActionResult> NotCreateCategorie(int id)
        {
            if (RequestExists(id))
            {
                Request request = _context.Request.Include(r => r.Requisitioner).Where(r => r.RequestID == id).First();

                //ViewBag for bell icon on the nav menu
                var userLoggedIn = await _userManager.GetUserAsync(User);
                ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

                return View(nameof(ReproveProvideServices), request);
            }
            else
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
        }

        /// <summary>
        /// Allow to reject the request
        /// </summary>
        /// <param name="id">Request identification</param>
        /// <param name="requestUpdate">Request updated object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [HttpPost]
        [Authorize(Roles = "Moderador")]
        public async Task<IActionResult> NotCreateCategorie(int id, [Bind("RequestID,RequisitionerID,RequestType,Description,CreationDateTime,RequestHandlerID,HandleDateTime,RequestStatus,RejectionMotive")] Request requestUpdate)
        {
            if (RequestExists(id))
            {
                Request request = await _context.Request.FindAsync(id);
                request.RequestHandlerID = _userManager.GetUserId(User);
                request.HandleDateTime = DateTime.Now;
                request.RequestStatus = RequestStatus.Rejected;
                request.RejectionMotive = requestUpdate.RejectionMotive;
                
                _context.Update(request);
                await _context.SaveChangesAsync();

                var subject = "Pedido Para Adicionar uma Nova Categoria";
                var content = "Foi rejeitado o seu pedido para adicionar uma nova categoria. O motivo para tal foi :" + request.RejectionMotive;
                var action = "/";
                SendNotification(request, subject, content, action);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
        }

        /// <summary>
        /// Alter the RequestStatus to reprove and add another important information to the request with the id passed by parameter (id).
        /// </summary>
        /// <param name="id">Request identification</param>
        /// <param name="requestUpdate">Request updated object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // Post: Requests/ReproveProvideServices/5
        [HttpPost]
        [Authorize(Roles = "Administrador, Moderador")]
        public async Task<IActionResult> ReproveProvideServices(int id, [Bind("RequestID,RequisitionerID,RequestType,Description,CreationDateTime,RequestHandlerID,HandleDateTime,RequestStatus,RejectionMotive")] Request requestUpdate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (RequestExists(id) && User.IsInRole("Moderador"))
            {
                Request request = await _context.Request.FindAsync(id);
                request.RequestHandlerID = _userManager.GetUserId(User);
                request.HandleDateTime = DateTime.Now;
                request.RequestStatus = RequestStatus.Rejected;
                request.RejectionMotive = requestUpdate.RejectionMotive;
                _context.Update(request);
                await _context.SaveChangesAsync();
                var subject = "Pedido Para Alteração de Conta Reprovado";
                var content = "O seu pedido foi analisado e rejeitado. O motivo foi: " + request.RejectionMotive;
                var action = "/Identity/Account/Manage";
                SendNotification(request, subject, content, action);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
        }

        /// <summary>
        /// This method show all the request that need to be analysing or aprove.
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [HttpGet]
        [Authorize(Roles = "Administrador, Moderador")]
        //  Get: Requests/ShowPendingRequest
        public async Task<IActionResult> ShowPendingRequestAsync()
        {
            int pageSize = 20;
            if (User.IsInRole("Moderador"))
            {
                var requests = _context.Request.Include(r => r.Requisitioner).Where(r => r.RequestStatus == RequestStatus.Analyzing ||
                r.RequestStatus == RequestStatus.WaitingApproval);

                //ViewBag for bell icon on the nav menu
                var userLoggedIn = await _userManager.GetUserAsync(User);
                ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

                return View("Index",await PaginatedList<Request>.CreateAsync(requests.ToList(), 1, pageSize));
            }
            else
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
        }

        /// <summary>
        /// Sends notification to the requisitioner
        /// </summary>
        /// <param name="request">Request Object</param>
        /// <param name="subject">Subject of the notification</param>
        /// <param name="content">Content of the notification</param>
        /// <param name="action">Action</param>
        private void SendNotification(Request request, string subject, string content, string action)
        {
            Notification notification = new Notification
            {
                DestinaryID = request.RequisitionerID,
                Subject = subject,
                Content = content,
                IsRead = false,
                Action = action,
                CreationDate = DateTime.Now,
            };
            
            _context.Add(notification);
            _context.SaveChanges();
        }

        /// <summary>
        /// Sends the notification to the moderator for the Add category request
        /// </summary>
        private void SendNotificationModeradores()
        {
            var subject = "Foi feito um novo pedido de adição de categoria";
            var content = "Houve um pedido de alteração de conta. Por favor responda o quanto antes.";
            List<string> ids = (from r_u in _context.UserRoles
                                join role in _context.Roles on r_u.RoleId equals role.Id
                                where role.Name == "Moderador"
                                select r_u).Select(ur => ur.UserId).ToList();
            ids.ForEach(id =>
            {
                Notification notification = new Notification
                {
                    DestinaryID = id,
                    Subject = subject,
                    Content = content,
                    IsRead = false,
                    Action = "/Requests",
                    CreationDate = DateTime.Now,
                };
                _context.Add(notification);
                _context.SaveChanges();
            });
        }
    }
}
