using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Prestar.Models
{
    /// <summary>
    /// This class serves as a model for a formation, which has an identification, 
    /// the number of registered users, the duration in minutes, the date of the 
    /// occurrence, the title, the content, the maximum number of registrations, 
    /// the location, the identification of the responsible user, the image and a 
    /// list of subscriptions.
    /// </summary>
    /// <see cref="Enrollment"/>
    public class Formation
    {
        /// <summary>
        /// Formation identification
        /// </summary>
        [Key]
        public int FormationID { get; set; }

        /// <summary>
        /// Number of registrations
        /// </summary>
        [Display(Name = "Número de Inscrições")]
        public int NumberOfRegistrations { get; set; }

        /// <summary>
        /// Formation duration in minutes
        /// </summary>
        [Display(Name = "Duração (em Minutos)")]
        public int DurationMinutes { get; set; }

        /// <summary>
        /// Date of the occurrence
        /// </summary>
        [Display(Name = "Data da Formação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Title of the formation
        /// </summary>
        [Required(ErrorMessage = "Deve colocar um titulo")]
        [Display(Name = "Titulo")]
        [StringLength(100, ErrorMessage = "O {0} deve conter no máximo {1} caracteres.")]
        public string Title { get; set; }

        /// <summary>
        /// Formation content
        /// </summary>
        [Required(ErrorMessage = "Deve apresentar o conteudo")]
        [Display(Name = "Conteúdo")]
        public string Content { get; set; }

        /// <summary>
        /// Maximum number of enrollments
        /// </summary>
        [Required(ErrorMessage = "Deve incluir o máximo de inscrições permitidas")]
        [Display(Name = "Máximo de Inscrições")]
        [Range(0, 100, ErrorMessage = "Valor máximo de inscrições deve ser entre {1} e {2}.")]
        public int MaxEnrollment { get; set; }

        /// <summary>
        /// Formation local
        /// </summary>
        [Required(ErrorMessage = "Deve incluir o local da formação")]
        [Display(Name = "Local")]
        public string Local { get; set; }

        /// <summary>
        /// Responsible User identification
        /// </summary>
        [Display(Name = "ID do Responsável pela Formação")]
        [ForeignKey("Responsible")]
        public string ResponsibleID { get; set; }

        /// <summary>
        /// Formation illustration
        /// </summary>
        [Display(Name = "Imagem da Formação")]
        public byte[] Illustration { get; set; }

        /// <summary>
        /// Responsible user object
        /// </summary>
        public User Responsible { get; set; }

        /// <summary>
        /// List of formation enrollments
        /// </summary>
        public List<Enrollment> Enrollments { get; set; }
    }
}