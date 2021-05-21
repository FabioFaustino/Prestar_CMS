using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Prestar.Models
{
    /// <summary>
    /// This class serves as a model for managing user access, which has an instance 
    /// of the user in question, the reason for the account lockout, the number of 
    /// days of the lockout, the identification of the complaint and a list of user roles.
    /// This class inherits from the User class.
    /// </summary>
    /// <see cref="Complaint"/>
    public class UserAccessViewModel : User
    {
        /// <summary>
        /// User instance
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// User access block motive
        /// </summary>
        [Display(Name = "Motivo de Bloqueio")]
        override public  String BlockMotive { get; set; }

        /// <summary>
        /// User access block duration in days
        /// </summary>
        [Display(Name = "Dias de Bloqueio")]
        override public  int? LockoutDays { get; set; }

        /// <summary>
        /// Complaint identification
        /// </summary>
        public int? ComplaintID { get; set; }

        /// <summary>
        /// List of user roles
        /// </summary>
        public IEnumerable<string> Roles { get; set; }
    }
}
