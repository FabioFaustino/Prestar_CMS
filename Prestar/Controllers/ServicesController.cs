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
using Microsoft.EntityFrameworkCore;
using Prestar.Data;
using Prestar.Models;
using Prestar.Services;

namespace Prestar.Controllers
{
    /// <summary>
    /// Services Controller
    /// </summary>
    /// <see cref="Service"/>
    /// <seealso cref="Complaint"/>
    /// <seealso cref="ComplaintsController"/>
    /// <seealso cref="Notification"/>
    /// <seealso cref="NotificationsController"/>
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Constructor of the ServicesController class that receives two parameters 
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
        public ServicesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
           
        }

        /// <summary>
        /// This method shows all the services in the database
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <param name="order">Service Evaluation order</param>
        /// <param name="page">Services page number</param>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [AllowAnonymous]
        // GET: Services
        public async Task<IActionResult> Index(string order, int? pageNumber)
        {
            int pageSize = 20;
            var user = await _userManager.GetUserAsync(User);
           
            if (user != null)
            {
                ViewData["User"] = user.UserName;
            }

            // Only displays services that are active and not blocked, and from unblocked users

            var services = _context.Service
                .Include(s => s.User)
                .Include(s => s.ServiceCategory)
                .Include(s => s.CommentsAndEvaluations)
                .AsEnumerable()
                .Where(s => s.IsActive == true && s.IsBlocked == false && (s.User.LockoutEnd <= DateTimeOffset.Now || s.User.LockoutEnd == null) && s.User.HasRemovedAccount == false)
                .ToList();

            ViewBag.OrderBy = new List<string>() { "Ordenar Classificação...", "Ascendente", "Descendente" };
            services = Sort(services, order);

            // Creating a new dictionary with usernames by userID
            Dictionary<string, User> providers = new();

            // Average evaluation
            Dictionary<int, double> averageEvaluations = new();
            // Adding values to the dictionary
            foreach (Service service in services)
            {
                if (!providers.ContainsKey(service.UserID))
                {
                    providers.Add(service.UserID, service.User);
                }

                if (!averageEvaluations.ContainsKey(service.ServiceID))
                    averageEvaluations.Add(service.ServiceID, GetAverageEvaluation(service));
            }

            ViewBag.ProviderName = providers;
            ViewBag.AverageEvaluations = averageEvaluations;

            //ViewBag for bell icon on the nav menu
            if (User.Identity.IsAuthenticated)
            {
                var userLoggedIn = await _userManager.GetUserAsync(User);
                ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();
            }

            return View( await PaginatedList<Service>.CreateAsync(services, pageNumber ?? 1, pageSize));
        }


        /// <summary>
        /// This method returns all services of a category wich id is passed as parameter
        /// </summary>
        /// <param name="id">Service Category identification</param>
        /// <param name="order">Order results </param>
        /// <returns>View with all services of a category</returns>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [AllowAnonymous]
        // GET: Services/5
        // View to filter the services by category
        public async Task<IActionResult> IndexByCategory(int? id, string order, int? pageNumber)
        {
            int pageSize = 20;

            if (_context.ServiceCategory.Find(id) == null)
            {
                //return NotFound();
                return View("/Views/Shared/NotFound.cshtml");
            }
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ViewData["User"] = user.UserName;
            }

            // Only displays services from chosen category that are active and not blocked
            List<Service> services = _context.Service.Include(s => s.User).Include(s => s.ServiceCategory).Include(s => s.CommentsAndEvaluations).AsEnumerable()
               .Where(s=> s.ServiceCategoryID == id && s.IsActive == true && s.IsBlocked == false && (s.User.LockoutEnd <= DateTimeOffset.Now || s.User.LockoutEnd == null) && s.User.HasRemovedAccount == false)
               .ToList();

            ServiceCategory serviceCategory = await _context.ServiceCategory.Include(s => s.ServiceCategories).Where(s => s.CategoryID == id).FirstOrDefaultAsync();
            foreach(ServiceCategory subcategory in serviceCategory.ServiceCategories)
            {
                List<Service> newServices = _context.Service.Include(s => s.User).Include(s => s.ServiceCategory).Include(s => s.CommentsAndEvaluations).AsEnumerable()
               .Where(s => s.ServiceCategoryID == subcategory.CategoryID && s.IsActive == true && s.IsBlocked == false && (s.User.LockoutEnd <= DateTimeOffset.Now || s.User.LockoutEnd == null) && s.User.HasRemovedAccount == false)
               .ToList();

                foreach(Service newService in newServices)
                {
                    services.Add(newService);
                }
            }

            ViewBag.OrderBy = new List<string>() { "Ordenar Classificação...", "Ascendente", "Descendente" };
            services = Sort(services, order);

            // Creating a new dictionary with usernames by userID
            Dictionary<string, User> providers = new();

            // Average evaluation
            Dictionary<int, double> averageEvaluations = new();

            // Adding values to the dictionary
            foreach (Service service in services)
            {
                if (!providers.ContainsKey(service.UserID))
                {
                    providers.Add(service.UserID, service.User);
                }

                if (!averageEvaluations.ContainsKey(service.ServiceID))
                {
                    averageEvaluations.Add(service.ServiceID, GetAverageEvaluation(service));
                }
            }

            ViewBag.ProviderName = providers;
            ViewData["CategoryID"] = id;
            ViewBag.Subcategories = serviceCategory.ServiceCategories;
            ViewData["CategoryName"] = await _context.ServiceCategory.FirstOrDefaultAsync(m => m.CategoryID == id);
            ViewBag.AverageEvaluations = averageEvaluations;

            //ViewBag for bell icon on the nav menu
            if (User.Identity.IsAuthenticated)
            {
                var userLoggedIn = await _userManager.GetUserAsync(User);
                ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();
            }

            return View(await PaginatedList<Service>.CreateAsync(services, pageNumber ?? 1, pageSize));
        }


        /// <summary>
        /// This method shows all the services of a 'Provider' user 
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Prestador")]
        // GET: Services/GetUserServices
        public async Task<IActionResult> UserServices(int? pageNumber)
        {
            int pageSize = 10;

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var services = await _context.Service.Where(s => s.UserID == user.Id).Include(s => s.ServiceCategory).ToListAsync();

            // Average evaluation
            Dictionary<int, bool> hasRequisitions = new();

            // Adding values to the dictionary
            foreach (Service service in services)
            {
                if (!hasRequisitions.ContainsKey(service.ServiceID))
                {

                    var requisitions = await _context.ServiceRequisition.Where(sr => sr.ServiceID == service.ServiceID).ToListAsync();
                    if (requisitions.Count > 0)
                    {
                        hasRequisitions.Add(service.ServiceID, true);
                    }
                    else
                    {
                        hasRequisitions.Add(service.ServiceID, false);
                    }
                }
            }
            

            //ViewBag for bell icon on the nav menu
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();

            return View(await PaginatedList<Service>.CreateAsync(services, pageNumber ?? 1, pageSize));
        }

        /// <summary>
        /// This method shows all the services of a 'Provider' user 
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [AllowAnonymous]
        // GET: Services/GetUserServices
        public async Task<IActionResult> ProviderServices(string id, string order, int? pageNumber)
        {
            //var user = await _context.User;
            //if (user == null)
            //{
            //    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            //}

            int pageSize = 20;
            List<Service> services = _context.Service
                .Include(s => s.User)
                .Include(s => s.ServiceCategory)
                .Include(s => s.CommentsAndEvaluations)
                .AsEnumerable()
                .Where(s => s.UserID == id && s.IsActive == true && s.IsBlocked == false && (s.User.LockoutEnd <= DateTimeOffset.Now || s.User.LockoutEnd == null) &&  !s.User.HasRemovedAccount)
                .ToList();

            ViewBag.OrderBy = new List<string>() { "Ordenar por...", "Ascendente", "Descendente" };
            services = Sort(services, order);

            // Creating a new dictionary with usernames by userID
            Dictionary<string, User> providers = new();

            // Average evaluation
            Dictionary<int, double> averageEvaluations = new();
            // Adding values to the dictionary
            foreach (Service service in services)
            {
                if (!providers.ContainsKey(service.UserID))
                {
                    providers.Add(service.UserID, service.User);
                }

                if (!averageEvaluations.ContainsKey(service.ServiceID))
                    averageEvaluations.Add(service.ServiceID, GetAverageEvaluation(service));
            }

            ViewBag.ProviderName = providers[id].FirstName +" "+ providers[id].LastName;
            ViewBag.AverageEvaluations = averageEvaluations;

            //ViewBag for bell icon on the nav menu
            var user = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();
            ViewBag.ProviderIdentification = id;
            //return RedirectToAction(nameof(Index), services);
            return View(await PaginatedList<Service>.CreateAsync(services, pageNumber ?? 1, pageSize));
        }

        /// <summary>
        /// Shows the details of the service with the category passed by parameter
        /// </summary>
        /// <param name="id">Service identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: Services/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                //return NotFound();
                return View("/Views/Shared/NotFound.cshtml");
            }


            var service = await _context.Service.Include(u => u.User).Include(s => s.ServiceCategory).FirstOrDefaultAsync(m => m.ServiceID == id);
            if (service == null)
            {
                //return NotFound();
                return View("/Views/Shared/NotFound.cshtml");
            }

            if(service.ServiceImages == null)
            {
                var images = new List<ServiceImage>();
                images = await _context.ServiceImage.Where(si => si.ServiceID == id).ToListAsync();

                if(images != null)
                {
                    service.ServiceImages = images;
                    _context.Service.Update(service);
                    await _context.SaveChangesAsync();
                }            
                    
            }

            ViewData["RequisitionerID"] = _userManager.GetUserAsync(User).Result.Id;
            ViewData["UserName"] = service.User.UserName;
            ViewData["CommentsAndEvaluation"] = _context.Service.Include(sr => sr.CommentsAndEvaluations).Include(sr => sr.User).ToList();

            // For comments Section
            //Hide comments removed by provider
            List<CommentAndEvaluation> comments = _context.CommentAndEvaluation
                .Include(c => c.Service)
                .Include(c => c.UserCommenting)
                .Where(c => c.ServiceID == service.ServiceID && c.IsRemoved == false)
                .OrderByDescending(c => c.CreationDate)
                .ToList();

            ViewBag.UserID = _userManager.GetUserAsync(User).Result.Id;
            ViewBag.Comments = comments;
            ViewBag.NumberComments = comments.Count();
            ViewBag.AverageEvaluation = GetAverageEvaluation(service);
            ViewBag.ServiceID = service.ServiceID;
            ViewBag.RequisitionerID = _userManager.GetUserAsync(User).Result.Id;

            //Creating a dictionary for usernames

            Dictionary<string, string> usernames = new();

            // Adding values to the dictionary

            foreach (CommentAndEvaluation comment in comments)
            {
                if (!usernames.ContainsKey(comment.UserCommenting.Id))
                {
                    usernames.Add(comment.UserCommenting.Id, comment.UserCommenting.UserName);
                }
            }

            ViewBag.Usernames = usernames;

            //Creating a dictionary for profile pictures

            Dictionary<string, byte[]> profilePictures = new();

            // Adding values to the dictionary

            foreach (CommentAndEvaluation comment in comments)
            {
                if (!profilePictures.ContainsKey(comment.UserCommenting.Id))
                {
                    profilePictures.Add(comment.UserCommenting.Id, comment.UserCommenting.ProfilePicture);
                }
            }

            ViewBag.ProfilePictures = profilePictures;
            ViewBag.ProviderID = service.UserID;


            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();


            return View(service);
        }


        /// <summary>
        /// This method shows the form to create a new service.
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Prestador")]
        // GET: Services/Create
        public async Task<IActionResult> Create(bool? send)
        {
            var categories = await _context.ServiceCategory.OrderBy(sc => sc.Name).ToListAsync();
            ViewBag.ServiceCategories = categories;

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            if (send != null && send == true)
            {
                ViewBag.Send = true;
            }
            else
            {
                ViewBag.Send = false;
            }
            return View();
        }

        /// <summary>
        /// This method allows to create a new service and add to the database
        /// </summary>
        /// <param name="service">Service new object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Prestador")]
        // POST: Services/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("ServiceID,ServiceName,RequisitionerID,ServiceCategoryID,Description,Illustration,CreationDate")] Service service)
        {
            service.User = await _userManager.GetUserAsync(User);
            service.UserID = service.User.Id;
            service.ServiceCategory = _context.ServiceCategory.Where(sc => sc.CategoryID == service.ServiceCategoryID).FirstOrDefault();
            service.ServiceCategoryID = service.ServiceCategory.CategoryID;
            service.CreationDate = DateTime.Now;
            service.IsActive = true;
            service.ServiceImages = new List<ServiceImage>();


            try
            {
                if (Request.Form.Files.Count > 0)
                {
                    IFormFile file = Request.Form.Files.FirstOrDefault();
                    using var dataStream = new MemoryStream();
                    await file.CopyToAsync(dataStream);
                    service.Illustration = dataStream.ToArray();
                }
            }
            catch (InvalidOperationException) { }

            if (ModelState.IsValid)
            {
                _context.Add(service);
                await _context.SaveChangesAsync();
                SendNotification(service);
                return RedirectToAction(nameof(UserServices));
            }

            var categories = await _context.ServiceCategory.OrderBy(sc => sc.Name).ToListAsync();
            ViewBag.ServiceCategories = categories;

            return View(service);
        }

        /// <summary>
        /// This method shows the form to edit the service with the id passed by paramenter
        /// </summary>
        /// <param name="id">Service identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Prestador,Administrador, Moderador")]
        // GET: Services/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var service = await _context.Service.Include(s => s.User).Include(s => s.ServiceCategory).Where(s => s.ServiceID == id).FirstOrDefaultAsync();
            var user = await _userManager.GetUserAsync(User);
            if (service == null || (User.IsInRole("Prestador") && user.Id != service.User.Id))
            {
                return NotFound();
            }

            ViewBag.ServiceCategories = await _context.ServiceCategory.Include(sc => sc.ServiceCategories).OrderBy(sc => sc.Name).ToListAsync();

            //ViewBag for bell icon on the nav menu
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();

            return View(service);
        }

        /// <summary>
        /// This method allows to update the service with the id passed by parameter.
        /// </summary>
        /// <param name="id">Service identification</param>
        /// <param name="service">Service updated object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Prestador,Administrador, Moderador")]
        // POST: Services/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("ServiceID,ServiceName,RequisitionerID,ServiceCategoryID,Description,Illustration,CreationDate,ServiceImages")] Service service)
        {
            if (id != service.ServiceID)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            service.User = await _userManager.GetUserAsync(User);
            service.UserID = service.User.Id;
            service.ServiceCategory = _context.ServiceCategory.Where(sc => sc.CategoryID == service.ServiceCategoryID).FirstOrDefault();            

            try
            {
                if (Request.Form.Files.Count > 0)
                {
                    IFormFile file = Request.Form.Files.FirstOrDefault();
                    using var dataStream = new MemoryStream();
                    await file.CopyToAsync(dataStream);
                    service.Illustration = dataStream.ToArray();
                }
            }
            catch (InvalidOperationException) { }

            if (service.Illustration == null)
            {
                byte[] illustration = _context.Service.Where(n => n.ServiceID == service.ServiceID).Select(n => n.Illustration).FirstOrDefault();
                service.Illustration = illustration;
            }

            service.IsActive = _context.Service.Where(n => n.ServiceID == service.ServiceID).Select(n => n.IsActive).FirstOrDefault();
            service.IsBlocked = _context.Service.Where(n => n.ServiceID == service.ServiceID).Select(n => n.IsBlocked).FirstOrDefault();


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.ServiceID))
                    {
                        return View("/Views/Shared/NotFound.cshtml");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(UserServices));
            }
            return View(service);
        }

        /// <summary>
        /// This method allows you to show the service you want to delete.
        /// </summary>
        /// <param name="id">Service identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Prestador, Administrador, Moderador")]
        // GET: Services/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            ViewBag.CanDelete = true;

            var service = await _context.Service.Include(u => u.User).Include(c => c.ServiceCategory).FirstOrDefaultAsync(m => m.ServiceID == id);
            var user = await _userManager.GetUserAsync(User);
            if (service == null || (User.IsInRole("Prestador") && user.Id != service.User.Id))
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var requisitions = await _context.ServiceRequisition.Where(sr => sr.ServiceID == service.ServiceID && sr.ServiceRequisitionStatus == ServiceRequisitionStatus.Accepted).ToListAsync();          
            var complaints = await _context.Complaint.Where(c => c.ComplaintTargetService.ServiceID == id).ToListAsync();
            
            if (complaints.Count != 0 || requisitions.Count != 0)
                ViewBag.CanDelete = false;
           


            ViewData["UserName"] = service.User.UserName;

            //ViewBag for bell icon on the nav menu
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();


            return View(service);
        }


        /// <summary>
        /// This method actualy deletes the service with the id passed by parameter from the database.
        /// </summary>
        /// <param name="id">Service identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Prestador, Administrador, Moderador")]
        // POST: Services/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Service.FindAsync(id);
            if (service == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            _context.Service.Remove(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(UserServices));
        }

        /// <summary>
        /// This method checks the existence of a service in the database according to the id passed by the argument.
        /// </summary>
        /// <param name="id">Service identification</param>
        /// <returns>bool</returns>
        private bool ServiceExists(int id)
        {
            return _context.Service.Any(e => e.ServiceID == id);
        }

        /// <summary>
        /// This method sorts the services by rating depending on the order.
        /// </summary>
        /// <param name="services">List of services to order</param>
        /// <param name="order">Order criteria</param>
        /// <returns>A list of services sorted by rating</returns>
        public List<Service> Sort(List<Service> services, string order)
        {
            // Average evaluation
            Dictionary<int, double> averageEvaluations = new();

            // Adding values to the dictionary
            foreach (Service service in services)
            {
                if (!averageEvaluations.ContainsKey(service.ServiceID))
                    averageEvaluations.Add(service.ServiceID, GetAverageEvaluation(service));
            }

            var OrderBy = averageEvaluations.OrderBy(s => s.Value).ToList();
            var OrderByDescending = averageEvaluations.OrderByDescending(s => s.Value).ToList();

            //Services order by evaluation
            List<Service> servicesOrderBy = new();

            foreach (KeyValuePair<int, double> service in OrderBy)
            {
                if (ServiceExists(service.Key))
                {
                    servicesOrderBy.Add(services.Where(s => s.ServiceID == service.Key).FirstOrDefault());
                }
            }

            //Services order by descending evaluation
            List<Service> servicesOrderByDescending = new();

            foreach (KeyValuePair<int, double> service in OrderByDescending)
            {
                if (ServiceExists(service.Key))
                {
                    servicesOrderByDescending.Add(services.Where(s => s.ServiceID == service.Key).FirstOrDefault());
                }
            }

            return order switch
            {
                "Ascendente" => servicesOrderBy,
                "Descendente" => servicesOrderByDescending,
                _ => services,
            };
        }


        /// <summary>
        /// This method calculates the average evaluation of a service
        /// </summary>
        /// <param name="service">Service object</param>
        /// <returns>Average Evaluation.</returns>
        public double GetAverageEvaluation(Service service)
        {
            List<CommentAndEvaluation> comments = _context.CommentAndEvaluation
                .Include(c => c.Service)
                .Include(c => c.UserCommenting)
                .Where(c => c.ServiceID == service.ServiceID)
                .ToList();
            if (comments == null)
                return 0;

            int total = 0;
            int evaluations = 0;
            foreach (var comment in comments)
            {
                if ( comment.Evaluation != 0)
                {
                    total += comment.Evaluation;
                    evaluations++;
                }

            }

            if (total > 0 && evaluations > 0)
                return Math.Round((double)total / evaluations, 1);

            return 0;

        }

        /// <summary>
        /// Since there shouldn't be deleted services, in case a service is by any means not wanted on the application
        /// it should be blocked. Making it invisible to other users, but it still persists on the database.
        /// There should allways be provided a reason for blocking a service.
        /// </summary>
        /// <param name="id">Service identification</param>
        /// <param name ="complaintID">Complaint identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Moderador,Administrador")]
        // GET: Services/Block/5?complaintID=31
        public async Task<IActionResult> Block(int? id, int? complaintID)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var service = await _context.Service.Include(u => u.User).Include(c => c.ServiceCategory).FirstOrDefaultAsync(m => m.ServiceID == id);
            var user = await _userManager.GetUserAsync(User);

            if (service == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            ViewBag.ComplaintID = complaintID;
            ViewData["UserName"] = service.User.UserName;


            //ViewBag for bell icon on the nav menu
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();


            return View(service);
        }

        /// <summary>
        /// This method Confirms the block of a service
        /// </summary>
        /// <param name="id">Service identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Moderador,Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockConfirmed(int id, int complaintID, String blockMotive)
        {
            
            Service service = await _context.Service.FindAsync(id);
            Complaint complaint = await _context.Complaint.FindAsync(complaintID);

            if (service == null || complaint == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            //update service params
            service.IsBlocked = true;
            service.BlockMotive = blockMotive;
            
            //Update complaints
            complaint.IsSolved = true;
            complaint.Resolution = "Serviço Bloqueado";
            complaint.ResolutionDate = DateTime.Now;
            complaint.ResolvedBy = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    _context.Update(complaint);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.ServiceID))
                    {
                        return View("/Views/Shared/NotFound.cshtml");
                    }
                    else
                    {
                        throw;
                    } 
                }
                SendBlockNotification(complaint, blockMotive, true);
                return RedirectToAction("Details","Complaints", new { id = complaint.ComplaintID });
            }
            return View(service);
        }

        /// <summary>
        /// This method Confirms the unblocking of a service
        /// </summary>
        /// <param name="id">Service identification</param>
        /// <param name="complaintID">Complaint identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Moderador,Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockConfirmed(int id,int complaintID)
        {
            Service service = await _context.Service.FindAsync(id);
            Complaint complaint = await _context.Complaint.FindAsync(complaintID);

            if (service == null || complaint == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            //update service params
            service.IsBlocked = false;
            service.BlockMotive = "";

            //Update complaints
            complaint.IsSolved = true;
            complaint.Resolution = "Serviço Desbloqueado";
            complaint.ResolutionDate = DateTime.Now;
            complaint.ResolvedBy = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    _context.Update(complaint);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.ServiceID))
                    {
                        return View("/Views/Shared/NotFound.cshtml");
                    }
                    else
                    {
                        throw;
                    }
                }
                SendBlockNotification(complaint, "", false);
                return RedirectToAction("Details", "Complaints", new { id = complaint.ComplaintID });
            }
            return View(service);
        }

        /// <summary>
        /// This method shows the service the user is trying to activate
        /// </summary>
        /// <param name="id">Service identifciation</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Prestador")]
        // GET: Services/Activate/5
        public async Task<IActionResult> Activate(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var service = await _context.Service.Include(u => u.User).Include(s => s.ServiceCategory).FirstOrDefaultAsync(m => m.ServiceID == id);
            var user = await _userManager.GetUserAsync(User);

            if (service == null || (User.IsInRole("Prestador") && user.Id != service.User.Id))
            {
                return View("/Views/Shared/NotFound.cshtml");
            }


            ViewData["UserName"] = service.User.UserName;

            //ViewBag for bell icon on the nav menu
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();


            return View(service);
        }

        /// <summary>
        /// This method Confirms the activation of a service
        /// </summary>
        /// <param name="id">Service identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Prestador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateConfirmed(int id)
        {
            Service service = await _context.Service.FindAsync(id);

            if (service == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            service.IsActive = true;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.ServiceID))
                    {
                        return View("/Views/Shared/NotFound.cshtml");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(UserServices));
            }
            return View(service);
        }



        /// <summary>
        /// Since there shouldn't be any services deleted from the application, this method deactivates a service,
        /// making it invisible to other users.
        /// </summary>
        /// <param name="id">Service identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Prestador")]
        // GET: Services/Deactivate/5
        public async Task<IActionResult> Deactivate(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var service = await _context.Service.Include(u => u.User).Include(s => s.ServiceCategory).FirstOrDefaultAsync(m => m.ServiceID == id);
            var user = await _userManager.GetUserAsync(User);

            if (service == null || (User.IsInRole("Prestador") && user.Id != service.User.Id))
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var requisitions = await _context.ServiceRequisition.Where(sr => sr.ServiceID == service.ServiceID && sr.ServiceRequisitionStatus == ServiceRequisitionStatus.Accepted).ToListAsync();

            if (requisitions.Count > 0)
            {
                ViewBag.HasActiveRequisitions = true;
            }
            else
            {
                ViewBag.HasActiveRequisitions = false;
            }


            //ViewBag for bell icon on the nav menu
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();


            ViewData["UserName"] = service.User.UserName;

            return View(service);
        }


        /// <summary>
        /// This method Confirms the deactivation of a service
        /// </summary>
        /// <param name="id">Service identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Prestador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateConfirmed(int id)
        {
            Service service = await _context.Service.FindAsync(id);

            if (service == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            service.IsActive = false;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.ServiceID))
                    {
                        return View("/Views/Shared/NotFound.cshtml");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(UserServices));
            }
            return View(service);
        }



        /// <summary>
        /// This method sends the notification to the moderator's alerting that a new service has been created
        /// </summary>
        /// <param name="service">Service object</param>
        /// <returns>void</returns>
        private void SendNotification(Service service)
        {
            NotificationsController notificationController = new NotificationsController(_context, _userManager);

            List<string> ids = (from r_u in _context.UserRoles
                                join role in _context.Roles on r_u.RoleId equals role.Id
                                where role.Name == "Moderador"
                                select r_u).Select(ur => ur.UserId).ToList();
            ids.ForEach(id =>
            {
                Notification notification = new Notification
                {
                    DestinaryID = id,
                    Subject = "Criação de um novo serviço",
                    Content = "Foi criado um serviço novo na plataforma! Por favor confira que este respeita as normas da plataforma. Entre em contacto" +
                    " com o administrador se achar alguma incoerência.",
                    IsRead = false,
                    Action = "/Services/Details/" + service.ServiceID,
                    CreationDate = DateTime.Now,
                };
                var result = notificationController.Create(notification).Result;
            });
        }

        /// <summary>
        /// This method sends the notification to the user alerting that his service has been Blocked
        /// </summary>
        /// <param name="blockMotive">Block motive text</param>
        /// <param name="complaint">Complaint object</param>
        /// <param name="IsBlock">Checks if the action is to block</param>
        /// <returns>void</returns>
        private void SendBlockNotification(Complaint complaint, String blockMotive, bool IsBlock)
        {
            String action = "";
            String content = "O seu serviço foi ";

            if (IsBlock) 
            {
                action = "bloqueado ";
                content += action + "pelo seguinte motivo: " + blockMotive;
            }
            else
            {
                action = "desbloqueado ";
                content += action;
            }

            NotificationsController notificationController = new NotificationsController(_context, _userManager);

            Notification notification = new Notification
                {
                    DestinaryID = complaint.ComplaintTargetService.UserID,
                    Subject = "O seu serviço - " + complaint.ComplaintTargetService.ServiceName + " - foi " + action,
                    Content = content,
                    IsRead = false,
                    Action = "/Complaints/Details/"+ complaint.ComplaintID,
                    CreationDate = DateTime.Now,
                };
                var result = notificationController.Create(notification).Result;
           
        }

        /// <summary>
        /// This method returns the id of a service with the title passed by parameter.
        /// </summary>
        /// <param name="title">Service title</param>
        /// <returns>int</returns>
        [HttpGet]
        public int GetIdByTitle(string title)
        {
            return _context.Service.Where(s => s.ServiceName == title).FirstOrDefault().ServiceID;
        }
    }
}
