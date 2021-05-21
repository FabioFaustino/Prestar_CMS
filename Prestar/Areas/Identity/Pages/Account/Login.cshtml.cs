using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Prestar.Models;

namespace Prestar.Areas.Identity.Pages.Account
{
    /// <summary>
    /// This class is responsible for representing the data model necessary for an 
    /// authenticated user to login. This class is also responsible for managing the
    /// actions that allow this login, using the HTTP GET and POST methods.
    /// This class inherits from the PageModel class, which is an abstract class that 
    /// represents a page.
    /// <see cref="PageModel"/>
    /// </summary>
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        /// <summary>
        /// Constructor of the LoginModel class that receives three parameters 
        /// and initializes them.
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
        public LoginModel(SignInManager<User> signInManager, 
            ILogger<LoginModel> logger,
            UserManager<User> userManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// Property that allows to obtain an instance of the InputModel class.
        /// </summary>
        /// <see cref="InputModel"/>
        /// <seealso cref="BindPropertyAttribute"/>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// List of external logins
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        /// Return URL
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Error message to display
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Class that serves as a model for the data to be obtained from the form 
        /// filled in by the user, when on login. The data is the email, the password
        /// and the bool property that checks if user selected RemeberMe checkbox.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Property that represents a required field for an email.
            /// </summary>
            [Required(ErrorMessage = "O email é obrigatório")]
            [EmailAddress(ErrorMessage = "Por favor introduza um email válido")]
            public string Email { get; set; }

            /// <summary>
            /// Property that represents a required field for a password.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            /// Property that represents a required field for a remember me bool value.
            /// </summary>
            [Display(Name = "Lembrar-me?")]
            public bool RememberMe { get; set; }
        }

        /// <summary>
        /// Asynchronous method that represents the HTTP GET method, where 
        /// it is used to request data from a specific resource. In this case, the 
        /// form for login. 
        /// </summary>
        /// <param name="returnUrl">Return URL</param>
        /// <returns>Returns a Task</returns>
        /// <seealso cref="Task"/>
        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        /// <summary>
        /// Asynchronous method that represents an HTTP POST method, which 
        /// allows data to be sent to the server in order to create and update a 
        /// resource, in this case, to login a user.
        /// </summary>
        /// <param name="returnUrl">Return URL</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            
            if (ModelState.IsValid)
            {
                string username = "";
                var userLogging = await _userManager.FindByEmailAsync(Input.Email);

                if (userLogging != null)
                    username = userLogging.UserName;

                if (userLogging != null &&  userLogging.HasRemovedAccount)
                {
                    TempData["LockoutReason"] = "Desativou a conta!";
                    TempData["LockoutDays"] = "um número indefinido de " ;
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(username, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    /// 
                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    
                    user.LastSeen = DateTime.Now;
                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
               
                if (result.IsLockedOut)
                {
                    var user =  _userManager.FindByEmailAsync(Input.Email).Result;
                    TempData["LockoutReason"] = user.BlockMotive;
                    TempData["LockoutDays"] = user.LockoutDays;
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    // Clear the existing external cookie to ensure a clean login process
                    await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                    // External Logins added manually here, to fix a bug where the external logins would disappear if the code reached this far
                    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
                    ModelState.AddModelError(string.Empty, "Tentativa de Login inválida.");
                    return Page();
                }
            }



            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
