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

namespace Prestar.Controllers
{
    /// <summary>
    /// Service Categories Controller
    /// </summary>
    /// <see cref="ServiceCategory"/>
    public class ServiceCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Constructor of the ServiceCategoriesController class that receives two parameters 
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
        public ServiceCategoriesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Gets all service categories and subcategories
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Administrador, Moderador")]
        // GET: ServiceCategories
        public async Task<IActionResult> Index()
        {
            IEnumerable<ServiceCategory> categories = await _context.ServiceCategory.Include(sc => sc.ServiceCategories).OrderBy(sc => sc.Name).ToListAsync();

            // Creating a new dictionary with subcategories by category
            Dictionary<int, string> subcategories = new();

            // Adding values to the dictionary
            foreach (ServiceCategory category in categories)
            {
                if (!category.IsSubcategory && category.ServiceCategories.Count!=0)
                {
                    foreach(ServiceCategory subcategory in category.ServiceCategories)
                    {
                        if (!subcategories.ContainsKey(subcategory.CategoryID))
                        {
                            subcategories.Add(subcategory.CategoryID, category.Name);
                        }
                    }
                }
            }

            ViewBag.Subcategories = subcategories;

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(categories);
        }

        /// <summary>
        /// Gets the details of a service category
        /// </summary>
        /// <param name="id">Service Category identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Administrador, Moderador")]
        // GET: ServiceCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var serviceCategory = await _context.ServiceCategory.Include(sc => sc.ServiceCategories).FirstOrDefaultAsync(m => m.CategoryID == id);
            if (serviceCategory == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            ViewBag.NumberOfServices = _context.Service.Where(s => s.ServiceCategoryID == id).ToList().Count;

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(serviceCategory);
        }

        /// <summary>
        /// To create a new service category
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Administrador, Moderador")]
        // GET: ServiceCategories/Create
        public async Task<IActionResult> CreateAsync()
        {
            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View();
        }

