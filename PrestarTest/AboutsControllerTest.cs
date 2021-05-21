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

    public class AboutsControllerTest : IClassFixture<ApplicationDbContextFixture>
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
        public AboutsController Controller { get; private set; }


        public AboutsControllerTest(ApplicationDbContextFixture contextFixture)
        {
            Context = contextFixture.DbContext;
            RoleManager = contextFixture.RoleManager;
            UserManager = contextFixture.UserManager;

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

            Controller = new AboutsController(Context, UserManager);
          
        }

       
        
        [Fact]
        public async void Create_ReturnRedirectToAction()
        {
            var size = Context.About.Count();

            var About = new About { 
                AboutID = 4,
                Title = "Camara",
                Content = "Camara de setubal"
            };

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Create(About);
            var actionResult =   Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index",actionResult.ActionName);

            Assert.Equal(size + 1, Context.About.Count());
        }



        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void Edit_ReturnViewResult(int? id)
        {         
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Edit(id);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<About>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(id, model.AboutID);
            
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
        [InlineData(1)]
        public async void Edit_ReturnRedirectToAction(int id)
        {
            var AboutEdited = Context.About.Find(id);
            AboutEdited.Title ="Titulo Editado";

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Edit(id, AboutEdited);

            var actionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", actionResult.ActionName);
            Assert.Equal("Titulo Editado", Context.About.Find(id).Title);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async void EditFails_ReturnNotFound(int id)
        {

            var AboutEdited = new About
            {
                AboutID = 1,
                Title = "Exemplo",
                Content = "Não vai encontrar",
              
            };
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            
            var result = await Controller.Edit(id, AboutEdited);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }



        [Theory]
        [InlineData(3)] 
        public async void DeleteConfirmed_ReturnRedirect(int id)
        {
            var size = Context.About.Count();

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.DeleteConfirmed(id);

            Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal(size - 1, Context.About.Count());
        }

        [Theory]
        [InlineData(10)]
        [InlineData(-11)]
        public async void DeleteConfirmed_ReturnNotFound(int id)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.DeleteConfirmed(id);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

    }
}
