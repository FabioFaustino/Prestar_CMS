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


    public class SectionsControllerTest : IClassFixture<ApplicationDbContextFixture>
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
        public SectionsController Controller {get; private set;}
        public UserManualsController UserManualController { get; private set; }

        public SectionsControllerTest(ApplicationDbContextFixture contextFixture)
        {
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

            Controller = new SectionsController(Context, UserManager);
            UserManualController = new UserManualsController(Context, UserManager);

        }

       

        [Theory]
        [InlineData(3)] // user manual id
        public async void CreateAsync_ReturnViewResult(int userManualID)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.CreateAsync(userManualID);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void Create_ReturnRedirect()
        {
            var size = Context.Section.Count();

            var section = new Section() { SectionID = 6, Title = "Denuncias - Cliente", Content = "Para consultar as denuncias de Clientes deve ir a secção de denuncias e...", UserManualID = 3 };

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Create(section);
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(size + 1, Context.Section.Count());
        }


        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void Edit_ReturnViewResult(int? id)
        {
         
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Edit(id);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Section>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(id, model.SectionID);
            
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
            var sectionEdited = Context.Section.Find(id);
            sectionEdited.Title += " Editado";
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var result = await Controller.Edit(id, sectionEdited);
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Criar um serviço Editado", Context.Section.Find(id).Title);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async void EditFails_ReturnNotFound(int id)
        {

            var sectionEdited = new Section { SectionID = 1, Title = "Criar um serviço Editado", Content = "Para criar um serviço voce deve primeiro pedir para ser prestador ...", UserManualID = 10 };

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Edit(id, sectionEdited);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }



        [Theory]
        [InlineData(3)]
        public async void DeleteConfirmed_ReturnRedirect(int id)
        {
            var size = Context.Section.Count();

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.DeleteConfirmed(id);

            Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal(size - 1, Context.Section.Count());
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
