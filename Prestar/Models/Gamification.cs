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
    /// This class serves as a model for a gamification configuration, which has an 
    /// identification, a name and the number of points to be assigned for comments, 
    /// evaluation and requested service.
    /// </summary>
    public class Gamification
    {
        /// <summary>
        /// Gamification Configuration identification
        /// </summary>
        [Key]
        public int GamificationID { get; set; }

        /// <summary>
        /// Gamification Configuration name
        /// </summary>
        [Display(Name = "Nome da gamificação")]
        [Required(ErrorMessage ="Este campo é obrigatório")]
        [StringLength(20, ErrorMessage = "O {0} deve conter no máximo {1} caracteres.")]
        public String GamificationName { get; set; }

        /// <summary>
        /// Number of points per comment
        /// </summary>
        [Required(ErrorMessage = "Este campo é obrigatório")]
        [Display(Name = "Pontos por Comentário")]
        public int PointsPerComment { get; set; }

        /// <summary>
        /// Number of points per evaluation
        /// </summary>
        [Display(Name = "Pontos por Avaliação")]
        [Required(ErrorMessage = "Este campo é obrigatório")]
        public int PointsPerEvaluation { get; set; }

        /// <summary>
        /// Number of points per service
        /// </summary>
        [Display(Name = "Pontos por Serviço")]
        [Required(ErrorMessage = "Este campo é obrigatório")]
        public int PointsPerService { get; set; }

        /// <summary>
        /// Checks if the configuration is active
        /// </summary>
        [Display(Name ="Configuração Ativa?")]

        public Boolean IsActive { get; set; }

     

    }
}
