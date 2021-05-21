using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Prestar.Models
{
    /// <summary>
    /// This class serves as a model for a service, which has a user who will be 
    /// the provider, the category identification, the name, the description, the 
    /// image, the creation date, a variable that controls whether the service is 
    /// blocked, the reason for the service. service is blocked, a variable that 
    /// checks whether the service is active,a list of comments and evaluations
    /// and a list of images that represent the service.
    /// </summary>
    /// <see cref="CommentAndEvaluation"/>
    /// <see cref="ServiceImage"/>
    public class Service
    {
        /// <summary>
        /// Service Identification
        /// </summary>
        [Key]
        public int ServiceID { get; set; }

        /// <summary>
        /// Service provider identification
        /// </summary>
        [Display(Name = "Prestador")]
        [ForeignKey("User")]
        public string UserID { get; set; }

        /// <summary>
        /// Service category identification
        /// </summary>
        [Required(ErrorMessage ="Deve escolher uma categoria!")]
        [Display(Name = "Categoria")]
        [ForeignKey("ServiceCategory")]
        public int ServiceCategoryID { get; set; }

        /// <summary>
        /// Service name
        /// </summary>
        [Required(ErrorMessage = "O nome do serviço é obrigatório!")]
        [Display(Name = "Nome do Serviço")]
        [StringLength(120, ErrorMessage = "O {0} deve conter no máximo {1} caracteres.")]
        public string ServiceName { get; set; }

        /// <summary>
        /// Service description
        /// </summary>
        [Required(ErrorMessage = "A descrição é obrigatória!")]
        [Display(Name = "Descrição")]
        [StringLength(1000, ErrorMessage = "O {0} deve conter no máximo {1} caracteres.")]
        public string Description { get; set; }

        /// <summary>
        /// Service image
        /// </summary>
        [Display(Name = "Imagem do serviço")]
        public byte[] Illustration { get; set; }

        /// <summary>
        /// Service creation date time
        /// </summary>
        [Display(Name = "Data de Criação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Checks if service is blocked
        /// </summary>
        [Display(Name ="Serviço Bloqueado?")]
        [DefaultValue(false)]
        public bool IsBlocked { get; set; }

        /// <summary>
        /// Service block motive
        /// </summary>
        [Display(Name ="Motivo do bloqueio")]
        [DefaultValue("")]
        public String BlockMotive { get; set; }

        /// <summary>
        /// Checks if service is active
        /// </summary>
        [Display(Name = "Serviço Ativo?")]
        [DefaultValue(true)]
        public bool IsActive { get; set; }

        /// <summary>
        /// Service provider instance
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Service Category instance
        /// </summary>
        public ServiceCategory ServiceCategory { get; set; }

        /// <summary>
        /// List of service comments and evalutions
        /// </summary>
        /// <see cref="List{T}"/>
        /// <see cref="CommentAndEvaluation"/>
        public List<CommentAndEvaluation> CommentsAndEvaluations {get; set; }

        /// <summary>
        /// List of service images
        /// </summary>
        /// <see cref="List{T}"/>
        /// <see cref="ServiceImage"/>
        public List<ServiceImage> ServiceImages { get; set; }

    }
}
