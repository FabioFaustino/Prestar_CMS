using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Prestar.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace Prestar.Models
{
    /// <summary>
    /// This class serves as a model for user identification, which inherits from the 
    /// IdentityUser class. The User class has an initial name, a nickname, a date 
    /// of birth, a username, postal code, a profile photo, an account creation date, 
    /// a date of last authentication, a variable that controls notification settings, a 
    /// variable that controls the visibility of the email, a variable that controls the 
    /// visibility of the phone number, the number of total points, the reason for the 
    /// account lockout and the number of days of account lockout.
    /// </summary>
    /// <see cref="IdentityUser"/>
    public class User : IdentityUser
    {
        
       /// <summary>
       /// User first name
       /// </summary>
        [Display(Name = "Nome")]
        [StringLength(20, ErrorMessage = "O {0} deve conter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 2)]
      
        public virtual String FirstName { get; set; }

        /// <summary>
        /// User last name
        /// </summary>
        [Display(Name = "Apelido")]
        [StringLength(20, ErrorMessage = "O {0} deve conter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 2)]
        public virtual String LastName { get; set; }

        /// <summary>
        /// User birthdate
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        [DataType(DataType.Date)]
        [DOBDateValidation]
        [Display(Name = "Data de Nascimento")]
        public virtual DateTime Birthdate { get; set; }

        /// <summary>
        /// User username
        /// </summary>
        [Required(ErrorMessage = "Este campo é de preenchimento obrigatório")]
        [Display(Name="Nome de utilizador")]
        [RegularExpression(@"^[A-z][A-z0-9|\.|\s]+$", ErrorMessage = "O nome de utilizador só pode conter caracteres alfanuméricos e pontos")]
        [StringLength(20, ErrorMessage = "O {0} deve conter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 2)]
        override public  String UserName { get; set; }

        /// <summary>
        /// User zipcode
        /// </summary>
        [Display(Name = "Código Postal")]
        [RegularExpression("^\\d{4}[- ]{0,1}\\d{3}$", ErrorMessage = "Introduza um codigo postal válido. \nCaso não queira fornecer o código postal exato pode sempre colocar -000 nos ultimos 3 digitos")]
        public virtual String ZipCode { get; set; }

        /// <summary>
        /// User profile picture
        /// </summary>
        [Display(Name = "Fotografia de Perfil")]
        public byte[] ProfilePicture { get; set; }

        /// <summary>
        /// User account creation date
        /// </summary>
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        [Display(Name = "Data de criação de conta")]
        public DateTime AccountCreationDate { get; set; }

        /// <summary>
        /// User last activity
        /// </summary>
        [Display(Name ="Última atividade")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime LastSeen { get; set; }

        /// <summary>
        /// Checks if user wants to receive notifications
        /// </summary>
        [Display(Name = "Receber Notificações?")]
        [DefaultValue(true)]
        public virtual Boolean ReceiveNotifications { get; set; }

        /// <summary>
        /// Checks if user wants to make email visible
        /// </summary>
        [Display(Name = "Mostrar email?")]
        [DefaultValue(true)]
        public virtual Boolean ShowEmail { get; set; }

        /// <summary>
        /// Checks if user wants to make phone number visible
        /// </summary>
        [Display(Name = "Mostrar contacto telefónico?")]
        [DefaultValue(true)]
        public virtual Boolean ShowPhoneNumber { get; set; }

        /// <summary>
        /// User total points
        /// </summary>
        [Display(Name = "Total de pontos obtidos")]
        [DefaultValue(0)]
        public virtual int TotalPoints { get; set; }

        /// <summary>
        /// User access block motive
        /// </summary>
        [Display(Name ="Motivo de Bloqueio")]
        public virtual String BlockMotive { get; set; }

        /// <summary>
        /// User access block number of days
        /// </summary>
        [Display(Name ="Dias de Bloqueio")]
        [DefaultValue(0)]
        public virtual int? LockoutDays { get; set; }

        /// <summary>
        /// Flag used to know if user decided to remove his account from the application.
        /// For statistical and integrational purposes, the user personal data will be removed,
        /// but account information will be kept.
        /// </summary>
        [Display(Name = "Conta Removida?")]
        [DefaultValue(false)]
        public bool HasRemovedAccount { get; set; }

    }
}
