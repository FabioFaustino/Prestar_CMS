
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
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
    /// This class is responsible for representing the data model necessary for an 
    /// authenticated user to login with an external account. 
    /// This class is also responsible for managing the actions that allow this change, 
    /// using the HTTP GET and POST methods.
    /// This class inherits from the PageModel class, which is an abstract class that 
    /// represents a page.
    /// <see cref="PageModel"/>
    /// </summary>
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;

        /// <summary>
        ///  Constructor of the ExternalLoginModel class that receives four parameters 
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
        /// This API supports the ASP.NET Core Identity default UI infrastructure and is 
        /// not intended to be used directly from your code.
        /// <see cref="IEmailSender"/>
        /// </param>
        public ExternalLoginModel(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
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
        /// Provider display name
        /// </summary>
        public string ProviderDisplayName { get; set; }

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
        /// filled in by the user, when loging with an external account. The data is
        /// the username and the email.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Property that represents a required field for a username.
            /// </summary>
            [Required(ErrorMessage = "Este campo é de preenchimento obrigatório")]
            [Display(Name = "Nome de utilizador")]
            [RegularExpression(@"^[A-z][A-z0-9|\.|\s]+$", ErrorMessage = "O nome de utilizador só pode conter caracteres alfanuméricos e pontos")]
            [StringLength(20, ErrorMessage = "O {0} deve conter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 2)]
            public String UserName { get; set; }

            /// <summary>
            /// Property that represents a required field for an email.
            /// </summary>
            [Required(ErrorMessage = "O email é obrigatório")]
            [EmailAddress]
            public string Email { get; set; }
            
        }

        /// <summary>
        /// Method that represents the HTTP GET method, where 
        /// it is used to request data from a specific resource. In this case, the 
        /// form for loging with an external login.
        /// </summary>
        /// <returns>Returns an IActionResult</returns>
        /// <seealso cref="IActionResult"/>
        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        /// <summary>
        /// Method that represents an HTTP POST method, which allows data to 
        /// be sent to the server in order to create and update a resource, in this 
        /// case, to login with an external account.
        /// </summary>
        /// <param name="provider"> Provider</param>
        /// <param name="returnUrl">Return URL</param>
        /// <returns>Returns an IActionResult</returns>
        /// <seealso cref="IActionResult"/>
        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        /// <summary>
        /// On get callback async method
        /// </summary>
        /// <param name="returnUrl">Return URL</param>
        /// <param name="remoteError">Remote Error</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new {ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor : true);
            if (result.Succeeded)
            {
                // Codigo para ter guardar o ultimo login do utilizador
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                user.LastSeen = DateTime.Now;               

                await _userManager.UpdateAsync(user);
                
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            // Código para passar os dias e motivo de bloqueio
            if (result.IsLockedOut)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                TempData["LockoutReason"] = user.BlockMotive;
                TempData["LockoutDays"] = user.LockoutDays;
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");            
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        
                };
                   
                }
                return Page();
            }
        }

        /// <summary>
        ///  On POST method confirmation async method
        /// </summary>
        /// <param name="returnUrl">Return URL</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Erro ao carregar informações de Login.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = new User { UserName = Input.UserName, Email = Input.Email, AccountCreationDate = DateTime.Now };

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        //Adiciona utilizador ao role Cliente
                        await _userManager.AddToRoleAsync(user, "Cliente");

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirmação de email",
                            $"Por favor confirme a sua conta <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicando aqui</a>.");

                        // If account confirmation is required, we need to show the link if we don't have a real email sender
                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }
    }
}
