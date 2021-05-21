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
    /// This class serves as a model for a customer's comment and assessment 
    /// of a service. The model includes the identification of the comment and 
    /// evaluation, the identification of the user who comments and evaluates, 
    /// the identification of the commented and evaluated service, the evaluation, 
    /// the comment, the date of creation, a variable that checks whether the 
    /// comment has been removed, the date the last modification of the comment 
    /// and / or assessment and a variable that checks whether the comment and 
    /// assessment have been edited.
    /// </summary>
    public class CommentAndEvaluation
    {
        /// <summary>
        /// Comment and evaluation identification
        /// </summary>
        [Key]
        public int CommentAndEvaluationID { get; set; }
        
        /// <summary>
        /// Identification of the user that comments and evaluates
        /// </summary>
        [ForeignKey("User")]
        [Display(Name = "Utilizador")]
        public string UserCommentingID { get; set; }

        /// <summary>
        /// Identification of the commented/evaluated service
        /// </summary>
        [Required]
        [ForeignKey("Service")]
        [Display(Name = "ID do Serviço")]
        public int ServiceID { get; set; }

        /// <summary>
        /// Evaluation given.
        /// It's a number between 0 and 5.
        /// </summary>
        [Display(Name = "Avaliação")]
        [Range(0, 5, ErrorMessage = "A avaliação atribuida deve estar entre 0 e 5")]
        public int Evaluation { get; set; }

        /// <summary>
        /// Comment given.
        /// Has a limit of 500 characters.
        /// </summary>
        [Display(Name ="Comentário")]
        [StringLength(500, ErrorMessage ="O comentário não deve exceder os {1} caracteres.")]
        public string Comment { get; set; }

        /// <summary>
        /// Comment/Evaluation creation date
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        [DataType(DataType.Date)]
        [Display(Name = "Criado")]
        public  DateTime CreationDate { get; set; }

        /// <summary>
        /// Checks if a comment has been removed
        /// </summary>
        [Display(Name ="Comentário removido?")]
        [DefaultValue(false)]
        public bool IsRemoved { get; set; }

        /// <summary>
        /// Comment/Evaluation last update date time
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        [DataType(DataType.Date)]
        [Display(Name = "Editado em")]
        public  DateTime LastUpdate { get; set; }

        /// <summary>
        /// Checks if the comment/evaluation has been edited
        /// </summary>
        [DefaultValue(false)]
        public bool IsEdited { get; set; }

        /// <summary>
        /// User object that commented/evaluated
        /// </summary>
        public User UserCommenting { get; set; }

        /// <summary>
        /// Service object that was commented/evaluated
        /// </summary>
        public Service Service { get; set; }
    }
}
