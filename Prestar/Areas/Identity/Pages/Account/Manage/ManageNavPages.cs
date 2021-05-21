using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Prestar.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// This class is responsible for managing the user profile navigation pages.
    /// </summary>
    public static class ManageNavPages
    {
        /// <summary>
        /// Represents Index Page.
        /// </summary>
        public static string Index => "Index";

        /// <summary>
        /// Represents Email Page.
        /// </summary>
        public static string Email => "Email";

        /// <summary>
        /// Represents Change Password Page.
        /// </summary>
        public static string ChangePassword => "ChangePassword";

        /// <summary>
        /// Represents Download Personal Data Page.
        /// </summary>
        public static string DownloadPersonalData => "DownloadPersonalData";

        /// <summary>
        /// Represents Delete Personal Data Page.
        /// </summary>
        public static string DeletePersonalData => "DeletePersonalData";

        /// <summary>
        /// Represents External Logins Page.
        /// </summary>
        public static string ExternalLogins => "ExternalLogins";

        /// <summary>
        /// Represents Personal Data Page.
        /// </summary>
        public static string PersonalData => "PersonalData";

        /// <summary>
        /// Represents Two Factor Authentication Page.
        /// </summary>
        public static string TwoFactorAuthentication => "TwoFactorAuthentication";

        /// <summary>
        /// Represents Change Type Of Account Page.
        /// </summary>
        public static string ChangeTypeOfAccount => "ChangeTypeOfAccount";

        /// <summary>
        /// Represents Settings Page.
        /// </summary>
        public static string Settings => "Settings";

        /// <summary>
        /// Represents the class of Index page
        /// </summary>
        /// <param name="viewContext">
        /// Encapsulates information that is related to rendering a view.
        /// </param>
        /// <seealso cref="ViewContext"/>
        /// <returns>string with active or null</returns>
        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

        /// <summary>
        /// Represents the class of Email page
        /// </summary>
        /// <param name="viewContext">
        /// Encapsulates information that is related to rendering a view.
        /// </param>
        /// <seealso cref="ViewContext"/>
        /// <returns>string with active or null</returns>
        public static string EmailNavClass(ViewContext viewContext) => PageNavClass(viewContext, Email);

        /// <summary>
        /// Represents the class of Change Password page
        /// </summary>
        /// <param name="viewContext">
        /// Encapsulates information that is related to rendering a view.
        /// </param>
        /// <seealso cref="ViewContext"/>
        /// <returns>string with active or null</returns>
        public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);

        /// <summary>
        /// Represents the class of Download Personal Data page
        /// </summary>
        /// <param name="viewContext">
        /// Encapsulates information that is related to rendering a view.
        /// </param>
        /// <seealso cref="ViewContext"/>
        /// <returns>string with active or null</returns>
        public static string DownloadPersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, DownloadPersonalData);

        /// <summary>
        /// Represents the class of DeletePersonalData page
        /// </summary>
        /// <param name="viewContext">
        /// Encapsulates information that is related to rendering a view.
        /// </param>
        /// <seealso cref="ViewContext"/>
        /// <returns>string with active or null</returns>
        public static string DeletePersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, DeletePersonalData);

        /// <summary>
        /// Represents the class of ExternalLogins page
        /// </summary>
        /// <param name="viewContext">
        /// Encapsulates information that is related to rendering a view.
        /// </param>
        /// <seealso cref="ViewContext"/>
        /// <returns>string with active or null</returns>
        public static string ExternalLoginsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ExternalLogins);

        /// <summary>
        /// Represents the class of PersonalData page
        /// </summary>
        /// <param name="viewContext">
        /// Encapsulates information that is related to rendering a view.
        /// </param>
        /// <seealso cref="ViewContext"/>
        /// <returns>string with active or null</returns>
        public static string PersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, PersonalData);

        /// <summary>
        /// Represents the class of TwoFactorAuthentication page
        /// </summary>
        /// <param name="viewContext">
        /// Encapsulates information that is related to rendering a view.
        /// </param>
        /// <seealso cref="ViewContext"/>
        /// <returns>string with active or null</returns>
        public static string TwoFactorAuthenticationNavClass(ViewContext viewContext) => PageNavClass(viewContext, TwoFactorAuthentication);

        /// <summary>
        /// Represents the class of  ChangeTypeOfAccount page
        /// </summary>
        /// <param name="viewContext">
        /// Encapsulates information that is related to rendering a view.
        /// </param>
        /// <seealso cref="ViewContext"/>
        /// <returns>string with active or null</returns>
        public static string ChangeTypeOfAccountClass(ViewContext viewContext) => PageNavClass(viewContext, ChangeTypeOfAccount);

        /// <summary>
        /// Represents the class of Settings page
        /// </summary>
        /// <param name="viewContext">
        /// Encapsulates information that is related to rendering a view.
        /// </param>
        /// <seealso cref="ViewContext"/>
        /// <returns>string with active or null</returns>
        public static string SettingsClass(ViewContext viewContext) => PageNavClass(viewContext, Settings);

        /// <summary>
        /// Allows to find which page is active
        /// </summary>
        /// <param name="viewContext">
        /// Encapsulates information that is related to rendering a view.
        /// </param>
        /// <param name="page">
        /// Name of the page as string
        /// </param>
        /// <seealso cref="ViewContext"/>
        /// <returns>string with active or null</returns>
        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
