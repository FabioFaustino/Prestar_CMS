using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Prestar.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// This class allows you to enable the authenticator.
    /// This class inherits from the PageModel class, which is an abstract class that 
    /// represents a page.
    /// <see cref="PageModel"/>
    /// </summary>
    public class EnableAuthenticatorModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<EnableAuthenticatorModel> _logger;
        private readonly UrlEncoder _urlEncoder;

        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        /// <summary>
        /// Constructor of the ResetAuthenticatorModel class that receives three 
        /// parameters and initializes them.
        /// </summary>
        /// <param name="userManager">
        /// Provides the APIs for managing user in a persistence store.
        /// <see cref="UserManager{TUser}"/>
        /// </param>
        /// <param name="logger">
        /// Represents a type used to perform logging.
        /// <see cref="ILogger"/>
        /// </param>
        /// <param name="urlEncoder">
        /// Represents a URL character encoding.
        /// <see cref="UrlEncoder"/>
        /// </param>
        public EnableAuthenticatorModel(
            UserManager<IdentityUser> userManager,
            ILogger<EnableAuthenticatorModel> logger,
            UrlEncoder urlEncoder)
        {
            _userManager = userManager;
            _logger = logger;
            _urlEncoder = urlEncoder;
        }

        /// <summary>
        /// Shared Key Property
        /// </summary>
        public string SharedKey { get; set; }

        /// <summary>
        /// Authentication Uri Property
        /// </summary>
        public string AuthenticatorUri { get; set; }

        /// <summary>
        /// Property that allows you to obtain recovery codes.
        /// </summary>
        [TempData]
        public string[] RecoveryCodes { get; set; }

        /// <summary>
        /// Property that allows to get the request status message and providing 
        /// feedback to the user after a form submission that results in the user 
        /// being redirected to another page.
        /// </summary>
        /// <seealso cref="TempDataAttribute"/>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Property that allows to obtain an instance of the InputModel class.
        /// </summary>
        /// <see cref="InputModel"/>
        /// <seealso cref="BindPropertyAttribute"/>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// Class that serves as a model for the data to be obtained from the form 
        /// filled in by the user, when enabling authenticator. The data is the 
        /// verification code (Code).
        /// </summary>
        public class InputModel
        {
            [Required]
            [StringLength(7, ErrorMessage = "O {0} deve ser menor que {2} e maior que {1}.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Código de Verificação")]
            public string Code { get; set; }
        }

        /// <summary>
        /// Asynchronous method that represents the HTTP GET method, where 
        /// it is used to request data from a specific resource. In this case, the 
        /// form to enable the authenticator.
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

            await LoadSharedKeyAndQrCodeUriAsync(user);

            return Page();
        }

        /// <summary>
        /// Asynchronous method that represents an HTTP POST method, which 
        /// allows data to be sent to the server in order to create and, in this case, 
        /// update a resource, in this case, the authenticator.
        /// If the user is not authenticated, a page Not Found (404) is obtained. 
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            // Strip spaces and hypens
            var verificationCode = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("Input.Code", "Verification code is invalid.");
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            var userId = await _userManager.GetUserIdAsync(user);
            _logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", userId);

            StatusMessage = "Your authenticator app has been verified.";

            if (await _userManager.CountRecoveryCodesAsync(user) == 0)
            {
                var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                RecoveryCodes = recoveryCodes.ToArray();
                return RedirectToPage("./ShowRecoveryCodes");
            }
            else
            {
                return RedirectToPage("./TwoFactorAuthentication");
            }
        }

        /// <summary>
        /// Loads shared key and Qr Code Uri
        /// </summary>
        /// <param name="user">
        /// User
        /// <see cref="IdentityUser"/>
        /// </param>
        /// <returns>Returns a Task</returns>
        /// <seealso cref="Task"/>
        private async Task LoadSharedKeyAndQrCodeUriAsync(IdentityUser user)
        {
            // Load the authenticator key & QR code URI to display on the form
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            SharedKey = FormatKey(unformattedKey);

            var email = await _userManager.GetEmailAsync(user);
            AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey);
        }

        /// <summary>
        /// Format Key
        /// </summary>
        /// <param name="unformattedKey">Unformatted Key</param>
        /// <returns>string with format key</returns>
        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// Generates Qr Code Uri
        /// </summary>
        /// <param name="email">email</param>
        /// <param name="unformattedKey">unformatted key</param>
        /// <returns>string with Qr Code Uri</returns>
        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                AuthenticatorUriFormat,
                _urlEncoder.Encode("Prestar"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }
    }
}
