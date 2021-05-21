using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Prestar.Models;

namespace Prestar.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// This class is responsible for representing a model to change a user's 
    /// account type and for having actions that make this change possible, using 
    /// HTTP GET and POST methods.
    /// This class inherits from the PageModel class, which is an abstract class that 
    /// represents a page.
    /// <see cref="PageModel"/>
    /// </summary>
    public class ChangeTypeAccountModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;
        private readonly Data.ApplicationDbContext _context;

        /// <summary>
        /// Constructor of the ChangeTypeAccountModel class that receives five parameters 
        /// and initializes them.
        /// </summary>
        /// <param name="userManager">
        /// Provides the APIs for managing user in a persistence store.
        /// <see cref="UserManager{TUser}"/>
        /// </param>
        /// <param name="signInManager">
        /// Provides the APIs for user sign in.
        /// <see cref="SignInManager{TUser}"/>
        /// </param>
        /// <param name="logger">
        /// Represents a type used to perform logging.
        /// <see cref="ILogger"/>
        /// </param>
        /// <param name="context">
        /// Application database context
        /// <see cref="ApplicationDbContext"/>
        /// </param>
        public ChangeTypeAccountModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<ChangePasswordModel> logger,
            Data.ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

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
        /// Class that serves as a model for the data to be obtained from the form 
        /// filled in by the user, when changing the account type. The data is the 
        /// description (Description).
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Property that represents a required field for a description.
            /// </summary>
            [Required(ErrorMessage = "É obrigatório preencher a descrição")]
            [DataType(DataType.Text)]
            [Display(Name = "Descrição")]
            public string Description { get; set; }
        }

        /// <summary>
        /// Asynchronous method that represents an HTTP POST method, which 
        /// allows data to be sent to the server in order to create and, in this case, 
        /// update a resource, in this case, the account type.
        /// If the user is not authenticated, a page Not Found (404) is obtained. 
        /// If the user's account type is that of a customer and the model of the data 
        /// entered is valid, a new account change request is created and added to 
        /// the database. Finally, the user in question is notified, using the private 
        /// SendNotification method.
        /// If the user's account type is that of a provider, the type of account is 
        /// changed to the type of customer account.
        /// If the user's account type is none of the above, the user remains on the 
        /// form for changing the account type.
        /// If the request to change the account is successful, the user is redirected 
        /// to the page where the updated information is reported.
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

            if (User.IsInRole("Cliente") && ModelState.IsValid)
            {
                
                int idNew = 0;
                try
                {
                    idNew = _context.Request.OrderBy(r => r.RequestID).Select(r => r.RequestID).First() + 1;
                }
                catch (InvalidOperationException)
                {
                    idNew = 1;
                }

                string descrip = Input.Description;
                if (descrip == null || descrip.Equals(""))
                {
                    descrip = "Não apresentou motivo.";
                }

                Request request = new()
                {
                    RequisitionerID = user.Id,
                    RequestType = RequestType.ProvideServices,
                    Description = descrip,
                    CreationDateTime = DateTime.Now,
                    RequestStatus = RequestStatus.WaitingApproval, RejectionMotive = ""
                };
                _context.Request.Add(request);
                _context.SaveChanges();
                SendNotification(user);

            }
            else if (User.IsInRole("Prestador"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Prestador");
                await _userManager.AddToRoleAsync(user, "Cliente");
            }
            else
            {
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("O seu pedido foi enviado com sucesso.");
            StatusMessage = "O seu pedido foi enviado com sucesso.";

            return RedirectToPage();
        }

        /// <summary>
        /// This method allows you to create and send a notification, with the 
        /// necessary data related to the operation of changing the type of account. 
        /// The process of sending the notification to moderators involves adding it 
        /// to the database and showing it whenever requested.
        /// </summary>
        /// <param name="user">User asking to update account type.</param>
        /// <see cref="User"/>
        /// <see cref="Notification"/>
        private void SendNotification(User user)
        {
            var subject = "Foi feito um novo pedido de alteração de conta para prestador";
            var content = user.FirstName + " " + user.LastName + " fez um pedido de alteração de conta. Por favor responda o quanto antes.";

            //NotificationsController notificationController = new NotificationsController(_context, _userManager);

            /*52c6c3c1-843a-4baf-b12f-c8f39ba31fcb -> ID do Role Moderador*/
            List<string> ids = _context.UserRoles
                .Where(ur => ur.RoleId == "52c6c3c1-843a-4baf-b12f-c8f39ba31fcb")
                .Select(ur => ur.UserId).ToList();
            ids.ForEach(id =>
            {
                Notification notification = new()
                {
                    DestinaryID = id,
                    Subject = subject,
                    Content = content,
                    IsRead = false,
                    Action = "/Requests",
                    CreationDate = DateTime.Now,
                };
                //var result = notificationController.Create(notification).Result;
                _context.Add(notification);
                _context.SaveChanges();
            });
        }
    }
}