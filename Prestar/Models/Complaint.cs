using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Prestar.Models
{
    /// <summary>
    /// This class serves as a model for a complaint that has an identification, a 
    /// creation date, a type of complaint, a reason, a complaining user, a target 
    /// user of the complaint, a service target of the complaint, a variable that 
    /// defines whether the complaint is resolved or not and, if resolved, a variable 
    /// that saves the user who resolved the complaint, the explanation of the 
    /// resolution and the date of the resolution.
    /// </summary>
    /// <seealso cref="ComplaintType"/>
    public class Complaint
    {
        /// <summary>
        /// Complaint identification
        /// </summary>
        [Key]
        public int ComplaintID { get; set; }

        /// <summary>
        /// Complaint creation date
        /// </summary>
        [Display(Name = "Data de Criação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Type of the complaint
        /// </summary>
        [Required]
        [EnumDataType(typeof(ComplaintType))]
        [Display(Name = "Tipo de Denúncia")]
        public ComplaintType ComplaintType { get; set; }

        /// <summary>
        /// Reason of the complaint
        /// </summary>
        [Required(ErrorMessage = "Este campo é obrigatório")]
        [Display(Name = "Motivo")]
        [StringLength(500, ErrorMessage = "O {0} deve conter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 5)]
        public string Reason { get; set; }

        /// <summary>
        /// User that makes the complaint
        /// </summary>
        /// <see cref="User"/>
        [Display(Name = "Denunciante")]
        public User UserComplaining { get; set; }

        /// <summary>
        /// User target of the complaint
        /// </summary>
        /// <see cref="User"/>
        [Display(Name = "Denunciado")]
        public User ComplaintTargetUser { get; set; }

        /// <summary>
        /// Service target of the complaint
        /// </summary>
        /// <see cref="Service"/>
        [Display(Name ="Serviço")]
        public Service ComplaintTargetService { get; set; }

        /// <summary>
        /// Checks if the complaint is solved
        /// </summary>
        [Display(Name = "Resolvido?")]
        public bool IsSolved { get; set; }

        /// <summary>
        /// Complaint resolution date
        /// </summary>
        [Display(Name = "Data de Resolução")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime ResolutionDate { get; set; }

        /// <summary>
        /// Resolution explanation
        /// </summary>
        [Display(Name ="Resolução")]
        public String Resolution { get; set; }

        /// <summary>
        /// User that solved the complaint
        /// </summary>
        [Display(Name ="Resolvido por")]
        public User ResolvedBy { get; set; }

    }
}
