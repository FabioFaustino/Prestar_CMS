using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Prestar.Models
{
    /// <summary>
    /// This class serves as a model for the user manual, which has a paper, a date 
    /// of the last update, the name of the user who updated the manual and a list 
    /// of sections.
    /// </summary>
    /// <see cref="Section"/>
    public class UserManual
    {
        /// <summary>
        /// User manual identification
        /// </summary>
        [Key]
        public int UserManualID { get; set; }

        /// <summary>
        /// User Manual Role
        /// </summary>
        [Required(ErrorMessage = "É necessário necessário selecionar um Role")]
        [Display(Name = "Role")]
        public string Role { get; set; }

        /// <summary>
        /// User Manual last update date time
        /// </summary>
        [Display(Name = "Última Atualização")]
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// User manual last update user identification
        /// </summary>
        [Display(Name = "ID do Responsável pela Última Atualização")]
        [ForeignKey("User")]
        public string LastUpdateUserID { get; set; }

        /// <summary>
        /// User manual last update user instance
        /// </summary>
        [Display(Name = "Responsável pela Última Atualização")]
        public User User { get; set; }

        /// <summary>
        /// List of user manual' sections
        /// </summary>
        /// <see cref="List{T}"/>
        public List<Section> Sections { get; set; }

    }
}
