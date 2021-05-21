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


    public class NewsControllerTest : IClassFixture<ApplicationDbContextFixture>
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

        public NewsController Controller { get; set; }

        public NewsControllerTest(ApplicationDbContextFixture contextFixture)
        {

            Context = contextFixture.DbContext;
            RoleManager = contextFixture.RoleManager;
            UserManager = contextFixture.UserManager;

            Controller = new NewsController(Context, UserManager);

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

        //Verify if only the visible news display
        [Fact]
        public async void Index_ReturnViewResult()
        {
            var visibleNews = Context.New.Where(n => n.Visible == true).Count();
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            var result = await Controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<New>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(visibleNews, model.Count());
        }


        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void Details_ReturnViewResult(int NewID)
        {

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await Controller.Details(NewID);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<New>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(NewID, model.NewsID);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public async void Details_ReturnNotFound(int NewID)
        {

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };
            var result = await Controller.Details(NewID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async void CreateAsync_ReturnViewResult()
        {

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await Controller.CreateAsync();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void Create_ReturnRedirect()
        {
            var size = Context.New.Count();

            var createNews = new New()
            {
                NewsID = 4,
                Title = "Noticia 4",
                SecondTitle = "Titulo secundário 4",
                Description = "Descrição de noticia de teste 4"
            };

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await Controller.Create(createNews);
           
            var actionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", actionResult.ActionName);
            Assert.Equal(size + 1, Context.New.Count());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void Edit_ReturnViewResult(int NewID)
        {

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await Controller.Edit(NewID);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<New>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(NewID, model.NewsID);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async void Edit_ReturnNotFound(int NewID)
        {

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await Controller.Edit(NewID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }


        [Theory]
        [InlineData(2)]
        
        public async void EditPost_ReturnNotFound(int NewID)
        {
            var newToUpdate = Context.New.Find(1);

            Assert.NotEqual(Context.New.Find(NewID).Description, newToUpdate.Description);


            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await Controller.Edit(NewID, newToUpdate);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

       

        [Theory]
        [InlineData(3)]
        public async void DeleteConfirmed_ReturnRedirect(int id)
        {
            var size = Context.New.Count();


            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await Controller.DeleteConfirmed(id);
            Assert.IsType<RedirectToActionResult>(result);

           
            Assert.Equal(size - 1, Context.New.Count());
        }

        [Theory]
        [InlineData(10)]
        [InlineData(-11)]
        public async void DeleteConfirmed_ReturnNotFound(int NewID)
        {

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await Controller.DeleteConfirmed(NewID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

    }
}
