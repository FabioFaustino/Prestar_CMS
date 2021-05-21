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
using System.Threading;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Encodings.Web;

namespace Prestar.Controllers
{
    /// <summary>
    /// Service Requisitions Controller
    /// </summary>
    /// <see cref="ServiceRequisition"/>
    /// <seealso cref="ServiceRequisitionStatus"/>
    public class ServiceRequisitionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;

        /// <summary>
        /// Constructor of the ServiceRequisitionsController class that receives two parameters 
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
        /// <param name="emailSender">
        /// This API supports the ASP.NET Core Identity default UI infrastructure and is 
        /// not intended to be used directly from your code.
        /// <see cref="IEmailSender"/>
        /// </param>
        public ServiceRequisitionsController(ApplicationDbContext context, UserManager<User> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Clients service requisitions
        /// </summary>
        /// <param name="status">Service Requisition Status</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: ServiceRequisitions of a client (either with the role of Client, or with the role of Provider)
        [Authorize(Roles = "Prestador, Cliente")]
        public async Task<IActionResult> Index(string status)
        {
            var user = await _userManager.GetUserAsync(User);
            List<ServiceRequisition> userRequisitions = await _context.ServiceRequisition.Include(s => s.Requisitioner).Include(s => s.Service).Where(sr => sr.RequisitionerID == user.Id).OrderByDescending(s => s.CreationDate).ToListAsync();
            ViewBag.Status = new List<string>() { "Filtrar por...", "Aceite", "Cancelado", "Concluído", "Pendente", "Rejeitado" };
            userRequisitions = Filter(userRequisitions, status);

            //ViewBag for bell icon on the nav menu
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();

            var statisticsController = new StatisticsController(_context, _userManager);
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.Requisitions = statisticsController.Requisitions(_userManager.GetUserAsync(User).Result.Id);
            }

            return View(userRequisitions);
        }

        /// <summary>
        /// Providers service requisitions form a specific service
        /// </summary>
        /// <param name="id">Service identification</param>
        /// <param name="status">Service Requisition Status</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: ServiceRequisitions/GetAllServiceRequisitions/ServiceID from a specific provider's service
        [Authorize(Roles = "Prestador")]
        public async Task<IActionResult> GetServiceRequisitions(int? id, string status)
        {
            var user = await _userManager.GetUserAsync(User);
            var service = await _context.Service.Include(s => s.User).Where(s => s.ServiceID == id).FirstOrDefaultAsync();

            if (user.Id != service.UserID)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            List<ServiceRequisition> userRequisitions = await _context.ServiceRequisition.Include(s => s.Requisitioner).Include(s => s.Service).Where(sr => sr.Service.UserID == user.Id && sr.ServiceID == id).OrderByDescending(s => s.CreationDate).ToListAsync();
            ViewBag.ServiceDescription = service.Description;
            ViewBag.ServiceName = service.ServiceName;
            ViewBag.Status = new List<string>() { "Filtrar por...", "Aceite", "Cancelado", "Concluído", "Pendente", "Rejeitado" };
            userRequisitions = Filter(userRequisitions, status);

            //ViewBag for bell icon on the nav menu
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();

            return View(userRequisitions);

        }

        /// <summary>
        /// All service requisitions from a provider
        /// </summary>
        /// <param name="status">Service Requisitions Status</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: ServiceRequisitions/GetAllServiceRequisitions from all provider services 
        [Authorize(Roles = "Prestador")]
        public async Task<IActionResult> GetAllServiceRequisitions(string status)
        {
            var user = await _userManager.GetUserAsync(User);
            List<ServiceRequisition> userRequisitions = await _context.ServiceRequisition.Include(s => s.Requisitioner).Include(s => s.Service).Where(sr => sr.Service.UserID == user.Id).OrderByDescending(s => s.CreationDate).ToListAsync();
            ViewBag.Status = new List<string>() { "Filtrar por...", "Aceite", "Cancelado", "Concluído", "Pendente", "Rejeitado" };
            userRequisitions = Filter(userRequisitions, status);

            //ViewBag for bell icon on the nav menu
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();

            return View(userRequisitions);
        }

