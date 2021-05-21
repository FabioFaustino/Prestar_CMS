using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Prestar.Models;

namespace Prestar.Areas.Identity.Pages.Account
{
    /// <summary>
    /// This class is responsible for representing a model to confirm a user registration
    /// and for having actions that make this change possible, using HTTP GET and 
    /// POST methods.
    /// This class inherits from the PageModel class, which is an abstract class that 
    /// represents a page.
    /// <see cref="PageModel"/>
    /// </summary>
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _sender;

        /// <summary>
        /// Constructor of the RegisterConfirmationModel class that receives two parameters 
        /// and initializes them.
        /// </summary>
        /// <param name="userManager">
        /// Provides the APIs for managing user in a persistence store.
        /// <see cref="UserManager{TUser}"/>
        /// </param>
        /// <param name="sender">
        /// This API supports the ASP.NET Core Identity default UI infrastructure and is not 
        /// intended to be used directly from your code.
        /// <see cref="IEmailSender"/>
        /// </param>
        public RegisterConfirmationModel(UserManager<User> userManager, IEmailSender sender)
        {
            _userManager = userManager;
            _sender = sender;
        }

        /// <summary>
        /// Property that represents an email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Property that checks if confirm account link is displayed.
        /// </summary>
        public bool DisplayConfirmAccountLink { get; set; }

        /// <summary>
        /// Property that represents an email confirmation URL.
        /// </summary>
        public string EmailConfirmationUrl { get; set; }

        /// <summary>
        /// Asynchronous method that represents the HTTP GET method, where 
        /// it is used to request data from a specific resource. In this case, the 
        /// form for confirm the user registration. 
        /// If the email entered is null, the user is redirected to the Index page 
        /// of the user's profile.
        /// If the email is not found in the database, it means that the user does 
        /// not exist and has not registered, he is redirected to the Not Found page.
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="returnUrl">return URL</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            Email = email;
            // Once you add a real email sender, you should remove this code that lets you confirm the account
            DisplayConfirmAccountLink = true;
            if (DisplayConfirmAccountLink)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                EmailConfirmationUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);
            }

            return Page();
        }
    }
}
