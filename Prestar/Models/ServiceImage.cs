using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Prestar.Models
{
    /// <summary>
    /// This class serves as a model for a image that represents a service.
    /// This is meant to be a part of a photogallery, where a provider can upload images
    /// to show on the description of his service
    /// </summary>
    /// <see cref="Service"/>
    public class ServiceImage
    {
        /// <summary>
        /// Service image identification
        /// </summary>
        [Key]
        public int ServiceImageID { get; set; }

        /// <summary>
        /// Service Identification
        /// </summary>
        [ForeignKey("Service")]
        public int ServiceID { get; set; }

        /// <summary>
        /// Picture of the service
        /// </summary>
        [Display(Name = "Fotografia")]
        public byte[] Image { get; set; }

        /// <summary>
        /// Date of the image creation
        /// </summary>
        [Display(Name ="Carregada em")]

        public DateTime CreationDate { get; set; }

        /// <summary>
        /// List of service images
        /// </summary>
        /// <see cref="Service"/>
        public Service Service { get; set; }

    }
}
