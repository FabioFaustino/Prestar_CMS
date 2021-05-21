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
    /// This class serves as a model for requesting an account change and for 
    /// requesting the addition of a subcategory. It has the identification of the 
    /// user making the request, the type of request, the description, the date 
    /// of creation, the identification of the user who responds to the request, 
    /// the status of the request and the reason for the rejection.
    /// </summary>
    /// <seealso cref="User"/>
    public class Request
    {
        /// <summary>
        /// Request identification
        /// </summary>
        [Key]
        public int RequestID { get; set; }

        /// <summary>
        /// Requisitioner identification
        /// </summary>
        [Required(ErrorMessage = "Este campo é de preenchimento obrigatório")]
        [ForeignKey("User")]
        public string  RequisitionerID { get; set; }

        /// <summary>
        /// Request type
        /// </summary>
        [Required(ErrorMessage = "Este campo é de preenchimento obrigatório")]
        [EnumDataType(typeof(RequestType))]
        [Display(Name = "Tipo de Pedido")]
        public RequestType RequestType { get; set; }

        /// <summary>
        /// Request description
        /// </summary>
        [Required(ErrorMessage = "Este campo é de preenchimento obrigatório")]
        [Display(Name = "Descrição")]
        [StringLength(1000, ErrorMessage = "O {0} deve conter no máximo {1} caracteres.")]
        public string Description { get; set; }

        /// <summary>
        /// Request creation date time
        /// </summary>
        [Display(Name ="Data do pedido")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreationDateTime { get; set; }

        /// <summary>
        /// Request handler user identification
        /// </summary>
        [Display(Name = "Aprovado por")]
        public string RequestHandlerID { get; set; }

        /// <summary>
        /// Request handle date
        /// </summary>
        [Display(Name = "Data da Resposta")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime HandleDateTime {get;set;}

        /// <summary>
        /// Request status
        /// </summary>
        [Required(ErrorMessage = "Este campo é de preenchimento obrigatório")]
        [EnumDataType(typeof(RequestStatus))]
        [Display(Name = "Estado do Pedido")]
        public RequestStatus RequestStatus { get; set; }

        /// <summary>
        /// Request rejection motive
        /// </summary>
        [Display(Name ="Motivo de rejeição")]
        [StringLength(1000, ErrorMessage = "O {0} deve conter no máximo {1} caracteres.")]
        public string RejectionMotive { get; set; }

        /// <summary>
        /// Requisitioner instance
        /// </summary>
        public User Requisitioner { get; set; }

        /// <summary>
        /// Requisition handler instance
        /// </summary>
        public User Handler { get;  set; }


    }
}
