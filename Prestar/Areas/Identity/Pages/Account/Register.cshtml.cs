using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Prestar.Models;

namespace Prestar.Areas.Identity.Pages.Account
{
    /// <summary>
    /// This class is responsible for representing the data model necessary to 
    /// register a user. This functionality is possible, thanks to the creation of 
    /// the InputModel class that represents the data model to be introduced 
    /// by the user. This class is also responsible for managing the actions that 
    /// allow this change, using the HTTP GET and POST methods.
    /// This class inherits from the PageModel class, which is an abstract class 
    /// that represents a page.
    /// <see cref="PageModel"/>
    /// </summary>
    [AllowAnonymous]
    //Trocar todas as referencias a IdentityUser por User
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        /// <summary>
        /// Constructor of the ChangePasswordModel class that receives four parameters 
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
        /// <param name="emailSender">
        /// This API supports the ASP.NET Core Identity default UI infrastructure and is not 
        /// intended to be used directly from your code.
        /// <see cref="IEmailSender"/>
        /// </param>
        public RegisterModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Property that allows to obtain an instance of the InputModel class.
        /// </summary>
        /// <see cref="InputModel"/>
        /// <seealso cref="BindPropertyAttribute"/>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// Return URL
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// List of external logins
        /// </summary>
        /// <see cref="IList{T}"/>
        /// <see cref="AuthenticationScheme"/>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        /// Class that serves as a model for the data to be obtained from the form 
        /// filled in by the user, when changing the password. The data is the email
        /// (Email), the password (Password) and the confirmation of the password 
        /// (ConfirmPassword).
        /// </summary>
        public class InputModel: User
        {

            /// <summary>
            /// Property that represents a required field for an email.
            /// </summary>
            [Required(ErrorMessage = "O email é obrigatório")]
            [EmailAddress(ErrorMessage = "Por favor introduza um email válido")]
            override public string Email { get; set; }

            /// <summary>
            /// Property that represents a required field for a password.
            /// </summary>
            [Required(ErrorMessage = "A password é obrigatória")]
            [StringLength(100, ErrorMessage = "A {0} deve conter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Palavra-passe")]
            public string Password { get; set; }

            /// <summary>
            /// Property that represents a required field for a confirmation of password.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar palavra-passe")]
            [Compare("Password", ErrorMessage = "A palavra-passe fornecida e a confirmação não coincidem.")]
            public string ConfirmPassword { get; set; }

        }

        /// <summary>
        /// Asynchronous method that represents the HTTP GET method, where 
        /// it is used to request data from a specific resource. In this case, the 
        /// form is for user registration. 
        /// If the user in question is not authenticated, a page Not Found (404) 
        /// is obtained. 
        /// If the user does not have a password, he is redirected to the password 
        /// set page.
        /// </summary>
        /// <param name="returnUrl">Return URL</param>
        /// <returns>Returns a Task</returns>
        /// <seealso cref="Task"/>
        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        /// <summary>
        /// Asynchronous method that represents an HTTP POST method, which 
        /// allows data to be sent to the server in order to create and, in this case, 
        /// update a resource, in this case, to add a new user.
        /// </summary>
        /// <param name="returnUrl">Return URL</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            
            if (ModelState.IsValid)
            {
                var user = new User {

                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    Birthdate = Input.Birthdate,
                    UserName = Input.UserName,
                    Email = Input.Email,
                    AccountCreationDate = DateTime.Now,
                    ShowEmail = true,
                    ShowPhoneNumber = true,
                    ReceiveNotifications = true
                };

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    
                    // Adiciona novo registo ao role 'cliente'
                    await _userManager.AddToRoleAsync(user, "Cliente");
                   
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirmação de email",
                        $"Por favor, confirme a sua conta na página prestar <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicando aqui</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
