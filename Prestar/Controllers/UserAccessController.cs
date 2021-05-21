using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prestar.Data;
using Prestar.Models;
using Prestar.Services;

namespace Prestar.Controllers
{
    /// <summary>
    /// User Acces Controller
    /// </summary>
    /// <see cref="UserAccessViewModel"/>
    [Authorize(Roles = "Administrador, Moderador")]
    public class UserAccessController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;

        /// <summary>
        /// Constructor of the UserAccessController class that receives two parameters 
        /// and initializes them.
        /// </summary>
        /// <param name="userManager">
        /// Provides the APIs for managing user in a persistence store.
        /// <see cref="UserManager{TUser}"/>
        /// </param>
        /// <param name="roleManager">
        /// Provides the APIs for managing roles in a persistence store.
        /// <see cref="RoleManager{TRole}"/>
        /// </param>
        /// <param name="context">
        /// Application database context
        /// <see cref="ApplicationDbContext"/>
        /// </param>
        /// <param name="emailSender">
        /// This API supports the ASP.NET Core Identity default UI infrastructure and is 
        /// not intended to be used directly from your code.
        /// <see cref="IEmailSender"/>
        /// </param>
        public UserAccessController( UserManager<User> userManager, 
            RoleManager<IdentityRole> roleManager, 
            ApplicationDbContext context,
            IEmailSender emailSender)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Presents a list with all the users and their roles
        /// </summary>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET: UserAccess
        [Authorize(Roles = "Moderador,Administrador")]
        public async Task<IActionResult> Index(int? pageNumber)
        {
            int pageSize = 20;
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new List<UserAccessViewModel>();
            foreach (User user in users)
            {
                var thisViewModel = new UserAccessViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    AccountCreationDate = user.AccountCreationDate.Date,
                    LockoutEnd = user.LockoutEnd,
                    Roles = await GetUserRoles(user)
                };
                userRoles.Add(thisViewModel);
            }

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(await PaginatedList<UserAccessViewModel>.CreateAsync(userRoles, pageNumber ?? 1, pageSize));
        }

        /// <summary>
        /// Returns a  List with all the roles currently associated with the use
        /// </summary>
        /// <param name="user">User object</param>
        /// <returns>List<string></returns>
        [Authorize(Roles = "Moderador,Administrador")]
        public async Task<List<string>> GetUserRoles(User user)
        {
            var result = await _userManager.GetRolesAsync(user);
            if(result != null)
                 return new List<string>(await _userManager.GetRolesAsync(user));

            return new List<string>();
        }

        /// <summary>
        /// Get the user from the id, compose the model with the user and all roles
        /// Checks for the roles the user currently has, and select them so they appear checked in view
        /// </summary>
        /// <param name="userId">User identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // Get: UserAccess/askf234-asfgdbgdsfcf-jafbv
        [Authorize(Roles = "Administrador, Moderador")]
        public async Task<IActionResult> Manage(string userId)
        {
            ViewBag.userId = userId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("/Views/Shared/NotFound.cshtml");
            }
            
            ViewBag.UserName = user.UserName;
            var model = new List<ManageUserRolesViewModel>();

            var roles = await _userManager.GetRolesAsync(user);
            if (roles == null)
                return View("/Views/Shared/NotFound.cshtml");

            foreach (var role in _roleManager.Roles)
            {
                var userRoles = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if (roles.Contains(role.Name))
                {
                    userRoles.Selected = true;
                }
                else
                {
                    userRoles.Selected = false;
                }
                model.Add(userRoles);
            }
            
            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(model);
        }

        /// <summary>
        /// Updates the roles the user is assigned to, based on the selection
        /// </summary>
        /// <param name="model">List of ManageUserRolesModel</param>
        /// <param name="userId">User idnetification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // Edit : UserAcess/asfasfar21r512-afafqf-asffqfq
        [HttpPost]
        [Authorize(Roles = "Moderador,Administrador")]
        public async Task<IActionResult> Manage(List<ManageUserRolesViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View();
            }
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("RemoveError", "Cannot remove user existing roles");
                return View(model);
            }
            result = await _userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Shows the details of the selected user, and asks for a confirmation for delete
        /// </summary>
        /// <param name="userId">User identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // GET : UserAcess/ovnoasn12-ocnwoefvn2
        [Authorize(Roles = "Moderador,Administrador")]
        public async Task<IActionResult> Delete(string userId)
        {
            if (userId == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var user = await _userManager.FindByIdAsync(userId);
                          

            if (user == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(user);
        }

        /// <summary>
        /// Removes the user from the roles table, and then removes the user itself
        /// </summary>
        /// <param name="userId">User identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        // POST: UserRoles/Delete/abcdsf223-asdff2
        [Authorize(Roles = "Moderador,Administrador")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string userId)
        {
            if (userId == null)
                return View("/Views/Shared/NotFound.cshtml");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return View("/Views/Shared/NotFound.cshtml");

            var roles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
                return View("/Views/Shared/NotFound.cshtml");

            await _userManager.DeleteAsync(user);
               
            
            return RedirectToAction(nameof(Index));

        }

        /// <summary>
        /// When trying to block or unblock a user, this will render the view with the detailed activity of that user, in
        /// order for the moderators to take informed action,
        /// </summary>
        /// <param name="userId">User identification</param>
        /// <param name="complaintID">Complaint identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> UserActivity(string userId, int? complaintID)
        {
            if (userId == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }
               
            ViewBag.ComplaintID = complaintID;

            var user = await _userManager.FindByIdAsync(userId);

            var thisViewModel = new UserAccessViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Birthdate = user.Birthdate,
                Email = user.Email,
                AccountCreationDate = user.AccountCreationDate,
                LastSeen = user.LastSeen,
                LockoutDays = user.LockoutDays,
                BlockMotive = user.BlockMotive,
                LockoutEnd = user.LockoutEnd,
                ComplaintID = complaintID
            };

           
            ViewBag.Services = await _context.Service.Where(u => u.User == user).CountAsync();
            ViewBag.ServicesRequestedTo = await _context.ServiceRequisition.Where(u => u.Service.User == user).CountAsync();
            ViewBag.ServicesRequestedBy = await _context.ServiceRequisition.Where(u => u.Requisitioner == user).CountAsync();
            ViewBag.ComplaintsMade = await _context.Complaint.Where(u => u.UserComplaining == user).CountAsync();
            ViewBag.ComplaintsReceived = await _context.Complaint.Where(u => u.ComplaintTargetUser == user || u.ComplaintTargetService.User == user).CountAsync();
            ViewBag.CommentAndEvaluationsMade = await _context.CommentAndEvaluation.Where(u => u.UserCommenting == user).CountAsync();
            ViewBag.CommentAndEvaluationsReceived = await _context.CommentAndEvaluation.Where(u => u.Service.User == user).CountAsync();

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(thisViewModel);

        }

        /// <summary>
        /// This method displays a user's profile details
        /// </summary>
        /// <param name="userId">User identification</param>
        /// <param name="requestID">Request identification</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        public async Task<IActionResult> UserDetails(string userId, int? requestID)
        {
            if (userId == null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            var request = await _context.Request.FindAsync(requestID);
           
            if(request != null)

            ViewBag.RequestID = requestID;
       
            var user = await _userManager.FindByIdAsync(userId);
            

            var thisViewModel = new User
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Birthdate = user.Birthdate,
                Email = user.Email,
                AccountCreationDate = user.AccountCreationDate,
                LastSeen = user.LastSeen,
            };

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return View(thisViewModel);

        }

        /// <summary>
        /// This method Confirms the blocking of a user for a given ammount of days
        /// When a user gets blocked, he won't be able to login into the application and all the services
        /// that user had ongoing (either pending or accepted) are cancelled.
        /// In case the blocking of the user comes from a Complaint, that complaint will be updated with the action details
        /// </summary>
        /// <param name="userAccessViewModel">UserAcessViewModel object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Moderador,Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Block([Bind("Id,UserName,FirstName,LastName,Birthdate,Email,AccountCreationDate,LastSeen,LockoutDays,BlockMotive,ComplaintID")]UserAccessViewModel userAccessViewModel)
        {

            var daysToBlock = userAccessViewModel.LockoutDays;
            var blockMotive = userAccessViewModel.BlockMotive;
            var complaintID = userAccessViewModel.ComplaintID;

            var user= await _userManager.FindByIdAsync(userAccessViewModel.Id);

            Complaint complaint = await _context.Complaint.FindAsync(complaintID);

            if (user== null)
            {
                return View("/Views/Shared/NotFound.cshtml");
            }

            //update service params
            user.LockoutEnd = DateTimeOffset.UtcNow.AddDays((double)daysToBlock); 
            user.LockoutDays = daysToBlock;
            user.BlockMotive = blockMotive;

            //Update complaints
            if(complaintID != 0) 
            { 
                complaint.IsSolved = true;
                complaint.Resolution = "Utilizador" + user.UserName +" bloqueado por "
                    + user.LockoutDays + " dias, devido a "+user.BlockMotive+".";
                complaint.ResolutionDate = DateTime.Now;
                complaint.ResolvedBy = await _userManager.GetUserAsync(User);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    SendEmail(user, true);
                    if(complaintID != 0)
                        _context.Update(complaint);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (user == null)
                    {
                        return View("/Views/Shared/NotFound.cshtml");
                    }
                    else
                    {
                        throw;
                    }
                }

                //Cancel Non closed service requisitions of the blocked user
                var serviceRequisitions = await _context.ServiceRequisition.Where(u => (u.Service.UserID == user.Id || u.Requisitioner.Id == user.Id)
                                        && (u.ServiceRequisitionStatus == ServiceRequisitionStatus.Accepted || u.ServiceRequisitionStatus == ServiceRequisitionStatus.Pending)).ToListAsync();
                
                if(serviceRequisitions.Count() > 0)
                {
                    foreach(var serviceRequisition in serviceRequisitions)
                    {
                        serviceRequisition.ServiceRequisitionStatus = ServiceRequisitionStatus.Cancelled;
                        _context.Update(serviceRequisition);
                        _context.SaveChanges();
                    }
                }

                if(complaintID != 0)
                    return RedirectToAction("Details", "Complaints", new { id = complaint.ComplaintID });
                return RedirectToAction("Index", "UserAccess");
            }

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();


            return RedirectToAction("UserActivity", "UserAccess", new { userId = user.Id , id = complaint.ComplaintID});
        }

        /// <summary>
        /// This method Confirms the manual unblocking of a user
        /// </summary>
        /// <param name="userAccessViewModel">UserAccessViewModel object</param>
        /// <returns>Returns a Task of an IActionResult</returns>
        /// <seealso cref="Task"/>
        /// <seealso cref="IActionResult"/>
        [Authorize(Roles = "Moderador,Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unblock([Bind("Id,UserName,FirstName,LastName,Birthdate,Email,AccountCreationDate,LastSeen,LockoutDays,BlockMotive,ComplaintID")] UserAccessViewModel userAccessViewModel)
        {

            var daysToBlock = userAccessViewModel.LockoutDays;
            var complaintID = userAccessViewModel.ComplaintID;

            var user = await _userManager.FindByIdAsync(userAccessViewModel.Id);

            Complaint complaint = await _context.Complaint.FindAsync(complaintID);

            if (user == null )
            {
                return View("/Views/Shared/NotFound.cshtml");
            }


            //update service params
            user.LockoutEnd = DateTimeOffset.UtcNow; 
            user.BlockMotive = "";
            user.LockoutDays = 0;


            //Update complaints
            if (complaintID != 0)
            {
                complaint.IsSolved = true;
                complaint.Resolution = "Utilizador" + user.UserName + " desbloqueado por " + await _userManager.GetUserAsync(User);
                complaint.ResolutionDate = DateTime.Now;
                complaint.ResolvedBy = await _userManager.GetUserAsync(User);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    if (complaintID != 0)
                        _context.Update(complaint);

                    SendEmail(user,false);
                    _context.SaveChanges();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (user == null)
                    {
                        return View("/Views/Shared/NotFound.cshtml");
                    }
                    else
                    {
                        throw;
                    }
                }
                if (complaintID != 0)
                    return RedirectToAction("Details", "Complaints", new { id = complaint.ComplaintID });
                return RedirectToAction("Index", "UserAccess");
            }

            //ViewBag for bell icon on the nav menu
            var userLoggedIn = await _userManager.GetUserAsync(User);
            ViewBag.HasNotificationToRead = _context.Notification.Include(n => n.User).Where(notif => notif.DestinaryID == userLoggedIn.Id && !notif.IsRead).Count();

            return RedirectToAction("UserActivity", "UserAccess", new { userId = user.Id, id = complaint.ComplaintID });
        }


        /// <summary>
        /// This Method composes an email to either inform a user he was blocked, for how long and the motives of the block or that he was unblock
        /// </summary>
        /// <param name="IsBlock">Checks if action is block</param>
        /// <param name="user">User object</param>
        /// <returns>void</returns>
        private void SendEmail(User user, Boolean IsBlock)
        {
            var emailBody = "";
            var subject = "";
            if (IsBlock) {
                subject = "Bloqueio de conta";
                emailBody = "<p>Informamos que a sua conta foi bloqueada durante " + user.LockoutDays + " pelo seguinte motivo:</p>" +
                user.BlockMotive;
            }
            else
            {
                subject = "Desbloqueio de Conta";
                emailBody = "<p>Informamos que a sua conta foi desbloqueada</p>";
            }
            _emailSender.SendEmailAsync(user.Email, subject, emailBody);

        }

        /// <summary>
        /// This method allows to return the role id by the name given by parameter
        /// </summary>
        /// <param name="name"></param>
        /// <returns>string</returns>
        [HttpGet]
        public string GetIdByNameRole(string name)
        {
            return (from role in _context.Roles
                    where role.Name == name
                    select role.Id).FirstOrDefault();
        }

        /// <summary>
        /// This method allows to return the user id by the email given by parameter
        /// </summary>
        /// <param name="email"></param>
        /// <returns>string</returns>
        [HttpGet]
        public string GetIdByEmail(string email)
        {
            return (from user in _context.Users
                    where user.Email == email
                    select user.Id).FirstOrDefault();
        }
    }
}