using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Prestar.Areas.Identity.Pages
{
    /// <summary>
    /// Represents model error
    /// Inherits PageModel
    /// <see cref="PageModel"/>
    /// </summary>
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        /// <summary>
        /// Request Identification
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Check if request identification is shown
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}
