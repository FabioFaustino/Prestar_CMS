using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Prestar.Models
{
    /// <summary>
    /// This class serves as a model for enrollment in training. It has an identification, 
    /// training identification and identification of the user who registered.
    /// </summary>
    /// <seealso cref="Formation"/>
    public class Enrollment
    {
        /// <summary>
        /// Enrollment identification
        /// </summary>
        [Key]
        public int EnrollmentID { get; set; }

        /// <summary>
        /// Formation identification
        /// </summary>
        [Display(Name = "ID da formação")]
        [ForeignKey("Formation")]
        public int FormationID { get; set; }

        /// <summary>
        /// Formation object
        /// </summary>
        public Formation Formation { get; set; }

        /// <summary>
        /// User who registered idnetification
        /// </summary>
        [Display(Name = "Utilizador Registado")]
        [ForeignKey("Registered")]
        public string RegisteredID { get; set; }

        /// <summary>
        /// User who registered object
        /// </summary>
        public User Registered { get; set; }

    }
}
