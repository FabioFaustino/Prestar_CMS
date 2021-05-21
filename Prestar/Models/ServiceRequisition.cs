using EntityFrameworkCore.Triggers;
using Prestar.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Prestar.Models
{
    /// <summary>
    /// This class serves as a model for requesting a service, which has a 
    /// customer, a service, a status, additional information, a creation date, 
    /// a date of the last update, the name of the user who made the last 
    /// update and the date of completion of the service.
    /// </summary>
    public class ServiceRequisition
    {
        /// <summary>
        /// Service request order ID
        /// </summary>
        [Key]
        [Display(Name = "Serviço Requisitado")]
        public int ServiceRequisitionID { get; set; }

        /// <summary>
        /// Ordering customer ID
        /// </summary>
        [Required]
        [ForeignKey("User")]
        [Display(Name = "Cliente")]
        public string RequisitionerID { get; set; }

        /// <summary>
        /// Service ID concerned
        /// </summary>
        [Required]
        [ForeignKey("Service")]
        [Display(Name = "Serviço")]
        public int ServiceID { get; set; }

        /// <summary>
        /// Request Status
        /// </summary>
        [Required (ErrorMessage = "Deve indicar um estado")]
        [EnumDataType(typeof(ServiceRequisitionStatus))]
        [Display(Name = "Estado")]
        public ServiceRequisitionStatus ServiceRequisitionStatus { get; set; }

        /// <summary>
        /// Additional request information
        /// </summary>
        [Display(Name = "Informação Adicional")]
        [StringLength(300, ErrorMessage = "O {0} deve conter no máximo {1} caracteres.")]
        public string AdditionalRequestInfo { get; set; }

        /// <summary>
        /// Creation Date of Request
        /// </summary>
        [Display(Name = "Data de Criação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Last updated time
        /// </summary>
        [Display(Name = "Última Atualização")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime LastUpdatedTime { get; set; }

        /// <summary>
        /// Last updated by
        /// </summary>
        [Display(Name = "Atualizado Por")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public string LastUpdatedBy { get; set; }

        /// <summary>
        /// Service
        /// </summary>
        public Service Service { get; set; }

        /// <summary>
        /// Client
        /// </summary>
        public User Requisitioner { get; set; }

        /// <summary>
        /// Conclusion Date (optional for Provider)
        /// </summary>
        [Display(Name = "Data de Conclusão")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? ConclusionDate { get; set; }

    }
}
