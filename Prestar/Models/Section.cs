using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Prestar.Models
{
    /// <summary>
    /// This class serves as a model for a section of the user manual, which has 
    /// a title, content, identification of the user manual and an illustration.
    /// </summary>
    /// <see cref="UserManual"/>
    public class Section
    {
        /// <summary>
        /// Section identification
        /// </summary>
        [Key]
        public int SectionID { get; set; }

        /// <summary>
        /// Section title
        /// </summary>
        [Required(ErrorMessage = "Deve colocar um titulo")]
        [Display(Name = "Titulo")]
        [StringLength(120, ErrorMessage = "O {0} deve conter no máximo {1} caracteres.")]
        public string Title { get; set; }

        /// <summary>
        /// Section content
        /// </summary>
        [Required(ErrorMessage = "Deve apresentar o conteudo")]
        [Display(Name = "Conteúdo")]
        public string Content { get; set; }

        /// <summary>
        /// User manual identification
        /// </summary>
        [Display(Name = "ID do Manual de Utilizador")]
        [ForeignKey("UserManual")]
        public int UserManualID { get; set; }

        /// <summary>
        /// Section illustration
        /// </summary>
        [Display(Name = "Imagem da Secção")]
        public byte[] Illustration { get; set; }

        /// <summary>
        /// User manual instance
        /// </summary>
        public UserManual UserManual { get; set; }
    }
}
