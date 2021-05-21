using Xunit;
using Prestar.Controllers;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Prestar.Models;
using Prestar.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using Prestar.Services;
using System;

namespace PrestarTest
{
    public class HomeControllerTest : IClassFixture<ApplicationDbContextFixture>
    {
        public ILogger<HomeController> Logger { get; set; }
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
        public HomeController Controller { get; private set; }

        public HomeControllerTest(ApplicationDbContextFixture contextFixture)
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

            Controller = new HomeController(Logger, Context, UserManager);
        }

        [Fact]
        public async void Index_ReturnsViewResult()
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var result = await Controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ServiceCategory>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.ServiceCategory.Count(), model.Count());
        }


        [Fact]
        public async void Privacy_ReturnViewResult()
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Privacy(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PaginatedList<PrivacyPolicySection>>(viewResult.Model);
            Assert.NotNull(model);
        }

        [Fact]
        public async void CreateSection_ReturnViewResult()
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = AdministradorLog };
            var result = await Controller.CreateSection();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void CreateSection_ReturnRedirectToAction()
        {
            var size = Context.PrivacyPolicySection.Count();

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = AdministradorLog };

            var newPrivacyPolicySection = new PrivacyPolicySection()
            {
                PrivacyPolicySectionID = 3,
                Title = "Tempo de permanencia dos dados",
                Content = "Para fins de controlo de atividade na plataforma, os dados deverão ser mantidos por....",
                PrivacyPolicySectionLastUpdate = DateTime.Now,
                LastUpdateUserID = "1"
            };

            var result = await Controller.CreateSection(newPrivacyPolicySection);
            var actionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Privacy", actionResult.ActionName);
            Assert.Equal(size + 1, Context.PrivacyPolicySection.Count());
        }

        [Fact]
        public async void EditSection_ReturnViewResult()
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = AdministradorLog };
            
            var result = await Controller.EditSection(1);
            
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotEqual("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
            var model = Assert.IsAssignableFrom<PrivacyPolicySection>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(1, model.PrivacyPolicySectionID);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        [InlineData(null)]
        public async void EditSection_ReturnNotFound(int? id)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = AdministradorLog };
            var result = await Controller.EditSection(id);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(1)]
        public async void EditSection_ReturnRedirectToAction(int id)
        {
            var PrivacyPolicySectionEdited = Context.PrivacyPolicySection.Find(id);
            PrivacyPolicySectionEdited.Title = "Titulo Editado";

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = AdministradorLog };
            var result = await Controller.EditSection(id, PrivacyPolicySectionEdited);

            var actionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Privacy", actionResult.ActionName);
            Assert.Equal("Titulo Editado", Context.PrivacyPolicySection.Find(id).Title);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async void EditSection_Fails_ReturnNotFound(int id)
        {
            var PrivacyPolicySectionEdited = new PrivacyPolicySection
            {
                PrivacyPolicySectionID = 1,
                Title = "Criar um serviço Editado",
                Content = "Para criar um serviço voce deve primeiro pedir para ser prestador...",
                PrivacyPolicySectionLastUpdate = DateTime.Now,
                LastUpdateUserID = "1"
            };
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = AdministradorLog };

            var result = await Controller.EditSection(id, PrivacyPolicySectionEdited);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(2)]
        public async void DeleteConfirmed_ReturnRedirect(int id)
        {
            var size = Context.PrivacyPolicySection.Count();

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = AdministradorLog };
            var result = await Controller.DeleteConfirmed(id);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(size - 1, Context.PrivacyPolicySection.Count());
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