        /// <summary>
        /// Details of a service requisition
        /// </summary>
        /// <param name="id">Service Requisition identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: ServiceRequisitions/Details/5
        [Authorize(Roles = "Prestador, Cliente, Moderador, Administrador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            var serviceRequisition = await _context.ServiceRequisition.Include(s => s.Requisitioner).Include(s => s.Service).Include(s => s.Service.ServiceCategory).FirstOrDefaultAsync(m => m.ServiceRequisitionID == id);
            if (serviceRequisition == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            var service = await _context.Service.Include(s => s.User).Where(s => s.ServiceID == serviceRequisition.ServiceID).FirstOrDefaultAsync();
            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Prestador") && !User.IsInRole("Moderador") && serviceRequisition.RequisitionerID != user.Id)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            if (User.IsInRole("Prestador") && serviceRequisition.RequisitionerID != user.Id)
            {
                if (User.IsInRole("Prestador") && service.UserID != user.Id)
                {
                    return View("/Views/Shared/NotFound.cshtml");
                }
                if (serviceRequisition.ConclusionDate != null && serviceRequisition.ServiceRequisitionStatus == ServiceRequisitionStatus.Accepted)
                {
                    if (serviceRequisition.ConclusionDate.Value.Date.CompareTo(DateTime.Now.Date) <= 0)
                    {
                        return RedirectToAction(nameof(ConcludeService), new { id });
                    }
                }
            }
            ViewData["UserID"] = user.Id;
            ViewBag.ProviderID = service.User.Id;

            //ViewBag for bell icon on the nav menu
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();

            return View(serviceRequisition);
        }

        /// <summary>
        /// Creates a new service requisition
        /// </summary>
        /// <param name="id">Service identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Cliente, Prestador")]
        // GET: ServiceRequisitions/Create/1
        public async Task<IActionResult> CreateAsync(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            var service = _context.Service.Include(s => s.User).Include(s => s.ServiceCategory).Where(s => s.ServiceID == id).FirstOrDefault();
            if (service == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            ViewBag.ServiceDescription = service.ServiceName;
            ViewBag.ServiceCategory = service.ServiceCategory.Name;
            ViewBag.ServiceProvider = service.User.FirstName + " " + service.User.LastName;

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();


            return View();
        }

        /// <summary>
        /// Creates a new service requisition
        /// </summary>
        /// <param name="id">Service identification</param>
        /// <param name="serviceRequisition">Service Requisition new object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: ServiceRequisitions1/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create(int id, [Bind("ServiceRequisitionID,RequisitionerID,ServiceID,ServiceRequisitionStatus,AdditionalRequestInfo,CreationDate,LastUpdatedTime,LastUpdatedBy")] ServiceRequisition serviceRequisition)
        {
            var service = _context.Service.Include(s => s.User).Include(s => s.ServiceCategory).Where(s => s.ServiceID == id).FirstOrDefault();

            if (service == null || id != service.ServiceID)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            serviceRequisition.RequisitionerID = _userManager.GetUserAsync(User).Result.Id;
            serviceRequisition.Requisitioner = await _context.Users.Where(u => u.Id == serviceRequisition.RequisitionerID).FirstOrDefaultAsync();
            serviceRequisition.Service = service;
            serviceRequisition.ServiceRequisitionStatus = ServiceRequisitionStatus.Pending;
            serviceRequisition.CreationDate = DateTime.Now;
            serviceRequisition.LastUpdatedTime = serviceRequisition.CreationDate;
            serviceRequisition.LastUpdatedBy = serviceRequisition.Requisitioner.UserName;

            if (serviceRequisition.AdditionalRequestInfo == null)
            {
                serviceRequisition.AdditionalRequestInfo = "";
            }

            _context.Add(serviceRequisition);
            await _context.SaveChangesAsync();

            if (_context.ServiceRequisition.Contains(serviceRequisition))
            {
                SendNotificationRequisition(serviceRequisition);
                SendCreateEmail(serviceRequisition, service);
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// To edit a service requisition
        /// </summary>
        /// <param name="id">Service Requisition identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: ServiceRequisitions/Edit/5
        [Authorize(Roles = "Prestador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            var user = await _userManager.GetUserAsync(User);
            var serviceRequisition = await _context.ServiceRequisition.Include(sr => sr.Service).Include(sr => sr.Requisitioner).Where(sr => sr.ServiceRequisitionID == id).FirstOrDefaultAsync();
            if (serviceRequisition == null || user.Id != serviceRequisition.Service.UserID)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            ViewBag.User = user;
            Debug.Write(serviceRequisition.ConclusionDate);
            //ViewBag for bell icon on the nav menu
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();

            return View(serviceRequisition);
        }

        /// <summary>
        /// Edits a service requisition
        /// </summary>
        /// <param name="id">Service requisition identification</param>
        /// <param name="SubmitButton">New service requisition status</param>
        /// <param name="ConclusionDate">New service requition conclusion date</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: ServiceRequisitions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Prestador")]
        public async Task<IActionResult> Edit(int id, string SubmitButton, string ConclusionDate)
        {
            var serviceRequisition = await _context.ServiceRequisition.Include(sr => sr.Requisitioner).Include(sr => sr.Service).Where(sr => sr.ServiceRequisitionID == id).FirstOrDefaultAsync();
            string buttonClicked = SubmitButton;
            var conclusionDate = ConclusionDate;
            switch (buttonClicked)
            {
                case "Approve":
                    serviceRequisition.ServiceRequisitionStatus = ServiceRequisitionStatus.Accepted;
                    UpdateInformation(serviceRequisition, _userManager.GetUserAsync(User).Result);

                    if (conclusionDate != null && !String.IsNullOrEmpty(conclusionDate))
                    {
                        DateTime conclusionDateTime = DateTime.Parse(conclusionDate);
                        if (conclusionDateTime.Date.CompareTo(DateTime.Now.Date) >= 0 && conclusionDateTime.Date.CompareTo(DateTime.Now.AddYears(1).Date) <= 0)
                        {
                            serviceRequisition.ConclusionDate = conclusionDateTime;
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Deve adicionar uma data entre " + DateTime.Now.Date.ToString() + " e " + DateTime.Now.AddYears(1).Date.ToString();
                            return View(serviceRequisition);
                        }
                    }

                    SendNotificationStatusRequisitionUpdate(serviceRequisition, false);
                    SendEditEmail(serviceRequisition, "Aceite");
                    break;
                case "Unapprove":
                    serviceRequisition.ServiceRequisitionStatus = ServiceRequisitionStatus.Rejected;
                    SendEditEmail(serviceRequisition, "Rejeitado");
                    UpdateInformation(serviceRequisition, _userManager.GetUserAsync(User).Result);
                    SendNotificationStatusRequisitionUpdate(serviceRequisition, false);
                    break;
                case "Conclude":
                    serviceRequisition.ServiceRequisitionStatus = ServiceRequisitionStatus.Concluded;
                    SendEditEmail(serviceRequisition, "Concluído");
                    UpdateInformation(serviceRequisition, _userManager.GetUserAsync(User).Result);
                    SendNotificationStatusRequisitionUpdate(serviceRequisition, false);
                    break;
                case "CancelService":
                    serviceRequisition.ServiceRequisitionStatus = ServiceRequisitionStatus.Cancelled;
                    SendEditEmail(serviceRequisition, "Cancelado");
                    UpdateInformation(serviceRequisition, _userManager.GetUserAsync(User).Result);
                    SendNotificationStatusRequisitionUpdate(serviceRequisition, false);
                    break;
                case "EditConclusionDate":
                    if (conclusionDate != null)
                    {
                        DateTime conclusionDateTime = DateTime.Parse(conclusionDate);
                        if (conclusionDateTime.Date.CompareTo(DateTime.Now.Date) >= 0 && conclusionDateTime.Date.CompareTo(DateTime.Now.AddYears(1).Date) <= 0)
                        {
                            serviceRequisition.ConclusionDate = conclusionDateTime;
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Deve adicionar uma data entre " + DateTime.Now.Date.ToString() + " e " + DateTime.Now.AddYears(1).Date.ToString();
                            return View(serviceRequisition);
                        }
                    }
                    UpdateInformation(serviceRequisition, _userManager.GetUserAsync(User).Result);
                    SendNotificationStatusRequisitionUpdate(serviceRequisition, true);
                    break;
                case "Cancel":
                    return RedirectToAction(nameof(GetAllServiceRequisitions));
            }

            try
            {
                _context.Update(serviceRequisition);
                // update user gamification points 
                if (serviceRequisition.ServiceRequisitionStatus == ServiceRequisitionStatus.Concluded)
                    UpdatePoints(serviceRequisition);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceRequisitionExists(serviceRequisition.ServiceRequisitionID))
                {
                    return View("/Views/Shared/NotFound.cshtml");
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(GetAllServiceRequisitions));
        }

        /// <summary>
        /// To delete a service requisition (Cancel)
        /// </summary>
        /// <param name="id">Service Requisition identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: ServiceRequisitions/Delete/5 to cancel a service requisition
        [Authorize(Roles = "Prestador, Cliente")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            var serviceRequisition = await _context.ServiceRequisition.Include(s => s.Requisitioner).Include(s => s.Service).FirstOrDefaultAsync(m => m.ServiceRequisitionID == id);
            var user = await _userManager.GetUserAsync(User);
            if (serviceRequisition == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            if (User.IsInRole("Cliente") && user.Id != serviceRequisition.RequisitionerID)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            if (User.IsInRole("Prestador") && user.Id != serviceRequisition.RequisitionerID)
            {
                if (User.IsInRole("Prestador") && user.Id != serviceRequisition.Service.UserID)
                {
                    return View("/Views/Shared/NotFound.cshtml");
                }
            }

            ViewBag.UserId = user.Id;
            if (serviceRequisition.ConclusionDate != null)
            {
                ViewBag.IsPossibleToCancel = (serviceRequisition.ConclusionDate.Value.CompareTo(DateTime.Now.Date.AddDays(2)) >= 0);
            }

            //ViewBag for bell icon on the nav menu
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();


            return View(serviceRequisition);
        }

        /// <summary>
        /// Deletes a service requisition
        /// </summary>
        /// <param name="id">Service Requition identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: ServiceRequisitions/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serviceRequisition = await _context.ServiceRequisition.Include(sr => sr.Requisitioner).Include(sr => sr.Service).Where(sr => sr.ServiceRequisitionID == id).FirstOrDefaultAsync();
            serviceRequisition.ServiceRequisitionStatus = ServiceRequisitionStatus.Cancelled;
            UpdateInformation(serviceRequisition, await _userManager.GetUserAsync(User));

            try
            {
                _context.Update(serviceRequisition);
                await _context.SaveChangesAsync();
                SendNotificationStatusRequisitionUpdate(serviceRequisition, false);
                SendEditEmail(serviceRequisition, "Cancelado");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceRequisitionExists(serviceRequisition.ServiceRequisitionID))
                {
                    return View("/Views/Shared/NotFound.cshtml");
                }
                else
                {
                    throw;
                }
            }
            if (User.IsInRole("Cliente"))
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(GetAllServiceRequisitions));
        }

        /// <summary>
        /// Alerts user when a service requisition conclusion date arrives.
        /// Adds a day to conclusion date
        /// </summary>
        /// <param name="id">Requisition identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: ServiceRequisitions/ConcludeService/5 to conclude a service requisition
        [Authorize(Roles = "Prestador")]
        public async Task<IActionResult> ConcludeService(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            var serviceRequisition = await _context.ServiceRequisition.Include(s => s.Requisitioner).Include(s => s.Service).FirstOrDefaultAsync(m => m.ServiceRequisitionID == id);
            var user = await _userManager.GetUserAsync(User);


            if (serviceRequisition == null || user.Id != serviceRequisition.Service.UserID)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            if (serviceRequisition.ServiceRequisitionStatus != ServiceRequisitionStatus.Accepted)
            {
                return RedirectToAction(nameof(Details), new { id });
            }
            if (serviceRequisition.ConclusionDate == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            //ViewBag for bell icon on the nav menu
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();

            return View(serviceRequisition);
        }

        /// <summary>
        /// Alerts user when a service requisition conclusion date arrives.
        /// Adds a day to conclusion date.
        /// </summary>
        /// <param name="id">Service requisition identification</param>
        /// <param name="SubmitButton">Service Requisition new status</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: ServiceRequisitions/Delete/5
        [HttpPost, ActionName("ConcludeService")]
        [Authorize(Roles = "Prestador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConcludeService(int id, string SubmitButton)
        {
            var serviceRequisition = await _context.ServiceRequisition.FindAsync(id);
            switch (SubmitButton)
            {
                case "Conclude":
                    serviceRequisition.ServiceRequisitionStatus = ServiceRequisitionStatus.Concluded;

                    break;
                case "DoNotConclude":
                    if (serviceRequisition.ConclusionDate.Value.Date.CompareTo(DateTime.Now.Date) == 0)
                    {
                        serviceRequisition.ConclusionDate = serviceRequisition.ConclusionDate.Value.AddDays(1);
                    }
                    else
                    {
                        serviceRequisition.ConclusionDate = DateTime.Now.AddDays(1);
                    }
                    break;
                default:
                    return View("/Views/Shared/NotFound.cshtml");
            }

            UpdateInformation(serviceRequisition, await _userManager.GetUserAsync(User));

            try
            {
                _context.Update(serviceRequisition);

                if (serviceRequisition.ServiceRequisitionStatus == ServiceRequisitionStatus.Concluded)
                    // update user gamification points 
                    UpdatePoints(serviceRequisition);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceRequisitionExists(serviceRequisition.ServiceRequisitionID))
                {
                    return View("/Views/Shared/NotFound.cshtml");
                }
                else
                {
                    throw;
                }
            }

            if (SubmitButton == "Concluded")
            {
                SendEditEmail(serviceRequisition, "Concluído");
            }
            return RedirectToAction(nameof(Details), new { id });
        }



        /// <summary>
        /// Checks if a service requisition exists
        /// </summary>
        /// <param name="id">Service Requisition identification</param>
        /// <returns>bool</returns>
        private bool ServiceRequisitionExists(int id)
        {
            return _context.ServiceRequisition.Any(e => e.ServiceRequisitionID == id);
        }

        /// <summary>
        /// Filters the service requisitions by status
        /// </summary>
        /// <param name="requisitions">List of service requisitions</param>
        /// <param name="status">Service Requisition Status</param>
        /// <returns>List<ServiceRequisition></returns>
        /// <seealso cref="List{T}"/>
        private static List<ServiceRequisition> Filter(List<ServiceRequisition> requisitions, string status)
        {
            return status switch
            {
                "Aceite" => requisitions.Where(us => us.ServiceRequisitionStatus == ServiceRequisitionStatus.Accepted).ToList(),
                "Cancelado" => requisitions.Where(us => us.ServiceRequisitionStatus == ServiceRequisitionStatus.Cancelled).ToList(),
                "Concluído" => requisitions.Where(us => us.ServiceRequisitionStatus == ServiceRequisitionStatus.Concluded).ToList(),
                "Pendente" => requisitions.Where(us => us.ServiceRequisitionStatus == ServiceRequisitionStatus.Pending).ToList(),
                "Rejeitado" => requisitions.Where(us => us.ServiceRequisitionStatus == ServiceRequisitionStatus.Rejected).ToList(),
                _ => requisitions,
            };
        }

        /// <summary>
        /// Updates the service requisition information
        /// </summary>
        /// <param name="serviceRequisition">Service Requisition object</param>
        /// <param name="user">Current user</param>
        private static void UpdateInformation(ServiceRequisition serviceRequisition, User user)
        {
            serviceRequisition.LastUpdatedTime = DateTime.Now;
            serviceRequisition.LastUpdatedBy = user.UserName;
        }

        /// <summary>
        /// Sends email on service requisition creation
        /// </summary>
        /// <param name="serviceRequisition">Service Requisition object</param>
        /// <param name="service">Service object</param>
        private void SendCreateEmail(ServiceRequisition serviceRequisition, Service service)
        {
            try
            {
                var providerEmailBody = string.Format("<p><b>Recebeu um novo pedido de requisição de {0}</b></p><br><p><b>Descrição do Serviço:</b> {1}</p><p><b>Cliente:</b> {2} {3} </p>" +
                        "<p><b>Informação Adicional do Pedido:</b> {4}</p><p><b>Data de Criação do Pedido:</b> {5}</p><br><p> Consulte mais detalhes do pedido na plataforma</p><br>" +
                        "<p><b>Este pedido aguarda a sua aprovação.",
                    serviceRequisition.Requisitioner.UserName,
                    service.Description,
                    serviceRequisition.Requisitioner.FirstName,
                    serviceRequisition.Requisitioner.LastName,
                    serviceRequisition.AdditionalRequestInfo,
                    serviceRequisition.CreationDate.ToString());

                var clientEmailBody = string.Format("<p><b>O seu pedido de requisição foi submetido com sucesso!</b></p><br><p><b>Descrição do Serviço:</b> {0}</p><p><b>Prestador:</b> {1} {2} </p>" +
                        "<p><b>Informação Adicional do Pedido:</b> {3}</p><p><b>Data de Criação do Pedido:</b> {4}</p><p>Consulte mais detalhes do pedido na plataforma.</p><br>" +
                        "<p><b>Este pedido aguarda a aprovação do prestador.",
                    service.Description,
                    service.User.FirstName,
                    service.User.LastName,
                    serviceRequisition.AdditionalRequestInfo,
                    serviceRequisition.CreationDate.ToString());

                var providerEmailSubject = "Pedido de Requisição de " + serviceRequisition.Requisitioner.UserName;
                var clientEmailSubject = "Pedido de Requisição Submetido com Sucesso";

                //Send mail to provider
                _emailSender.SendEmailAsync(service.User.Email, providerEmailSubject, providerEmailBody);
                Thread.Sleep(2000);
                _emailSender.SendEmailAsync(serviceRequisition.Requisitioner.Email, clientEmailSubject, clientEmailBody);
            }
            catch (Exception e) { }
        }

        /// <summary>
        /// Sends email on service requisition update
        /// </summary>
        /// <param name="serviceRequisition">Service Requisition object</param>
        /// <param name="status">Service Object</param>
        private void SendEditEmail(ServiceRequisition serviceRequisition, string status)
        {
            try
            {
                var provider = _context.Service.Include(s => s.User).Where(s => s.ServiceID == serviceRequisition.ServiceID).FirstOrDefault();



                //Email bodies:
                var clientEmailBody = "<p>O pedido para o serviço foi {0}</p>" +
                    "<p><b>Descrição do Serviço:</b> {1}</p>" +
                    "<p><b>Prestador:</b> {2} {3} </p>" +
                    "<p><b>Informação Adicional do Pedido:</b> {4}</p>" +
                    "<p><b>Data de Criação do Pedido:</b> {5} </p>";

                var providerEmailBody = "<p>O pedido para o serviço foi alterado com sucesso</p>" +
                    "<p><b>Descrição do Serviço:</b> {0}</p>" +
                    "<p><b>Cliente:</b> {1}  </p>" +
                    "<p><b>Informação Adicional do Pedido:</b> {3}</p>" +
                    "<p><b>Data de Criação do Pedido:</b> {4} </p>";




                //Emails Subject
                var subject = "Há uma nova atualização do pedido de Requisição " + status;

                if (serviceRequisition.ConclusionDate != null)
                {
                    clientEmailBody += "<p><b>Data de Conclusão do Serviço:</b>" + serviceRequisition.ConclusionDate.Value.ToString() + " </p>";
                    providerEmailBody += "<p><b>Data de Conclusão do Serviço:</b>" + serviceRequisition.ConclusionDate.Value.ToString() + "</p>";
                }

                clientEmailBody += "Consulte mais detalhes do pedido na plataforma</a>.";
                providerEmailBody += "Consulte mais detalhes do pedido na plataforma</a>.";

                //client email
                _emailSender.SendEmailAsync(serviceRequisition.Requisitioner.Email, subject, string.Format(clientEmailBody, status, serviceRequisition.Service.Description,
                    serviceRequisition.Requisitioner.FirstName, serviceRequisition.Requisitioner.LastName, serviceRequisition.AdditionalRequestInfo,
                    serviceRequisition.CreationDate));
                Thread.Sleep(1000);
                //provider email
                _emailSender.SendEmailAsync(provider.User.Email, subject, string.Format(providerEmailBody, serviceRequisition.Service.Description, serviceRequisition.Requisitioner.FirstName,
                        serviceRequisition.Requisitioner.LastName, serviceRequisition.AdditionalRequestInfo, serviceRequisition.CreationDate.ToString()));
            }
            catch (Exception e) { }
        }


        /// <summary>
        /// This method sends the notification to the service provider alerting that a service request has been made.
        /// </summary>
        /// <param name="service">Service Requisition object</param>
        private void SendNotificationRequisition(ServiceRequisition service)
        {
            NotificationsController notificationController = new(_context, _userManager);
            Notification notification = new()
            {
                DestinaryID = service.Service.UserID,
                Subject = "Teve uma Requisição de Serviço",
                Content = "Existe um cliente interessado em requisitar o seu serviço - " + service.Service.ServiceName + " - confirme com regularidade os " +
                "contactos que disponibilizou. Quando souber o que o cliente pretende, retorne à aplicação e aprove ou reprovar o pedido para dar " +
                "seguimento ao processo da requisição do serviço e deixar o cliente seguro da sua decisão",
                IsRead = false,
                Action = "/ServiceRequisitions/Edit/" + service.ServiceRequisitionID,
                CreationDate = DateTime.Now,
            };
            _ = notificationController.Create(notification).Result;
        }

        /// <summary>
        /// This method sends the notification alerting that modifications have been made to the service request
        /// </summary>
        /// <param name="service">Service Requistion Object</param>
        /// <param name="alterDate">Checks date alteration</param>
        private async void SendNotificationStatusRequisitionUpdate(ServiceRequisition service, bool alterDate)
        {
            Notification notification = new()
            {
                Subject = "Alteração do estado do Serviço",
                CreationDate = DateTime.Now,
                IsRead = false,
                Action = "/ServiceRequisitions/Details/" + service.ServiceRequisitionID
            };
            NotificationsController notificationController = new(_context, _userManager);

            if (alterDate)
            {
                notification.DestinaryID = service.RequisitionerID;
                notification.Content = "A data do pedido foi alterada.\nPara mais informações contacte o prestador do serviço.\nNova Data: " + service.ConclusionDate;
            }
            else if (!service.ServiceRequisitionStatus.Equals(ServiceRequisitionStatus.Cancelled))
            {
                notification.DestinaryID = service.RequisitionerID;
                switch (service.ServiceRequisitionStatus)
                {
                    case ServiceRequisitionStatus.Accepted:
                        notification.Content = "O seu pedido foi Aceite.\nContinue a manter contacto com o prestador para saber como " +
                            "está o seu pedido.\nSe pretender cancelar o pedido deve fazê-lo antes que faltem 48 horas para a data de conclusão.";
                        break;
                    case ServiceRequisitionStatus.Rejected:
                        notification.Content = "O seu pedido foi Rejeitado.\nPara saber o motivo contacte o prestador que fornece este" +
                            " serviço.";
                        break;
                    case ServiceRequisitionStatus.Concluded:
                        notification.Content = "Parabéns!\nO seu pedido foi Concluido com sucesso.";
                        break;
                }
            }
            else
            {
                var user = await _userManager.GetUserAsync(User);
                if (user.Id == service.RequisitionerID)
                {
                    notification.DestinaryID = service.Service.UserID;
                    notification.Content = "O serviço foi Cancelado.\nSe não perceber o motivo contacte o cliente";
                }
                else
                {
                    notification.DestinaryID = service.RequisitionerID;
                    notification.Content = "O serviço foi Cancelado.\nSe não perceber o motivo contacte o prestador de serviço.";
                }
            }
            _ = notificationController.Create(notification).Result;
        }

        /// <summary>
        /// This method increments the user points after a service conclusion
        /// </summary>
        /// <param name="serviceRequisition">Service Requisition object</param>
        /// <returns></returns>
        private void UpdatePoints(ServiceRequisition serviceRequisition)
        {
            var gamification = _context.Gamification.Where(g => g.IsActive == true).FirstOrDefault();

            if (gamification != null)
            {
                var user = _context.Users.Where(u => u.Id == serviceRequisition.Service.UserID).FirstOrDefault();
                user.TotalPoints += gamification.PointsPerService;
                _context.Users.Update(user);
            }
            return;
        }

        /// <summary>
        /// This method allows to obtain the id of a requisition of service with the id passed with parameter.
        /// </summary>
        /// <param name="serviceID">Service identification</param>
        /// <returns></returns>
        [HttpGet]
        public int GetIdByServiceID(int serviceID)
        {
            var user = _userManager.GetUserAsync(User).Result;
            return _context.ServiceRequisition.Where(r => r.ServiceID == serviceID && r.RequisitionerID == user.Id).FirstOrDefault().ServiceRequisitionID;
        }

    }
}
