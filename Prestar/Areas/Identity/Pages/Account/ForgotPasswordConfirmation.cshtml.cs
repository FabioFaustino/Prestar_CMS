using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Prestar.Areas.Identity.Pages.Account
{
    /// <summary>
    /// This class is responsible for representing the data model necessary for an 
    /// authenticated user to confirm his backup to password. 
    /// This class is also responsible for managing the actions that allow this change, 
    /// using the HTTP GET and POST methods.
    /// This class inherits from the PageModel class, which is an abstract class that 
    /// represents a page.
    /// <see cref="PageModel"/>
    /// </summary>
    [AllowAnonymous]
    public class ForgotPasswordConfirmation : PageModel
    {
        public void OnGet()
        {
        }
    }
}