        /// <summary>
        /// Creates a new service category
        /// </summary>
        /// <param name="serviceCategory">Service Category new object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: ServiceCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Administrador, Moderador")]
        public async Task<IActionResult> Create([Bind("CategoryID,Name,Illustration")] ServiceCategory serviceCategory)
        {
            try
            {
                if (Request != null && Request.Form.Files.Count > 0)
                {
                    IFormFile file = Request.Form.Files.FirstOrDefault();
                    using var dataStream = new MemoryStream();
                    await file.CopyToAsync(dataStream);
                    serviceCategory.Illustration = dataStream.ToArray();
                }
            }
            catch (InvalidOperationException) { }
         

            if (ModelState.IsValid)
            {
                serviceCategory.ServiceCategories = new List<ServiceCategory>();
                serviceCategory.IsSubcategory = false;
                _context.Add(serviceCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(serviceCategory);
        }

        /// <summary>
        /// This method show the form to create a new service subcategory
        /// </summary>
        /// <param name="id">Main Service Category identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [HttpGet]
        [Authorize(Roles = "Administrador, Moderador")]
        // GET: ServiceCategories/CreateSubcategory/5
        public async Task<IActionResult> CreateSubcategoryAsync(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var serviceCategory = _context.ServiceCategory.Where(sc => sc.CategoryID == id && sc.IsSubcategory == false).FirstOrDefault();
            if (serviceCategory == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            ViewBag.Category = serviceCategory;

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View();
        }

        /// <summary>
        /// This method allows the creation of a new service subcategory and add to the database
        /// </summary>
        /// <param name="id">Main service category identification</param>
        /// <param name="serviceSubcategory">Service Subcategory new object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: ServiceCategories/CreateSubcategory/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Administrador, Moderador")]
        public async Task<IActionResult> CreateSubcategory(int id, [Bind("CategoryID,Name,Illustration")] ServiceCategory serviceSubcategory)
        {
            //Adding new subcategory to category's list
            var serviceCategory = _context.ServiceCategory.Where(sc => sc.CategoryID == id && sc.IsSubcategory == false).FirstOrDefault();
            if (serviceCategory == null || id != serviceCategory.CategoryID)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
            try
            {
                if (Request != null && Request.Form.Files.Count > 0)
                {
                    IFormFile file = Request.Form.Files.FirstOrDefault();
                    using var dataStream = new MemoryStream();
                    await file.CopyToAsync(dataStream);
                    serviceSubcategory.Illustration = dataStream.ToArray();
                }
            }
            catch (InvalidOperationException) { }


            if (ModelState.IsValid)
            {
                
                serviceSubcategory.IsSubcategory = true;
                _context.Add(serviceSubcategory);
                await _context.SaveChangesAsync();

                if(serviceCategory.ServiceCategories == null)
                {
                    serviceCategory.ServiceCategories = new List<ServiceCategory> { serviceSubcategory };
                }
                else
                {
                    serviceCategory.ServiceCategories.Add(serviceSubcategory);
                }

                try
                {
                    _context.Update(serviceCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceCategoryExists(serviceCategory.CategoryID))
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
            return View(serviceSubcategory);
        }

        /// <summary>
        /// To edit a service category
        /// </summary>
        /// <param name="id">Category identification</param>
        /// <returns></returns>
        [Authorize(Roles = "Administrador, Moderador")]
        // GET: ServiceCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var serviceCategory = await _context.ServiceCategory.FindAsync(id);
            if (serviceCategory == null || serviceCategory.IsSubcategory)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(serviceCategory);
        }

        /// <summary>
        /// Edits a service category
        /// </summary>
        /// <param name="id">Service category identification</param>
        /// <param name="serviceCategory">Service category updated object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: ServiceCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador, Moderador")]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryID,Name")] ServiceCategory serviceCategory)
        {
            if (id != serviceCategory.CategoryID)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            try
            {
                if (Request != null && Request.Form.Files.Count > 0)
                {
                    IFormFile file = Request.Form.Files.FirstOrDefault();
                    using var dataStream = new MemoryStream();
                    await file.CopyToAsync(dataStream);
                    serviceCategory.Illustration = dataStream.ToArray();
                }
            }
            catch (InvalidOperationException) { }


            if (serviceCategory.Illustration == null)
            {
                byte[] illustration = _context.ServiceCategory.Where(n => n.CategoryID == serviceCategory.CategoryID).Select(n => n.Illustration).FirstOrDefault();
                serviceCategory.Illustration = illustration;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    serviceCategory.IsSubcategory = false;

                    if (serviceCategory.ServiceCategories == null)
                        serviceCategory.ServiceCategories = new List<ServiceCategory>();

                    _context.Update(serviceCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceCategoryExists(serviceCategory.CategoryID))
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
            return View(serviceCategory);
        }

        /// <summary>
        /// Show the form to alter the service subcategory with the id passed by parameter.
        /// </summary>
        /// <param name="id">Service Subcategory identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: ServiceCategories/Edit/5
        [Authorize(Roles = "Administrador, Moderador")]
        public async Task<IActionResult> EditSubcategory(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var serviceSubcategory = await _context.ServiceCategory.FindAsync(id);
            if (serviceSubcategory == null || !serviceSubcategory.IsSubcategory)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(serviceSubcategory);
        }

        /// <summary>
        /// This method allows to effect the desired changes in the service category with the id passed by paramenter.
        /// </summary>
        /// <param name="id">Service Subcategory identification</param>
        /// <param name="serviceSubcategory">Service subcategory updated object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: ServiceCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador, Moderador")]
        public async Task<IActionResult> EditSubcategory(int id, [Bind("CategoryID,Name")] ServiceCategory serviceSubcategory)
        {
            if (id != serviceSubcategory.CategoryID)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            try
            {
                if (Request != null && Request.Form.Files.Count > 0)
                {
                    IFormFile file = Request.Form.Files.FirstOrDefault();
                    using var dataStream = new MemoryStream();
                    await file.CopyToAsync(dataStream);
                    serviceSubcategory.Illustration = dataStream.ToArray();
                }
            }
            catch (InvalidOperationException) { }

            if (serviceSubcategory.Illustration == null)
            {
                byte[] illustration = _context.ServiceCategory.Where(n => n.CategoryID == serviceSubcategory.CategoryID).Select(n => n.Illustration).FirstOrDefault();
                serviceSubcategory.Illustration = illustration;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    serviceSubcategory.IsSubcategory = true;
                    _context.Update(serviceSubcategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceCategoryExists(serviceSubcategory.CategoryID))
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
            return View(serviceSubcategory);
        }

        /// <summary>
        /// To delete a service category
        /// </summary>
        /// <param name="id">Service category identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: ServiceCategories/Delete/5
        [Authorize(Roles = "Administrador, Moderador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
         
            var serviceCategory = await _context.ServiceCategory.FirstOrDefaultAsync(m => m.CategoryID == id);

            if (serviceCategory == null)
                return View("/Views/Shared/NotFound.cshtml");

            int serviceCategoriesCount = 0;
            
            if (serviceCategory.ServiceCategories != null)
                serviceCategoriesCount = serviceCategory.ServiceCategories.Count;


            if (serviceCategory == null || (!serviceCategory.IsSubcategory && serviceCategoriesCount != 0))
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var services = await _context.Service.Where(s => s.ServiceCategoryID == serviceCategory.CategoryID).ToListAsync();
            if (services.Count != 0)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(serviceCategory);
        }

        /// <summary>
        /// Deletes a service category
        /// </summary>
        /// <param name="id">Service category identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: ServiceCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador, Moderador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serviceCategory = await _context.ServiceCategory.FindAsync(id);

            if (serviceCategory == null)
                return View("/Views/Shared/NotFound.cshtml");


            int serviceCategoriesCount = 0;

            if (serviceCategory.ServiceCategories != null)
                serviceCategoriesCount = serviceCategory.ServiceCategories.Count;

            if (serviceCategory == null || (!serviceCategory.IsSubcategory && serviceCategoriesCount != 0))
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var services = await _context.Service.Where(s=>s.ServiceCategoryID == serviceCategory.CategoryID).ToListAsync();
            if(services.Count != 0)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            _context.ServiceCategory.Remove(serviceCategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Checks if a service category already exists
        /// </summary>
        /// <param name="id">Service Category identification</param>
        /// <returns>bool</returns>
        private bool ServiceCategoryExists(int id)
        {
            return _context.ServiceCategory.Any(e => e.CategoryID == id);
        }

        /// <summary>
        /// This method allows to create categories to the Integration Test
        /// </summary>
        /// <param name="name"></param>
        /// <returns>int</returns>
        [HttpGet]
        public string CreateCategoriesToTest()
        {
            var c = Create(new ServiceCategory { Name = "Teste_Categoria", Illustration = null}).Result;
            int categoryID = _context.ServiceCategory.Where(c => c.Name == "Teste_Categoria").FirstOrDefault().CategoryID;
            var s = Create(new ServiceCategory { Name = "Teste_SubCategoria", Illustration = null }).Result;
            int subcategoryID = _context.ServiceCategory.Where(c => c.Name == "Teste_SubCategoria").FirstOrDefault().CategoryID;
            return categoryID + "|" + subcategoryID;
        }

        /// <summary>
        /// This method allows to delete categories to the Integration Test
        /// </summary>
        /// <param name="name"></param>
        /// <returns>int</returns>
        [HttpPost]
        public void DeleteCategoriesToTest(string c, string s)
        {
            var b = DeleteConfirmed(Int32.Parse(c)).Result;
            var a = DeleteConfirmed(Int32.Parse(s)).Result;
        }
    }
}