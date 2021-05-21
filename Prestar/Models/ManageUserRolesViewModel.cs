using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Prestar.Models
{
    /// <summary>
    /// This class serves as a model for managing the roles of users of the application.
    /// </summary>
    public class ManageUserRolesViewModel
    {
        /// <summary>
        /// Role identification
        /// </summary>
        public string RoleId { get; set; }

        /// <summary>
        /// Role name
        /// </summary>
        [Display(Name ="Acesso")]
        public string RoleName { get; set; }

        /// <summary>
        /// Checks if role is selected
        /// </summary>
        [Display(Name ="Selecionado")]
        public bool Selected { get; set; }

    }
}
