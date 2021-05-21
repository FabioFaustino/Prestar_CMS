using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prestar.Data;

namespace Prestar.Models
{

    /// <summary>
    /// CommentAndEvaluations Controller
    /// </summary>
    /// <see cref="CommentAndEvaluation"/>
    public class ServiceImagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly int MAX_IMAGES = 10;


        /// <summary>
        /// Constructor of the ServiceImagesController class that receives two parameters 
        /// and initializes them.
        /// </summary>      
        /// <param name="context">
        /// Application database context
        /// <see cref="ApplicationDbContext"/>
        /// </param>
        /// <param name="userManager">
        /// Provides the APIs for managing user in a persistence store.
        /// <see cref="UserManager{TUser}"/>
        /// </param>
        public ServiceImagesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }



        /// <summary>
        /// Allows you to display every image that belongs to a service
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <param name="id">
        /// Service ID
        /// <see cref="Service"/>
        /// </param>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: ServiceImages
        public async Task<IActionResult> Index(int id)
        {
            var serviceImages = _context.ServiceImage.Include(s => s.Service).Where(s => s.ServiceID == id);

            var user = await _userManager.GetUserAsync(User);
            var providerID = _context.Service.Where(s => s.ServiceID == id).FirstOrDefault().UserID;

            if (user.Id != providerID)
                return View("/Views/Shared/NotFound.cshtml");

            ViewBag.ServiceID = id;

            return View(await serviceImages.ToListAsync());
        }


        /// <summary>
        /// Allows you to add one or more images to the service.
        /// For performance reasons there is a limit of images allowed per service, so here we make sure
        /// that information reaches theview so the user can see it
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <param name="id">
        /// Service ID
        /// <see cref="Service"/>
        /// </param>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: ServiceImages/Create/4
        public IActionResult Create(int id)
        {           
           
            var service = _context.Service.Find(id);

            if (service == null)
                return View("/Views/Shared/NotFound.cshtml");

            var user =  _userManager.GetUserAsync(User).Result;

            if (!user.Id.Equals(service.UserID))
                return View("/Views/Shared/NotFound.cshtml");

            ViewBag.ServiceID = service.ServiceID;
            ViewBag.ServiceName = service.ServiceName;

            ViewBag.MaxImages = MAX_IMAGES;
            ViewBag.CurrentImages = _context.ServiceImage.Where(si => si.ServiceID == service.ServiceID).Count();

            return View();
        }


        /// /// <summary>
        /// On the creation of image(s), the number of actual images is checked, and compared against the max number of items.
        /// A list with all the form items will be created, and added as a single register to the database.
        /// If a user doesn't add any image, or the number of added images surpasses the max, an error will be shown and the 
        /// database won't be updated.
        /// </summary>        
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: ServiceImages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ServiceID,Image,CreationDate")] ServiceImage serviceImage)
        {
            var service = _context.Service.Find(serviceImage.ServiceID);
            var currentServiceImagesCount =  _context.ServiceImage.Where(si => si.ServiceID == service.ServiceID).Count();
            var imagesInFormCount = 0;

            if (Request.Form.Files.Count == 0)
            {
                ModelState.AddModelError("Image", "Selecione pelo menos uma imagem.");
            }
            else
            {
                imagesInFormCount = Request.Form.Files.Count;
            }
           

            ViewBag.CurrentImages = currentServiceImagesCount;
            ViewBag.MaxImages = MAX_IMAGES;
            ViewBag.ServiceID = serviceImage.ServiceID;

            if ((imagesInFormCount + currentServiceImagesCount) > MAX_IMAGES)
            {
                ModelState.AddModelError("Image", "O número máximo de imagens foi ultrapassado.");
            }


            List<ServiceImage> images = new List<ServiceImage>();

            var user = await _userManager.GetUserAsync(User);
            var providerID = _context.Service.Where(s => s.ServiceID == service.ServiceID).FirstOrDefault().UserID;
            
            if(user.Id != providerID)
                return View("/Views/Shared/NotFound.cshtml");

           

            try
            {
                foreach (var image in Request.Form.Files)
                {
                    IFormFile file = image;
                    using var dataStream = new MemoryStream();
                    await file.CopyToAsync(dataStream);
                    ServiceImage img = new ServiceImage()
                    {
                        ServiceID = serviceImage.ServiceID,
                        Service = service,
                        Image = dataStream.ToArray(),
                        CreationDate = DateTime.Now
                    };
                    images.Add(img);
                }
            }
            catch (InvalidOperationException) { }
            
            if (ModelState.IsValid)
            {
                _context.ServiceImage.AddRange(images);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "ServiceImages", new { id = service.ServiceID });
            }
            return View(); 
           
        }

        /// <summary>
        /// Since we are using a Modal to confirm the deletion of a service image, we dont go through the Get phase
        /// </summary>
        /// <param name="id">ServiceImage identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> RemoveImage(int id)
        {
            var serviceImage = await _context.ServiceImage.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);
            var providerID = _context.Service.Where(s => s.ServiceID == serviceImage.ServiceID).FirstOrDefault().UserID;

            if (user.Id != providerID)
                return View("/Views/Shared/NotFound.cshtml");

            _context.ServiceImage.Remove(serviceImage);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "ServiceImages", new { id = serviceImage.ServiceID });
        }

        /// <summary>
        /// Checks if a ServiceImage exists
        /// </summary>
        /// <param name="id">ServiceImage identification</param>
        /// <returns>bool</returns>
        private bool ServiceImageExists(int id)
        {
            return _context.ServiceImage.Any(e => e.ServiceImageID == id);
        }
    }
}
