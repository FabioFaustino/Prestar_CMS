using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Prestar.Models
{
    /// <summary>
    /// This class represents a section with information related to a 
    /// sub-theme of the privacy policy. 
    /// A section has a title and content.
    /// </summary>
    public class PrivacyPolicySection
    {
        /// <summary>
        /// Privacy policy section identification
        /// </summary>
        [Key]
        public int PrivacyPolicySectionID { get; set; }

        /// <summary>
        /// Privacy policy section title
        /// </summary>
        [Required(ErrorMessage = "Deve colocar um título")]
        [Display(Name = "Título")]
        [StringLength(120, ErrorMessage = "O {0} deve conter no máximo {1} caracteres.")]
        public string Title { get; set; }

        /// <summary>
        /// Privacy policy section content
        /// </summary>
        [Required(ErrorMessage = "Deve apresentar o conteúdo")]
        [Display(Name = "Conteúdo")]
        public string Content { get; set; }

        /// <summary>
        /// Date of the last update of the privacy policy
        /// </summary>
        [Display(Name = "Última Atualização")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime PrivacyPolicySectionLastUpdate { get; set; }

        /// <summary>
        /// Identification of the last user who updated the privacy policy
        /// </summary>
        [Display(Name = "Atualizado Por")]
        [ForeignKey("User")]
        public string LastUpdateUserID { get; set; }

        /// <summary>
        /// Last user who updated the privacy policy
        /// </summary>
        public User User { get; set; }
    }
}
