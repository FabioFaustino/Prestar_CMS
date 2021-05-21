using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prestar.Data;
using Prestar.Models;
using Prestar.Services;

namespace Prestar.Controllers
{
    /// <summary>
    /// Complaints Controller
    /// </summary>
    /// <see cref="Complaint"/>
    public class ComplaintsController : Controller
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
        public ComplaintsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// List of complaints
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: Complaints
        [Authorize(Roles = "Administrador, Moderador")]
        public async Task<IActionResult> Index(int? pageNumber)
        {
           var complaints = await _context.Complaint.Include(s => s.ComplaintTargetUser).Include(s => s.UserComplaining).OrderByDescending(s =>s.CreationDate).ToListAsync();
            int pageSize = 20;

            // Creating a new dictionary with usernames by userID
            Dictionary<string, string> usersComplaining = new();
            Dictionary<string, string> complaintTargetUsers = new();


            // Adding values to the dictionary
            foreach (Complaint complaint in complaints)
            {
                if (!usersComplaining.ContainsKey(complaint.UserComplaining.Id))
                {
                    usersComplaining.Add(complaint.UserComplaining.Id, complaint.UserComplaining.UserName);
                }
            }

            // Adding values to the dictionary
            foreach (Complaint complaint in complaints)
            {
                if (!complaintTargetUsers.ContainsKey(complaint.ComplaintTargetUser.Id))
                {
                    complaintTargetUsers.Add(complaint.ComplaintTargetUser.Id, complaint.ComplaintTargetUser.UserName);
                }
            }

            ViewBag.Userscomplaining = usersComplaining;
            ViewBag.ComplaintTargetUsers = complaintTargetUsers;

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(await PaginatedList<Complaint>.CreateAsync(complaints, pageNumber ?? 1, pageSize));
        }

        /// <summary>
        /// Details of a complaint
        /// </summary>
        /// <param name="id">Complaint identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: Complaints/Details/5
        [Authorize(Roles = "Administrador, Moderador, Prestador, Cliente")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var complaint = await _context.Complaint.Include(s => s.ComplaintTargetUser).Include(s => s.UserComplaining).Include(s => s.ComplaintTargetService).FirstOrDefaultAsync(m => m.ComplaintID == id);
            if (complaint == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var user = await _userManager.GetUserAsync(User);

            if (!User.IsInRole("Moderador"))
            {
                if (!User.IsInRole("Administrador"))
                {
                    if (complaint.ComplaintTargetUser.Id != user.Id)
                    {
                        if (complaint.UserComplaining.Id != user.Id)
                        {
                            return View("/Views/Shared/NotFound.cshtml");
                        }
                    }
                }
            }

            ViewBag.Usercomplaining = complaint.UserComplaining.UserName;
            ViewBag.ComplaintTargetUser = complaint.ComplaintTargetUser.UserName;
            ViewBag.ComplaintTarget = complaint.ComplaintTargetUser;
            if (complaint.ComplaintTargetService != null)
                ViewBag.ComplaintTargetService = complaint.ComplaintTargetService;

            //ViewBag for bell icon on the nav menu
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();


            return View(complaint);
        }

        /// <summary>
        /// To create a new complaint
        /// </summary>
        /// <param name="id">Service Requisition ID</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: Complaints/Create
        [Authorize(Roles = "Prestador, Cliente")]
        public async Task<IActionResult> CreateAsync(int id)
        {
            ViewBag.ClienteID = _context.ServiceRequisition.Where(r => r.ServiceRequisitionID == id).FirstOrDefault().RequisitionerID;

            ViewBag.ProviderID = (from serviceRequisition in _context.ServiceRequisition
                                  join service in _context.Service on serviceRequisition.ServiceID equals service.ServiceID
                                  where serviceRequisition.ServiceRequisitionID == id
                                  select service).FirstOrDefault().UserID;

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.UserID = userLoggedIn.Id;
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View();
        }

        /// <summary>
        /// Creates a new complaint
        /// </summary>
        /// <param name="complaint">New Complaint Object</param>
        /// <param name="id">Depending on type, it is a service/provider/client identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Prestador, Cliente")]
        // POST: Complaints/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ComplaintID,CreationDate,ComplaintType,Reason")] Complaint complaint, int id)
        {
            complaint.CreationDate = DateTime.Now;
            complaint.ComplaintType = complaint.ComplaintType;
            complaint.Reason = complaint.Reason;
            complaint.UserComplaining = await _userManager.GetUserAsync(User);

            //The complaint is against a ServiceProvider
            if (complaint.ComplaintType == ComplaintType.ReportServiceProvider)
            {
                var serviceID = _context.ServiceRequisition.Find(id).ServiceID;
                var service = _context.Service.Include(s => s.User).Where(s => s.ServiceID == serviceID).FirstOrDefault();
                complaint.ComplaintTargetUser = service.User;
            }
            //The complaint is against a Service
            else if (complaint.ComplaintType == ComplaintType.ReportService)
            {
                var serviceID = _context.ServiceRequisition.Find(id).ServiceID;
                var service = _context.Service.Include(s => s.User).Where(s => s.ServiceID == serviceID).FirstOrDefault();
                complaint.ComplaintTargetUser = service.User;
                complaint.ComplaintTargetService = service;
            }
            else
            {
                /* The complaint is against the client*/
                var targetUser = _context.ServiceRequisition.Include(s => s.Requisitioner).Where(s => s.ServiceRequisitionID == id).FirstOrDefault();
                complaint.ComplaintTargetUser = targetUser.Requisitioner;
            }

            if (ModelState.IsValid)
            {
                _context.Add(complaint);
                await _context.SaveChangesAsync();
                SendNotification(complaint);
                return RedirectToAction(nameof(Details), new { id = complaint.ComplaintID });
            }

            return View(complaint);
        }

        /// <summary>
        /// To edit a complaint
        /// </summary>
        /// <param name="id">Complaint identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: Complaints/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var complaint = await _context.Complaint.FindAsync(id);
            if (complaint == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(complaint);
        }

        /// <summary>
        /// Edits a complaint
        /// </summary>
        /// <param name="id">Complaint identification</param>
        /// <param name="complaint">Complaint updated object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: Complaints/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ComplaintID,CreationDate,ComplaintType,Reason")] Complaint complaint)
        {
            if (id != complaint.ComplaintID)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(complaint);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComplaintExists(complaint.ComplaintID))
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
            return View(complaint);
        }

