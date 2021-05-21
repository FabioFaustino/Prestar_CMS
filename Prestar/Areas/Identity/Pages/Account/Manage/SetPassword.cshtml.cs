using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Prestar.Models;

namespace Prestar.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// This class is responsible for representing a model for changing the 
    /// password, as well as for representing the actions that make this 
    /// change possible.
    /// This class inherits from the PageModel class, which is an abstract class that 
    /// represents a page.
    /// <see cref="PageModel"/>
    /// </summary>
    public class SetPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        /// <summary>
        /// Constructor of the SetPasswordModel class that receives two 
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
        public SetPasswordModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Property that allows to obtain an instance of the InputModel class.
        /// </summary>
        /// <see cref="InputModel"/>
        /// <seealso cref="BindPropertyAttribute"/>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// Property that allows to get the request status message and providing 
        /// feedback to the user after a form submission that results in the user 
        /// being redirected to another page.
        /// </summary>
        /// <seealso cref="TempDataAttribute"/>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Class that serves as a model for the data to be obtained from the form 
        /// filled in by the user, when changing the password. The data is the new 
        /// password (NewPassword) and the confirmation of the new password 
        /// (ConfirmPassword).
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Property that represents a required field for a new password.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Nova Palavra-passe")]
            public string NewPassword { get; set; }

            /// <summary>
            /// Property that represents a required field to confirm the new 
            /// password.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar palavra-passe")]
            [Compare("NewPassword", ErrorMessage = "a nova palavra-passe e a confirmação não coincidem.")]
            public string ConfirmPassword { get; set; }
        }

        /// <summary>
        /// Asynchronous method that represents the HTTP GET method, where 
        /// it is used to request data from a specific resource. In this case, the 
        /// form for changing the password. 
        /// If the user in question is not authenticated, a page Not Found (404) 
        /// is obtained. 
        /// If user has a password, he is redirected to the password change page.
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);

            if (hasPassword)
            {
                return RedirectToPage("./ChangePassword");
            }

            return Page();
        }

        /// <summary>
        /// Asynchronous method that represents an HTTP POST method, which 
        /// allows data to be sent to the server in order to create and, in this case, 
        /// update a resource, in this case, the password.
        /// If the data entered is not valid, the user remains on the form page, in 
        /// order to correct the data that, perhaps, are invalid. 
        /// If the user is not authenticated, a page Not Found (404) is obtained. 
        /// If the change ends in failure, the user remains on the form page, where 
        /// errors will be shown in the invalid fields.
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

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, Input.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                foreach (var error in addPasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "A sua palavra-passe foi definida.";

            return RedirectToPage();
        }
    }
}
