using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Prestar.Models
{
    /// <summary>
    /// This class serves as a model for a news item, which has a title, an optional second 
    /// title, a description, an image, a creation date, a variable that checks the visibility 
    /// of the news, a variable that checks whether the news is main and the identification 
    /// of writings.
    /// </summary>
    public class New
    {
        /// <summary>
        /// New identification
        /// </summary>
        [Key]
        public int NewsID { get; set; }

        /// <summary>
        /// New title
        /// </summary>
        [Required(ErrorMessage = "Deve dar um titulo à Notícia")]
        [Display(Name = "Titulo")]
        [StringLength(120, ErrorMessage = "O {0} deve conter no máximo {1} caracteres.")]
        public string Title { get; set; }

        /// <summary>
        /// New second title
        /// </summary>
        [Display(Name = "Segundo Titulo")]
        [StringLength(100, ErrorMessage = "O {0} deve conter no máximo {1} caracteres.")]
        public string SecondTitle { get; set; }

        /// <summary>
        /// New description
        /// </summary>
        [Required(ErrorMessage = "A noticia deve mostrar detalhes")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }

        /// <summary>
        /// New illustration
        /// </summary>
        [Display(Name = "Imagem da Notícia")]
        public byte[] Illustration { get; set; }

        /// <summary>
        /// New creation date
        /// </summary>
        [Display(Name = "Data de Criação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Checks new visibility
        /// </summary>
        [Display(Name = "Visibilidade")]
        [DefaultValue(false)]
        public bool Visible { get; set; }

        /// <summary>
        /// Checks if new is principal
        /// </summary>
        [Display(Name = "Noticia Principal")]
        [DefaultValue(false)]
        public bool PrincipalNew { get; set; }

        /// <summary>
        /// New writter identification
        /// </summary>
        [ForeignKey("Writter")]
        public string WriterID { get; set; }

        /// <summary>
        /// New writer object
        /// </summary>
        public User Writter { get; set; }

    }
}
