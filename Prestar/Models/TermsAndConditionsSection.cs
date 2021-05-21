using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Prestar.Models
{
    /// <summary>
    /// This class allows you to create a section template for the Terms and Conditions page. 
    /// Each section must contain a title and content. This must be available for consultation 
    /// by all users and only editable for moderators and administrators.
    /// </summary>
    public class TermsAndConditionsSection
    {
        /// <summary>
        /// Terms And Conditions Section Identification
        /// </summary>
        [Key]
        public int TermsAndConditionsSectionID { get; set; }

        /// <summary>
        /// Terms And Conditions Section Title
        /// </summary>
        [Required(ErrorMessage = "Deve colocar um título")]
        [Display(Name = "Título")]
        [StringLength(120, ErrorMessage = "O {0} deve conter no máximo {1} caracteres.")]
        public string Title { get; set; }

        /// <summary>
        /// Terms And Conditions Section Content 
        /// </summary>
        [Required(ErrorMessage = "Deve colocar o conteúdo")]
        [Display(Name = "Conteúdo")]
        public string Content { get; set; }

        /// <summary>
        /// Date of the Last Update of Terms And Conditions Section
        /// </summary>
        [Display(Name = "Última Atualização")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// Identification of User that Made the Last Update of Terms And Conditions Section
        /// </summary>
        [Display(Name = "Atualizado Por")]
        [ForeignKey("User")]
        public string LastUpdateUserID { get; set; }

        /// <summary>
        /// User that Made the Last Update of Terms And Conditions Section
        /// </summary>
        public User User { get; set; }


    }
}
