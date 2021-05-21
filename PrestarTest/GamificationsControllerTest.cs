using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prestar.Controllers;
using Prestar.Data;
using Prestar.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace PrestarTest
{


    public class GamificationsControllerTest : IClassFixture<ApplicationDbContextFixture>
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

        public GamificationsController Controller { get; set; }

        public GamificationsControllerTest(ApplicationDbContextFixture contextFixture)
        {

            Context = contextFixture.DbContext;
            RoleManager = contextFixture.RoleManager;
            UserManager = contextFixture.UserManager;

            Controller = new GamificationsController(Context, UserManager);

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
        public async void Index_ReturnViewResult()
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var result = await Controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Gamification>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            
        }


        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void Details_ReturnViewResult(int GamificationID)
        {

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Details(GamificationID);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Gamification>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(GamificationID, model.GamificationID);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public async void Details_ReturnNotFound(int GamificationID)
        {

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Details(GamificationID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async void CreateAsync_ReturnViewResult()
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.CreateAsync();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void Create_ReturnRedirect()
        {
            var size = Context.Gamification.Count();

            var gamification = new Gamification()
            {
                GamificationID = 4,
                PointsPerComment = 5,
                GamificationName = "Teste 4",
                PointsPerEvaluation = 5,
                PointsPerService = 5,
                IsActive = false
            };

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Create(gamification);
           
            var actionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", actionResult.ActionName);
            Assert.Equal(size + 1, Context.Gamification.Count());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void Edit_ReturnViewResult(int GamificationID)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Edit(GamificationID);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Gamification>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(GamificationID, model.GamificationID);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async void Edit_ReturnNotFound(int GamificationID)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var result = await Controller.Edit(GamificationID);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(2)]
        public async void EditPost_ReturnRedirectToAction(int gamificationID)
        {
            var gamification = Context.Gamification.Find(gamificationID);
            var oldGamificationName = gamification.GamificationName;
            gamification.GamificationName = "Edited";
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var result = await Controller.Edit(gamificationID, gamification);
            var actionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.NotEqual(oldGamificationName, Context.Gamification.Find(gamificationID).GamificationName);
            Assert.Equal("Edited", Context.Gamification.Find(gamificationID).GamificationName);
            Assert.Equal("Index", actionResult.ActionName);
        }

        [Theory]
        [InlineData(2)]
        public async void EditPost_ReturnNotFound(int gamificationID)
        {
            var gamification = Context.Gamification.Find(1);

            Assert.NotEqual(Context.Gamification.Find(gamificationID).GamificationName, gamification.GamificationName);


            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Edit(gamificationID, gamification);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void Delete_ReturnViewResult(int gamificationID)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Delete(gamificationID);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Gamification>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(gamificationID, model.GamificationID);
        }

        [Theory]
        [InlineData(-8)]
        [InlineData(0)]
        public async void Delete_ReturnNotFound(int gamificationID)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Delete(gamificationID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(3)]
        public async void DeleteConfirmed_ReturnRedirect(int id)
        {
            var size = Context.Gamification.Count();
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.DeleteConfirmed(id);
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(size - 1, Context.Gamification.Count());
        }

        [Theory]
        [InlineData(10)]
        [InlineData(-11)]
        public async void DeleteConfirmed_ReturnNotFound(int gamificationID)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var result = await Controller.DeleteConfirmed(gamificationID);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async void Configurations_ReturnsViewResult()
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };
            var result = await Controller.Configurations();
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Gamification>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.Gamification.Count(), model.Count());
        }

        [Theory]
        [InlineData(2)]
        public async void Activate_ReturnViewResult(int gamificationID)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Activate(gamificationID);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Gamification>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(gamificationID, model.GamificationID);
        }

        [Theory]
        [InlineData(-8)]
        [InlineData(0)]
        public async void Activate_ReturnNotFound(int gamificationID)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Delete(gamificationID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async void ActivateConfirmed_ReturnRedirect()
        {
            var gamification = Context.Gamification.Find(2);
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var result = await Controller.ActivateConfirmed(gamification);
            var actionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.False(Context.Gamification.Find(1).IsActive);
            Assert.True(Context.Gamification.Find(2).IsActive);           
            Assert.Equal("Configurations", actionResult.ActionName);
        }

    }
}
