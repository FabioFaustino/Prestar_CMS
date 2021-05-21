using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Prestar.Models;

namespace Prestar.Areas.Identity.Pages.Account
{
    /// <summary>
    /// This class is responsible for representing a model to reset password and for 
    /// having actions that make this change possible, using HTTP GET and POST 
    /// methods.
    /// This class inherits from the PageModel class, which is an abstract class that 
    /// represents a page.
    /// <see cref="PageModel"/>
    /// </summary>
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Constructor of the ResetPasswordModel class that receives five parameters 
        /// and initializes them.
        /// </summary>
        /// <param name="userManager">
        /// Provides the APIs for managing user in a persistence store.
        /// <see cref="UserManager{TUser}"/>
        /// </param>
        public ResetPasswordModel(UserManager<User> userManager)
        {
            _userManager = userManager;
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
        /// filled in by the user, when changing the account type. The data is the 
        /// email (Email), the password (Password), the password confirmation 
        /// (ConfirmPassword) and the two factor authentication code (Code).
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
            [StringLength(100, ErrorMessage = "A {0} deve conter no minimo {2} e no máximo {1} caracteres.", MinimumLength = 6)]
            [Display(Name = "Nova palavra-passe")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            /// Property that represents a required field for a password confirmation.
            /// </summary>
            [DataType(DataType.Password)]
            [Required]
            [Display(Name = "Confirmar palavra-passe")]
            [Compare("Password", ErrorMessage = "A palavra-passe e a confirmação não coincidem.")]
            public string ConfirmPassword { get; set; }

            /// <summary>
            /// Property that represents an optinal field for the code.
            /// </summary>
            public string Code { get; set; }
        }

        /// <summary>
        /// Asynchronous method that represents the HTTP GET method, where 
        /// it is used to request data from a specific resource. In this case, the 
        /// form for reseting the password. 
        /// </summary>
        /// <param name="code">Code</param>
        /// <returns>Returns an IActionResult</returns>
        /// <seealso cref="IActionResult"/>
        public IActionResult OnGet(string code = null)
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                Input = new InputModel
                {
                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
                };
                return Page();
            }
        }

        /// <summary>
        /// Asynchronous method that represents an HTTP POST method, which 
        /// allows data to be sent to the server in order to create and, in this case, 
        /// update a resource, in this case, to reset the password.
        /// If the user is not authenticated, he is redirected to the page 
        /// ResetPasswordConfirmation, not revealing that the user does not exist.
        /// If the action is succeeded, this user will be redirect to the page 
        /// ResetPasswordConfirmation.
        /// Otherwise, if there are errors in the model, then this user will stay in 
        /// the same page, and will be able to correct the errors where they are shown.
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (result.Succeeded)
            {
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}
