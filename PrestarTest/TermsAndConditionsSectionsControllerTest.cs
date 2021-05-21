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
using Prestar.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace PrestarTest
{

    public class TermsAndConditionsSectionsControllerTest : IClassFixture<ApplicationDbContextFixture>
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
        public TermsAndConditionsSectionsController Controller { get; private set; }


        public TermsAndConditionsSectionsControllerTest(ApplicationDbContextFixture contextFixture)
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

            Controller = new TermsAndConditionsSectionsController(Context, UserManager);
          
        }

        [Fact]
        public async void Index_ReturnViewResult()
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Index(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PaginatedList<TermsAndConditionsSection>>(viewResult.Model);
            Assert.NotNull(model);
        }

        [Fact]
        public async void Create_ReturnRedirectToAction()
        {
            var size = Context.TermsAndConditionsSection.Count();
            var termsAndConditionsSection = new TermsAndConditionsSection { 
                TermsAndConditionsSectionID = 4,
                Title = "Tempo de permanencia dos dados",
                Content = "Para fins de controlo de atividade na plataforma, os dados deverão ser mantidos por...."

            };

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = AdministradorLog };
            var result = await Controller.Create(termsAndConditionsSection);

            var actionResult =   Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index",actionResult.ActionName);
            Assert.Equal(size + 1, Context.TermsAndConditionsSection.Count());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void Edit_ReturnViewResult(int? id)
        {         
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = AdministradorLog };
            
            var result = await Controller.Edit(id);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<TermsAndConditionsSection>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(id, model.TermsAndConditionsSectionID);
            
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async void Edit_ReturnNotFound(int? id)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = AdministradorLog };
            var result = await Controller.Edit(id);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(1)]
        public async void Edit_ReturnRedirectToAction(int id)
        {
            var TermsAndConditionsSectionEdited = Context.TermsAndConditionsSection.Find(id);
            TermsAndConditionsSectionEdited.Title ="Titulo Editado";

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = AdministradorLog };
            var result = await Controller.Edit(id, TermsAndConditionsSectionEdited);

            var actionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", actionResult.ActionName);
            Assert.Equal("Titulo Editado", Context.TermsAndConditionsSection.Find(id).Title);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async void EditFails_ReturnNotFound(int id)
        {

            var TermsAndConditionsSectionEdited = new TermsAndConditionsSection
            {
                TermsAndConditionsSectionID = 1,
                Title = "Exemplo",
                Content = "Nao vai encontrar ...",
                LastUpdate = DateTime.Now,
                LastUpdateUserID = "1"
            };
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = AdministradorLog };
            
            var result = await Controller.Edit(id, TermsAndConditionsSectionEdited);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }



        [Theory]
        [InlineData(3)] 
        public async void DeleteConfirmed_ReturnRedirect(int id)
        {
            var size = Context.TermsAndConditionsSection.Count();

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = AdministradorLog };
            var result = await Controller.DeleteConfirmed(id);

            Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal(size - 1, Context.TermsAndConditionsSection.Count());
        }

        [Theory]
        [InlineData(10)]
        [InlineData(-11)]
        public async void DeleteConfirmed_ReturnNotFound(int id)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = AdministradorLog };
            var result = await Controller.DeleteConfirmed(id);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

    }
}
