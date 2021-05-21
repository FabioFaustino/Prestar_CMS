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
    

    public class ServicesControllerTest : IClassFixture<ApplicationDbContextFixture>
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

        public ServicesControllerTest(ApplicationDbContextFixture contextFixture)
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
        }

        [Theory]
        [InlineData("Ascendente")]
        public async void Index_ReturnViewResult(string order)
        {
            var controller = new ServicesController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };
            var result = await controller.Index(order,1);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Service>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.Service.Count(), model.Count());
        }

        [Theory]
        [InlineData(1, "Ascendente")]
        [InlineData(2, "Descendente")]
        public async void IndexByCategory_ReturnViewResult(int categoryID, string order)
        {
            var controller = new ServicesController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog};

            var result = await controller.IndexByCategory(categoryID, order,1);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Service>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.Service.Where(cat => cat.ServiceCategoryID == categoryID).Count(), model.Count());
        }

        [Theory]
        [InlineData(20, "Ascendente")]
        [InlineData(-10, "Ascendente")]
        public async void IndexByCategory_ReturnNotFound(int categoryID, string order)
        {
            var controller = new ServicesController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            var result = await controller.IndexByCategory(categoryID,order,1);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async void UserServices_ReturnViewResult()
        {
            var controller = new ServicesController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await controller.UserServices(1);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Service>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.Service.Where(cat => cat.UserID == Prestador.Id).Count(), model.Count());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void Details_ReturnViewResult(int serviceID)
        {
            var controller = new ServicesController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await controller.Details(serviceID);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Service>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(serviceID, model.ServiceID);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public async void Details_ReturnNotFound(int serviceID)
        {
            var controller = new ServicesController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };
            var result = await controller.Details(serviceID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async void Create_ReturnViewResult()
        {
            var controller = new ServicesController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await controller.Create(false);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void Create_ReturnRedirect()
        {
            var size = Context.Service.Count();

            var service = new Service() { ServiceID = 3, UserID = "2", ServiceCategoryID = 1, ServiceName = "Limpeza de Chaminés", Description = "Limpeza de Chaminés de Manhã" };
            var controller = new ServicesController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await controller.Create(service);
            Assert.IsType<RedirectToActionResult>(result);

            //Confirmar UserServices
            var resultIndex = await controller.UserServices(1);
            var viewResultIndex = Assert.IsType<ViewResult>(resultIndex);
            var modelIndex = Assert.IsAssignableFrom<IEnumerable<Service>>(viewResultIndex.ViewData.Model);
            Assert.NotNull(modelIndex);
            Assert.Equal(size+1, modelIndex.Count());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void Edit_ReturnViewResult(int serviceID)
        {
            var controller = new ServicesController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await controller.Edit(serviceID);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Service>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(serviceID, model.ServiceID);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async void Edit_ReturnNotFound(int serviceID)
        {
            var controller = new ServicesController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await controller.Edit(serviceID);
            Assert.IsType<NotFoundResult>(result);
        }


        [Theory]
        [InlineData(3)]
        [InlineData(-1)]
        public async void EditPost_ReturnNotFound(int serviceID)
        {
            var serviceUpdate = new Service { ServiceID = 4, UserID = "2", ServiceCategoryID = 1, ServiceName = "Limpeza de portas", Description = "Limpeza de Portas de Tarde" };
            Assert.Equal("Limpeza de Portas", Context.Service.Find(4).ServiceName);
            Assert.NotEqual(Context.Service.Find(4).Description, serviceUpdate.ServiceName);

            var controller = new ServicesController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await controller.Edit(serviceID, serviceUpdate);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(4)]
        public async void Delete_ReturnViewResult(int serviceID)
        {
            var controller = new ServicesController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await controller.Delete(serviceID);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Service>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(serviceID, model.ServiceID);
        }

        [Theory]
        [InlineData(-8)]
        [InlineData(0)]
        public async void Delete_ReturnNotFound(int serviceID)
        {
            var controller = new ServicesController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await controller.Delete(serviceID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async void DeleteConfirmed_ReturnRedirect()
        {
            var size = Context.Service.Count();

            var controller = new ServicesController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await controller.DeleteConfirmed(5);
            Assert.IsType<RedirectToActionResult>(result);

            //Confirmar Index
            var resultIndex = await controller.UserServices(1);
            var viewResultIndex = Assert.IsType<ViewResult>(resultIndex);
            var modelIndex = Assert.IsAssignableFrom<IEnumerable<Service>>(viewResultIndex.ViewData.Model);
            Assert.NotNull(modelIndex);
            Assert.Equal(size - 1, modelIndex.Count());
        }

        [Theory]
        [InlineData(10)]
        [InlineData(-11)]
        public async void DeleteConfirmed_ReturnNotFound(int serviceID)
        {
            var controller = new ServicesController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await controller.DeleteConfirmed(serviceID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

    }
}
