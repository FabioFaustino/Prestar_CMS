using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prestar.Models
{
    /// <summary>
    /// This class serves as a model for a notification, which has an identification, 
    /// the recipient's idnetification, the subject's title, the message's content, a 
    /// variable that checks if it has already been read, the notification action and 
    /// the creation date.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Notification identification
        /// </summary>
        [Key]
        public int NotificationID { get; set; }

        /// <summary>
        /// Destinary user identification
        /// </summary>
        [Display(Name = "ID do Destinatário")]
        [ForeignKey("User")]
        public string DestinaryID { get; set; }

        /// <summary>
        /// Notification subject
        /// </summary>
        [Display(Name = "Assunto")]
        [StringLength(120, ErrorMessage = "O {0} deve conter no máximo {1} caracteres.")]
        public string Subject { get; set; }

        /// <summary>
        /// Notification content
        /// </summary>
        [Display(Name = "Conteúdo")]
        [StringLength(5000, ErrorMessage = "O {0} deve conter no máximo {1} caracteres.")]
        public string Content { get; set; }

        /// <summary>
        /// Checks if the notification was read
        /// </summary>
        [Display(Name = "Visto")]
        public bool IsRead { get; set; }

        /// <summary>
        /// Notification action
        /// </summary>
        [Display(Name = "Ação")]
        public string Action { get; set; }

        /// <summary>
        /// Notification creation date
        /// </summary>
        [Display(Name = "Data de Criação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Destinary user object
        /// </summary>
        public User User { get; set; }

    }
}
