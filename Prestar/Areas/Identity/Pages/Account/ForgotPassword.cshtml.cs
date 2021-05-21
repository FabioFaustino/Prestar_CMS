using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Prestar.Models;

namespace Prestar.Areas.Identity.Pages.Account
{
    /// <summary>
    /// This class is responsible for representing the data model necessary for an 
    /// authenticated user to backup his password. This class is also responsible 
    /// for managing the actions that allow this change, using the HTTP GET and 
    /// POST methods.
    /// This class inherits from the PageModel class, which is an abstract class that 
    /// represents a page.
    /// <see cref="PageModel"/>
    /// </summary>
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;

        /// <summary>
        /// Constructor of the ChangePasswordModel class that receives four parameters 
        /// and initializes them.
        /// </summary>
        /// <param name="userManager">
        /// Provides the APIs for managing user in a persistence store.
        /// <see cref="UserManager{TUser}"/>
        /// </param>
        /// <param name="emailSender">
        /// This API supports the ASP.NET Core Identity default UI infrastructure and is 
        /// not intended to be used directly from your code.
        /// <see cref="IEmailSender"/>
        /// </param>
        public ForgotPasswordModel(UserManager<User> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
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
        /// Class that serves as a model for the data to be obtained from the form 
        /// filled in by the user, when backing up the password. The data is email.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Property that represents a required field for an email.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        /// <summary>
        /// Asynchronous method that represents an HTTP POST method, which 
        /// allows data to be sent to the server in order to create and update a 
        /// resource, in this case, the password.
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Redefinir palavra-passe",
                    $"Por favor, redifina a sua palavra-passe <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicando aqui.</a>.");

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
