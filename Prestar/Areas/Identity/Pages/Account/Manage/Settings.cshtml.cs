using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Prestar.Data;
using Prestar.Models;

namespace Prestar.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// This class is responsible for representing a model of user account settings, 
    /// relating to the enabling / disenabling of notifications, the permission to 
    /// show / hide the email and the permission to show / hide the phone number,
    /// if specified.
    /// This class inherits from the PageModel class, which is an abstract class that 
    /// represents a page.
    /// <see cref="PageModel"/>
    /// </summary>
    public class SettingsModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor of the SettingsModel class that receives four 
        /// parameters and initializes them.
        /// </summary>
        /// <param name="userManager">
        /// Provides the APIs for managing user in a persistence store.
        /// <see cref="UserManager{TUser}"/>
        /// </param>
        /// <param name="signInManager">
        /// Provides the APIs for user sign in.
        /// <see cref="SignInManager{TUser}"/>
        /// </param>
        /// <param name="logger">
        /// Represents a type used to perform logging.
        /// <see cref="ILogger"/>
        /// </param>
        /// <param name="context">
        /// Represents the context of the application database.
        /// <see cref="ApplicationDbContext"/>
        /// </param>
        public SettingsModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<ChangePasswordModel> logger,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Property that allows to get the request status message and providing 
        /// feedback to the user after a form submission that results in the user 
        /// being redirected to another page.
        /// </summary>
        /// <seealso cref="TempDataAttribute"/>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Property that allows to obtain an instance of the Configurations class.
        /// </summary>
        /// <see cref="Configurations"/>
        /// <seealso cref="BindPropertyAttribute"/>
        [BindProperty]
        public Configurations Input { get; set; }

        /// <summary>
        /// Class that serves as a model for the data to be obtained from the form 
        /// filled in by the user, when changing the configurations of his account. 
        /// The data are the enabling / disenabling of notifications (represented 
        /// by a bool value for the Notifications property), the visibility of the email 
        /// (representing a bool value for the ShowEmail property) and the visibility 
        /// of the phone number (represented by a bool value for the ShowPhone 
        /// property).
        /// </summary>
        public class Configurations : User
        {
            /*/// <summary>
            /// Property that represents a required boolean field for enabling / 
            /// disenabling notifications.
            /// </summary>
            [Required]
            [Display(Name = "Notificações")]
            public bool Notifications{ get; set; }

            /// <summary>
            /// Property that represents a required boolean field for email's visibility.
            /// </summary>
            [Required]
            [Display(Name = "Mostrar Email")]
            public bool ShowEmail { get; set; }

            /// <summary>
            /// Property that represents a required boolean field for phone's visibility.
            /// </summary>
            [Required]
            [Display(Name = "Mostrar Telefone")]
            public bool ShowPhone { get; set; }*/
        }

        /// <summary>
        /// Property that enables/disables user's notifications
        /// </summary>
        /// <returns>
        /// <see cref="bool"/> where true means enabling and false disenabling
        /// </returns>
        public bool ReceiveNotifications()
        {
            var user = _userManager.GetUserAsync(User).Result;
            return user.ReceiveNotifications;
        }

        /// <summary>
        /// Property that gets and sets visibility for user's email
        /// </summary>
        /// <returns>
        /// <see cref="bool"/> where true means visible and false hidden
        /// </returns>
        public bool ShowEmail()
        {
            var user = _userManager.GetUserAsync(User).Result;
            return user.ShowEmail;
        }

        /// <summary>
        /// Property that gets and sets visibility for user's phone
        /// </summary>
        /// <returns>
        /// <see cref="bool"/> where true means visible and false hidden
        /// </returns>
        public bool ShowPhone()
        {
            var user = _userManager.GetUserAsync(User).Result;
            return user.ShowPhoneNumber;
        }

        /// <summary>
        /// Asynchronous method that represents the HTTP GET method, where 
        /// it is used to request data from a specific resource. In this case, the 
        /// form to set up user's account.
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> OnPostAsync()
        {
            var user = _userManager.GetUserAsync(User).Result;

            if (ReceiveNotifications() != Input.ReceiveNotifications)
            {              
                user.ReceiveNotifications = Input.ReceiveNotifications;
                _context.Update(user);
                await _context.SaveChangesAsync();

            }
            if(user.ShowEmail != Input.ShowEmail)
            {
                user.ShowEmail = Input.ShowEmail;
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            if(user.ShowPhoneNumber != Input.ShowPhoneNumber)
            {
                user.ShowPhoneNumber = Input.ShowPhoneNumber;
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToPage();
        }
    }
}
