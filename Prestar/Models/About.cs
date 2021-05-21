using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prestar.Models
{
    /// <summary>
    /// This class represents the model of the information related to the About 
    /// section in the about page of the application. It will be possible to show 
    /// information related to everyone involved in the project, being part of the 
    /// client possible to edit.
    /// </summary>
    public class About
    {
        /// <summary>
        /// About Information Identification
        /// </summary>
        [Key]
        public int AboutID { get; set; }

        /// <summary>
        /// About Title
        /// </summary>
        [Required(ErrorMessage = "Deve colocar um título")]
        [StringLength(120, ErrorMessage = "O {0} deve conter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 2)]
        [Display(Name = "Título")]
        public string Title { get; set; }

        /// <summary>
        /// About content information
        /// </summary>
        [Required(ErrorMessage = "Deve colocar o conteúdo")]
        [Display(Name = "Conteúdo")]
        public string Content { get; set; }

        /// <summary>
        /// Complementary image of the About page section
        /// </summary>
        [Display(Name = "Imagem Complementar")]
        public byte[] Illustration { get; set; }

        /// <summary>
        /// Date of the last update of about section
        /// </summary>
        [Display(Name = "Última Atualização")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime AboutLastUpdate { get; set; }

        /// <summary>
        /// Identification of the last user who updated the about section
        /// </summary>
        [Display(Name = "Atualizado Por")]
        [ForeignKey("User")]
        public string LastUpdateUserID { get; set; }

        /// <summary>
        /// Last user who updated the About section
        /// </summary>
        public User User { get; set; }

    }
}