        /// <summary>
        /// To delete a complaint
        /// </summary>
        /// <param name="id">Complaint identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: Complaints/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var complaint = await _context.Complaint.FirstOrDefaultAsync(m => m.ComplaintID == id);
            if (complaint == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(complaint);
        }

        /// <summary>
        /// Deletes a complaint
        /// </summary>
        /// <param name="id">Complaint identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: Complaints/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var complaint = await _context.Complaint.Include(c => c.ComplaintTargetUser).Include(c => c.UserComplaining).Where(c => c.ComplaintID == id).FirstOrDefaultAsync();
            if (complaint == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            _context.Complaint.Remove(complaint);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComplaintExists(int id)
        {
            return _context.Complaint.Any(e => e.ComplaintID == id);
        }

        /// <summary>
        /// This method sends the notification to the moderator's alerting that a new complaint has been created
        /// </summary>
        /// <param name="complaint">Complaint object</param>
        /// <returns>void</returns>
        private void SendNotification(Complaint complaint)
        {
            var subject = "";
            var content = "";

            NotificationsController notificationController = new NotificationsController(_context, _userManager);

            switch (complaint.ComplaintType)
            {
                case ComplaintType.ReportClient:
                    subject = "Foi feita uma denúncia a um CLIENTE";
                    content = "Foi feito pelo utilizador com o email eletrónico " + complaint.UserComplaining.Email + " uma denuncia sobre um " +
                        "cliente que não procedeu de acordo com as normas da plataforma";
                    break;

                case ComplaintType.ReportService:
                    subject = "Foi feita uma denúncia a um SERVIÇO";
                    content = "Foi feito pelo utilizador com o email eletrónico " + complaint.UserComplaining.Email + " uma denuncia sobre um " +
                        "serviço que não está de acordo com as normas da plataforma";
                    break;

                case ComplaintType.ReportServiceProvider:
                    subject = "Foi feita uma denúncia a um PRESTADOR DE SERVIÇO";
                    content = "Foi feito pelo utilizador com o email eletrónico " + complaint.UserComplaining.Email + " uma denuncia sobre um " +
                        "prestador de serviço que não procedeu de acordo com as normas da plataforma";
                    break;
            }

            List<string> ids = (from r_u in _context.UserRoles
                                join role in _context.Roles on r_u.RoleId equals role.Id
                                where role.Name == "Moderador" || role.Name == "Administrador"
                                select r_u).Select(ur => ur.UserId).ToList();
            ids.ForEach(id =>
            {
                Notification notification = new Notification
                {
                    DestinaryID = id,
                    Subject = subject,
                    Content = content,
                    IsRead = false,
                    Action = "/Complaints/Details/" + complaint.ComplaintID,
                    CreationDate = DateTime.Now,
                };
                var result = notificationController.Create(notification).Result;
            });
        }

        /// <summary>
        /// This method resolves the complaint with no action
        /// </summary>
        /// <param name="id">Complaint identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveComplaintNoAction(int id)
        {
            var complaint = await _context.Complaint.FindAsync(id);

            complaint.IsSolved = true;
            complaint.Resolution = "Queixa fechada sem ação";
            complaint.ResolutionDate = DateTime.Now;
            complaint.ResolvedBy = await _userManager.GetUserAsync(User);


            if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(complaint);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ComplaintExists(complaint.ComplaintID))
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
                return View(complaint);
            }
    }
}
