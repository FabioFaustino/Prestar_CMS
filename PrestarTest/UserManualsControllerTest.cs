using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace PrestarTest
{


    public class UserManualsControllerTest : IClassFixture<ApplicationDbContextFixture>
    {
        public ApplicationDbContext Context { get; private set; }
        public RoleManager<IdentityRole> RoleManager { get; private set; }
        public UserManager<User> UserManager { get; private set; }
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
        public UserManualsController Controller {get; private set;}

        public UserManualsControllerTest(ApplicationDbContextFixture contextFixture)
        {
            Context = contextFixture.DbContext;
            RoleManager = contextFixture.RoleManager;
            UserManager = contextFixture.UserManager;

            Moderador = UserManager.FindByNameAsync("ModeradorTeste").Result;
            Cliente = UserManager.FindByNameAsync("InêsBotelho").Result;
            Prestador = UserManager.FindByNameAsync("PrestadorTeste").Result;
            Administrador = UserManager.FindByNameAsync("AdministradorTeste").Result;

            ClienteRole = RoleManager.FindByNameAsync("Cliente").Result;
            ModeradoresRole = RoleManager.FindByNameAsync("Moderador").Result;
            PrestadorRole = RoleManager.FindByNameAsync("Prestador").Result;
            AdministradorRole = RoleManager.FindByNameAsync("Administrador").Result;

            Controller = new UserManualsController(Context, UserManager);


            ModeradorLog = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()
            {
                new Claim(ClaimTypes.Name, Moderador.UserName),
                new Claim(ClaimTypes.Email, Moderador.Email),
                new Claim(ClaimTypes.NameIdentifier, Moderador.Id),
                new Claim(ClaimTypes.Role, ModeradoresRole.Name),
            }, "Test"));
            ClientLog = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()
            {
                new Claim(ClaimTypes.Name, Cliente.UserName),
                new Claim(ClaimTypes.Email, Cliente.Email),
                new Claim(ClaimTypes.NameIdentifier, Cliente.Id),
                new Claim(ClaimTypes.Role, ClienteRole.Name),
            }, "Test"));
            AdministradorLog = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()
            {
                new Claim(ClaimTypes.Name, Administrador.UserName),
                new Claim(ClaimTypes.Email, Administrador.Email),
                new Claim(ClaimTypes.NameIdentifier, Administrador.Id),
                new Claim(ClaimTypes.Role, AdministradorRole.Name),
            }, "Test"));
            PrestadorLog = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()
            {
                new Claim(ClaimTypes.Name, Prestador.UserName),
                new Claim(ClaimTypes.Email, Prestador.Email),
                new Claim(ClaimTypes.NameIdentifier, Prestador.Id),
                new Claim(ClaimTypes.Role, PrestadorRole.Name),
            }, "Test"));
        }

       

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void Details_ReturnViewResult(int? id)        {
           
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Details(id);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<UserManual>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(id, model.UserManualID);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public async void Details_ReturnNotFound(int? id)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };
            var result = await Controller.Details(id);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async void Create_ReturnViewResult()
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Create();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void Create_ReturnRedirect()
        {
            var size = Context.UserManual.Count();

            var userManual = new UserManual { UserManualID = 4, Role = "Administrador", LastUpdate = DateTime.Now, LastUpdateUserID = "3" };

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await Controller.Create(userManual);
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(size + 1, Context.UserManual.Count());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void Edit_ReturnViewResult(int? id)
        {
         
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Edit(id);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<UserManual>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(id, model.UserManualID);
            
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async void Edit_ReturnNotFound(int? id)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Edit(id);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

      

        [Theory]
        [InlineData(3)]
        public async void DeleteConfirmed_ReturnRedirect(int id)
        {
            var size = Context.UserManual.Count();

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.DeleteConfirmed(id);

            Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal(size - 1, Context.UserManual.Count());
        }

        [Theory]
        [InlineData(10)]
        [InlineData(-11)]
        public async void DeleteConfirmed_ReturnNotFound(int id)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await Controller.DeleteConfirmed(id);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

    }
}
