using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Prestar.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// This class is responsible for showing the recovery codes.
    /// This class inherits from the PageModel class, which is an abstract class that 
    /// represents a page.
    /// </summary>
    /// <see cref="PageModel"/>
    public class ShowRecoveryCodesModel : PageModel
    {
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
        /// Asynchronous method that represents the HTTP GET method, where 
        /// it is used to request data from a specific resource. In this case, the 
        /// form to obtain user's recovey codes.
        /// If there are no recovery codes, the user is redirected to the Two Factor 
        /// Authentication page.
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public IActionResult OnGet()
        {
            if (RecoveryCodes == null || RecoveryCodes.Length == 0)
            {
                return RedirectToPage("./TwoFactorAuthentication");
            }

            return Page();
        }
    }
}
