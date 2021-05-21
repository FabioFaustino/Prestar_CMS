using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Prestar.Controllers;
using Prestar.Data;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Prestar.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace PrestarTest
{
   
    public class ServiceCategoriesControllerTest : IClassFixture<ApplicationDbContextFixture>
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
        public ServiceCategoriesController Controller { get; private set; }

        public ServiceCategoriesControllerTest(ApplicationDbContextFixture contextFixture)
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

            Controller = new ServiceCategoriesController(Context, UserManager);

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

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async void Details_ReturnsViewResult(int CategoryID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var result = await Controller.Details(CategoryID);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ServiceCategory>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(CategoryID, model.CategoryID);
        }

        [Theory]
        [InlineData(-10)]
        [InlineData(20)]
        public async void Details_ReturnsNotFound(int CategoryID)
        {
            var result = await Controller.Details(CategoryID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async  void Create_ReturnsViewResult()
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.CreateAsync();
            Assert.IsType<ViewResult>(result);
        }

        [Theory]
        [InlineData(4)]
        public async void CreateSubcategory_ReturnsViewResult(int CategoryID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var result = await Controller.CreateSubcategoryAsync(CategoryID);
            Assert.IsType<ViewResult>(result);

        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async void CreateSubcategory_ReturnsNotFoundIsSubcategory(int CategoryID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var result = await Controller.CreateSubcategoryAsync(CategoryID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(-10)]
        [InlineData(20)]
        public async void CreateSubcategory_ReturnsNotFound(int CategoryID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var result = await Controller.CreateSubcategoryAsync(CategoryID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async void EditCategory_ReturnsNotFoundIsSubcategory(int CategoryID)
        {
            var result = await Controller.Edit(CategoryID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(-5)]
        public async void EditCategory_ReturnsNotFound(int CategoryID)
        {
            var result = await Controller.Edit(CategoryID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(4)]
        public void EditCategory_ReturnsViewResult(int CategoryID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var result = Controller.Edit(CategoryID);
            var viewResult = Assert.IsType<ViewResult>(result.Result);
            var model = Assert.IsAssignableFrom<ServiceCategory>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(CategoryID, model.CategoryID);
        }

        [Theory]
        [InlineData(4)]
        public async void EditSubcategory_ReturnsNotFoundIsCategory(int CategoryID)
        {
            var result = await Controller.EditSubcategory(CategoryID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(6)]
        [InlineData(0)]
        public async void EditSubcategory_ReturnsNotFound(int CategoryID)
        {
            var result = await Controller.EditSubcategory(CategoryID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void EditSubcategory_ReturnsViewResult(int CategoryID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var result = Controller.EditSubcategory(CategoryID);
            var viewResult = Assert.IsType<ViewResult>(result.Result);
            var model = Assert.IsAssignableFrom<ServiceCategory>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(CategoryID, model.CategoryID);
        }

        [Theory]
        [InlineData(4)]
        public async void Delete_ReturnsNotFoundHaveSubcategories(int CategoryID)
        {
            
            var result = await Controller.Delete(CategoryID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(-3)]
        [InlineData(0)]
        public async void Delete_ReturnsNotFound(int CategoryID)
        {
            
            var result = await Controller.Delete(CategoryID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }
        
        [Theory]
        [InlineData(3)]// Only category not bound to a service or a subcategory
        public async void Delete_ReturnsViewResult(int CategoryID)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Delete(CategoryID);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ServiceCategory>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(CategoryID, model.CategoryID);
        }

        [Theory]
        [InlineData(4)]
        public async void DeleteConfirmed_ReturnsNotFoundHaveSubcategories(int CategoryID)
        {

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.DeleteConfirmed(CategoryID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }


        [Theory]
        [InlineData(-3)]
        [InlineData(0)]
        public async void DeleteConfirmed_ReturnsNotFound(int CategoryID)
        {
            

            var result = await Controller.DeleteConfirmed(CategoryID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(5)]
        public void DeleteConfirmed_ReturnsRedirectToActionResult(int CategoryID)
        {
            

            var result = Controller.DeleteConfirmed(CategoryID);
            Assert.IsType<RedirectToActionResult>(result.Result);
        }

        
        [Fact]
        public async void CreatePost_ReturnsRedirectToAction()
        {
            var serviceCategory = new ServiceCategory { CategoryID = 7, Name = "Eletricidade" };

            
            Controller.ModelState.Clear();
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var atualSize = Context.ServiceCategory.Count();

            var result = await Controller.Create(serviceCategory);
            Assert.IsType<RedirectToActionResult>(result);

            //Confirmar o Indice
            var resultI = Controller.Index();
            var viewResultI = Assert.IsType<ViewResult>(resultI.Result);
            var modelI = Assert.IsAssignableFrom<IEnumerable<ServiceCategory>>(viewResultI.ViewData.Model);
            Assert.NotNull(modelI);
            Assert.Equal(atualSize + 1, modelI.Count());
        }

        [Fact]
        public void CreatePost_ReturnsViewResult_WhenError()
        {
            var serviceCategory = new ServiceCategory { CategoryID = 10, Name = null };

            
            Controller.ModelState.AddModelError("Error", "Nome não está correto");

            var atualSize = Context.ServiceCategory.Count();

            var result = Controller.Create(serviceCategory);
            Assert.IsType<ViewResult>(result.Result);

            Assert.Equal(atualSize, Context.ServiceCategory.Count());
        }
        
        
        [Fact]
        public void CreateSubcategoryPost_ReturnsRedirectToAction()
        {
            var serviceCategory = new ServiceCategory { CategoryID = 8, Name = "Eletricidade" };

            
            Controller.ModelState.Clear();
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var atualSize = Context.ServiceCategory.Count();

            var result = Controller.CreateSubcategory(4, serviceCategory);
            Assert.IsType<RedirectToActionResult>(result.Result);

            //Confirmar o Indice
            var resultI = Controller.Index();
            var viewResultI = Assert.IsType<ViewResult>(resultI.Result);
            var modelI = Assert.IsAssignableFrom<IEnumerable<ServiceCategory>>(viewResultI.ViewData.Model);
            Assert.NotNull(modelI);
            Assert.Equal(atualSize + 1, modelI.Count());
        }
        
        [Fact]
        public void CreateSubcategoryPost_ReturnsViewResult_WhenError()
        {
            var serviceCategory = new ServiceCategory { CategoryID = 10, Name = null };

            
            Controller.ModelState.AddModelError("Error", "Nome não está correto");

            var atualSize = Context.ServiceCategory.Count();

            var result = Controller.CreateSubcategory(4, serviceCategory);
            Assert.IsType<ViewResult>(result.Result);

            Assert.Equal(atualSize, Context.ServiceCategory.Count());
        }

        [Fact]
        public async void CreateSubcategoryPost_ReturnsNotFound_WhenIdSubcategoty()
        {
            var serviceCategory = new ServiceCategory { CategoryID = 10, Name = "Teste" };
             Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            
            Controller.ModelState.AddModelError("Error", "Nome não está correto");
            

            var result = await Controller.CreateSubcategory(1, serviceCategory);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

    }
}
