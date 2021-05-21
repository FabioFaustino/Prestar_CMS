using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Prestar.Data;
using Prestar.Models;
using Prestar.Validations;

namespace Prestar.Areas.Identity.Pages.Account.Manage
{

    /// <summary>
    /// This class represents the index page of the user's profile.
    /// This class inherits from the PageModel class, which is an abstract class that 
    /// represents a page.
    /// <see cref="PageModel"/>
    /// </summary>
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor of the IndexModel class that receives three 
        /// parameters and initializes them.
        /// </summary>
        /// <param name="userManager">
        /// Provides the APIs for managing user in a persistence store.
        /// <see cref="UserManager{TUser}"/>
        /// </param>
        /// <param name="signInManager">
        /// Provides the APIs for user sign in.
        /// <see cref="SignInManager{TUser}"/>
        /// </param>
        /// <param name="context">
        /// Represents application database context
        /// <see cref="ApplicationDbContext"/>
        /// </param>
        public IndexModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        /// <summary>
        /// Property that represents user's username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Property that allows to get the request status message and providing 
        /// feedback to the user after a form submission that results in the user 
        /// being redirected to another page.
        /// </summary>
        /// <seealso cref="TempDataAttribute"/>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Property that allows to obtain an instance of the InputModel class.
        /// </summary>
        /// <see cref="InputModel"/>
        /// <seealso cref="BindPropertyAttribute"/>
        [BindProperty]
        public InputModel Input { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public class InputModel : User
        {

            [Display(Name = "Nome")]
            [StringLength(20, ErrorMessage = "O {0} deve conter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 2)]
            // [RegularExpression("[a-zA-Z]*", ErrorMessage = "Introduza um apelido válido")] - desta forma a acentuação não poderia ser usado, guardado para referencia regex
            override public string FirstName { get; set; }


            [Display(Name = "Apelido")]

            [StringLength(20, ErrorMessage = "O {0} deve conter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 2)]
            override public string LastName { get; set; }

            
            /// <summary>
            /// This class is responsible for representing the User model, which contains 
            /// the first name (FirstName), the last name (LastName) and the phone 
            /// number (PhoneNumber).
            /// </summary>
            [Phone]
            [Display(Name = "Nº de Telefone")]
            override public string PhoneNumber { get; set; }

        }

        /// <summary>
        /// This asynchronous method allows to obtain the data of the authenticated user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Returns a Task</returns>
        /// <seealso cref="Task"/>
        private async Task LoadAsync(User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Input = new InputModel
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Birthdate = user.Birthdate,
                ZipCode = user.ZipCode,
                PhoneNumber = phoneNumber,
                ProfilePicture = user.ProfilePicture
            };
        }

        /// <summary>
        /// Asynchronous method that represents the HTTP GET method, where 
        /// it is used to request data from a specific resource. In this case, the 
        /// form for managing profile data. 
        /// If the user in question is not authenticated, a page Not Found (404) 
        /// is obtained. 
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            //ViewBag for bell icon on the nav menu
            ViewData["HasNotificationToRead"] = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == user.Id && !notif.IsRead).Count();

            await LoadAsync(user);
            return Page();
        }

        /// <summary>
        /// Asynchronous method that represents an HTTP POST method, which 
        /// allows data to be sent to the server in order to create and, in this case, 
        /// update a resource, in this case, the profile data.
        /// If the user is not authenticated, a page Not Found (404) is obtained.
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (Input.UserName != user.UserName)
            {
                user.UserName = Input.UserName;
                await _userManager.UpdateAsync(user);
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Houve um erro ao tentar guardar o seu numero de telefone.";
                    return RedirectToPage();
                }
            }
            if (Input.FirstName != user.FirstName)
            {
                user.FirstName = Input.FirstName;
                await _userManager.UpdateAsync(user);
            }

            if (Input.LastName != user.LastName)
            {
                user.LastName = Input.LastName;
                await _userManager.UpdateAsync(user);
            }

            if (Input.Birthdate != user.Birthdate)
            {
                user.Birthdate = Input.Birthdate;
                await _userManager.UpdateAsync(user);
            }
           

            if (Input.ZipCode != user.ZipCode)
            {
                user.ZipCode = Input.ZipCode;
                await _userManager.UpdateAsync(user);
            }

            if (Request.Form.Files.Count > 0)
            {
                IFormFile file = Request.Form.Files.FirstOrDefault();
                using (var dataStream = new MemoryStream())
                {
                    await file.CopyToAsync(dataStream);
                    user.ProfilePicture = dataStream.ToArray();
                }
                await _userManager.UpdateAsync(user);
            }
           
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "O seu perfil foi atualizado.";
            return RedirectToPage();
        }
    }
}
