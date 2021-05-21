using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prestar.Controllers;
using Prestar.Data;
using Prestar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace PrestarTest
{


    public class UserAccessControllerTest : IClassFixture<ApplicationDbContextFixture>
    {
        public ApplicationDbContext Context { get; private set; }
        public RoleManager<IdentityRole> RoleManager { get; private set; }
        public UserManager<User> UserManager { get; private set; }
        public IEmailSender EmailSender { get; private set; }
        public User Moderador { get; private set; }
        public User Cliente { get; private set; }
        public User Prestador { get; private set; }
        public User Administrador { get; private set; }
        public IdentityRole ClienteRole { get; private set; }
        public IdentityRole ModeradoresRole { get; private set; }
        public IdentityRole PrestadorRole { get; private set; }
        public IdentityRole AdministradorRole { get; private set; }
        public ClaimsPrincipal ModeradorLog { get; private set; }
        public ClaimsPrincipal ClientLog { get; private set; }
        public ClaimsPrincipal PrestadorLog { get; private set; }
        public ClaimsPrincipal AdministradorLog { get; private set; }


        public UserAccessControllerTest(ApplicationDbContextFixture contextFixture)
        {
            
            EmailSender = contextFixture.EmailSender;

            Context = contextFixture.DbContext;
            RoleManager = contextFixture.RoleManager;
            UserManager = contextFixture.UserManager;

            Moderador = contextFixture.Moderador;
            Cliente = contextFixture.Cliente;
            Prestador = contextFixture.Prestador;
            Administrador = contextFixture.Administrador;

            ModeradorLog = contextFixture.ModeradorLog;
            ClientLog = contextFixture.ClientLog;
            PrestadorLog = contextFixture.PrestadorLog;
            AdministradorLog = contextFixture.AdministradorLog;

            ClienteRole = contextFixture.ClienteRole;
            ModeradoresRole = contextFixture.ModeradoresRole;
            PrestadorRole = contextFixture.PrestadorRole;
            AdministradorRole = contextFixture.AdministradorRole;


        }


        [Fact]
        public async void Index_ReturnsViewResult()
        {
            var controller = new UserAccessController(UserManager, RoleManager,Context,EmailSender);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await controller.Index(1);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<UserAccessViewModel>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.Users.Count(), model.Count());
        }

        
  
      

        [Theory]
        [InlineData("-10")]
        [InlineData("10")]
        public async void DeleteConfirmed_ReturnsNotFound(string UserID)
        {
            var controller = new UserAccessController(UserManager, RoleManager,Context, EmailSender);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var result = await controller.DeleteConfirmed(UserID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);

            //Confirmar o Indice
            var resultI = await controller.Index(1);
            var viewResultI = Assert.IsType<ViewResult>(resultI);
            var modelI = Assert.IsAssignableFrom<IEnumerable<UserAccessViewModel>>(viewResultI.ViewData.Model);
            Assert.NotNull(modelI);
            Assert.Equal(Context.Users.Count(), modelI.Count());
        }


         [Fact]
        public async void GetUserRoles_ReturnsListWithModeratorRole()
        {
            var controller = new UserAccessController(UserManager, RoleManager,Context, EmailSender);
            var result =   await controller.GetUserRoles(Moderador);
            Assert.NotNull(result);
            Assert.Contains("Moderador", result);
        }

        [Fact]
        public async void GetUserRoles_ReturnsEmptyList()
        {
            //Arrange
            var utilizador = new User {
                Id = "5", 
                FirstName = "Inês",
                LastName = "Botelho",
                Birthdate = new DateTime(2002, 10, 31),
                AccountCreationDate = DateTime.Now,
                LastSeen = DateTime.Now,
                Email = "ines.botelho@gmail.com", 
                EmailConfirmed = true, 
                UserName = "InêsBotelho" 
            };

            var controller = new UserAccessController(UserManager, RoleManager, Context, EmailSender);

            //Act
            var result = await controller.GetUserRoles(utilizador);

            //Assert
            Assert.NotNull(result);
            Assert.True(result.Count == 0);
        }
       

        [Theory]
        [InlineData("1")]
        public async void Delete_ReturnsViewResult(string UserID)
        {
            //Arrange
            var controller = new UserAccessController(UserManager, RoleManager, Context, EmailSender);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            //Act
            var result = await controller.Delete(UserID);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<User>(viewResult.ViewData.Model);
            //Assert
            Assert.Equal(Context.Users.Find(UserID).Id, model.Id);
            Assert.NotNull(model);
        }

        
        [Theory]
        [InlineData("1")]
        public async void Manage_Get_ReturnsViewResult(string UserID)
        {
            //Arrange
            var controller = new UserAccessController(UserManager, RoleManager, Context, EmailSender);
            var user = await UserManager.FindByIdAsync(UserID);
            _ = await UserManager.GetRolesAsync(user);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            //Act
            var result = await controller.Manage(UserID);

            
            Assert.IsType<ViewResult>(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<ManageUserRolesViewModel>>(viewResult.ViewData.Model);

            //Assert
            Assert.NotNull(model);
        }
        
        [Theory]
        [InlineData("-1")]
        [InlineData("5")]
        public async void Manage_Get_ReturnsViewResultNotFound(string UserID)
        {
            //Arrange
            var controller = new UserAccessController(UserManager, RoleManager, Context, EmailSender);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            //Act
            var result = await controller.Manage(UserID);
            //Assert

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);

            //Confirmar o Indice
            var resultI = await controller.Index(1);
            _ = Assert.IsType<ViewResult>(resultI);

        }

        //Incompleto
        [Theory]
        [InlineData("3")]
        public async void Manage_Post_ReturnsRedirectToActionIndex(string UserID)
        {
            //Arrange 
            var userRole1 = new Prestar.Models.ManageUserRolesViewModel
            {
                RoleId = (await RoleManager.FindByNameAsync("Moderador")).Id,
                RoleName = "Moderador"
            };
            var userRole2 = new Prestar.Models.ManageUserRolesViewModel
            {
                RoleId = (await RoleManager.FindByNameAsync("Prestador")).Id,
                RoleName = "Prestador"
            };

            var rolesList = new List<ManageUserRolesViewModel>() { userRole1, userRole2 };
            _ = await UserManager.FindByIdAsync(UserID);

            var controller = new UserAccessController(UserManager, RoleManager,Context, EmailSender);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            // Act
            var result = await controller.Manage(rolesList, UserID);
  
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", viewResult.ActionName);
            _ = await controller.Index(1);
   
            
        }

        [Theory]
        [InlineData("4")]
        public async void DeleteConfirmed_ReturnsRedirectToActionIndex(string UserID)
        {

            var controller = new UserAccessController(UserManager, RoleManager, Context, EmailSender);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var deleteUser = Context.Users.Find(UserID);

            var result = await controller.DeleteConfirmed(UserID);
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", viewResult.ActionName);

            //Confirmar o Indice
            var resultI = await controller.Index(1);
            var viewResultI = Assert.IsType<ViewResult>(resultI);
            var modelI = Assert.IsAssignableFrom<IEnumerable<UserAccessViewModel>>(viewResultI.ViewData.Model);
            Assert.NotNull(modelI);
            Assert.Equal(Context.Users.Count(), modelI.Count());

            //Add Removed Request
            Context.Users.Add(deleteUser);
        }




    }
}
