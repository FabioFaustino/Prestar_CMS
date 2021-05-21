using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Prestar.Data;
using Prestar.Models;
using System;
using Moq;
using Microsoft.AspNetCore.Identity;
using Prestar.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace PrestarTest
{
    
    public class RequestsControllerTest : IClassFixture<ApplicationDbContextFixture>
    {
        public ApplicationDbContext Context { get; private set; }
        public RoleManager<IdentityRole> RoleManager { get; private set; }
        public UserManager<User> UserManager { get; private set; }
        public User Moderador { get; private set; }
        public User Cliente { get; private set; }
        public IdentityRole ClienteRole { get; private set; }
        public IdentityRole ModeradoresRole { get; private set; }
        public IdentityRole ProvidersRole { get; private set; }
        public ClaimsPrincipal ModeradorLog { get; private set; }
        public ClaimsPrincipal ClientLog { get; private set; }
        public RequestsController Controller { get; private set; }


        public RequestsControllerTest(ApplicationDbContextFixture contextFixture)
        {
            Context = contextFixture.DbContext;
            RoleManager = contextFixture.RoleManager;
            UserManager = contextFixture.UserManager;

            Controller = new RequestsController(Context, UserManager, RoleManager);

            Moderador = UserManager.FindByNameAsync("ModeradorTeste").Result;
            Cliente = UserManager.FindByNameAsync("InêsBotelho").Result;

            ClienteRole = RoleManager.FindByNameAsync("Cliente").Result;
            ModeradoresRole = RoleManager.FindByNameAsync("Moderador").Result;
            ProvidersRole = RoleManager.FindByNameAsync("Prestador").Result;

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
        }

        [Fact]
        public async void Index_ReturnsViewResult()
        {
           
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Index(1);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Request>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.Request.Count(), model.Count());
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public async void Delete_ReturnsViewResult(int RequestID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Delete(RequestID);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Request>(viewResult.ViewData.Model);
            Assert.Equal(Context.Request.Find(RequestID).RequestID, model.RequestID);
            Assert.NotNull(model);
        }

        [Theory]
        [InlineData(4)]
        [InlineData(-1)]
        public async void Delete_ReturnsNotFound(int RequestID)
        {
            
            var result = await Controller.Delete(RequestID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(1)]
        public async void DeleteConfirmed_ReturnsRedirectToActionIndex(int RequestID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var deleteRequest = Context.Request.Find(RequestID);
            var result = await Controller.DeleteConfirmed(RequestID);
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", viewResult.ActionName);

            //Confirmar o Indice
            var resultI = await Controller.Index(1);
            var viewResultI = Assert.IsType<ViewResult>(resultI);
            var modelI = Assert.IsAssignableFrom<IEnumerable<Request>>(viewResultI.ViewData.Model);
            Assert.NotNull(modelI);
            Assert.Equal(Context.Request.Count(), modelI.Count());

            //Add Removed Request
            Context.Request.Add(deleteRequest);
        }

        [Theory]
        [InlineData(-10)]
        [InlineData(10)]
        public async void DeleteConfirmed_ReturnsNotFound(int RequestID)
        {
            
            var result = await Controller.DeleteConfirmed(RequestID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            //Confirmar o Indice
            var resultI = await Controller.Index(1);
            var viewResultI = Assert.IsType<ViewResult>(resultI);
            var modelI = Assert.IsAssignableFrom<IEnumerable<Request>>(viewResultI.ViewData.Model);
            Assert.NotNull(modelI);
            Assert.Equal(Context.Request.Count(), modelI.Count());
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public void RequestExists_ReturnsTrue(int RequestID)
        {
            
            var result = Controller.RequestExists(RequestID);
            Assert.True(result);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(20)]
        public void RequestExists_ReturnsFalse(int RequestID)
        {
            
            var result = Controller.RequestExists(RequestID);
            Assert.False(result);
        }

        [Theory]
        [InlineData(2, "4")]
        [InlineData(3, "4")]
        public async void AproveProvideServices_ReturnsRedirectToActionIndex(int RequestID, string UserID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var resultC = await Controller.AproveProvideServices(UserID, RequestID);
            _ = Assert.IsType<RedirectToActionResult>(resultC);
            Assert.True(UserManager.IsInRoleAsync(Cliente, ProvidersRole.Name).Result);
            Assert.False(UserManager.IsInRoleAsync(Cliente, ClienteRole.Name).Result);
            Assert.Equal(RequestStatus.Aproved, Context.Request.Find(RequestID).RequestStatus);

            //Confirmar o Indice
            var resultI = await Controller.Index(1);
            var viewResultI = Assert.IsType<ViewResult>(resultI);
            var modelI = Assert.IsAssignableFrom<IEnumerable<Request>>(viewResultI.ViewData.Model);
            Assert.NotNull(modelI);
            Assert.Equal(Context.Request.Count(), modelI.Count());

            //Back To Be Just Cliente
            await UserManager.RemoveFromRoleAsync(Cliente, ProvidersRole.Name);
            await UserManager.AddToRoleAsync(Cliente, ClienteRole.Name);
        }

        [Theory]
        [InlineData(2, "3")]
        [InlineData(3, "3")]
        public async void AproveProvideServices_ReturnsNotFoundUser(int RequestID, string UserID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var resultC = await Controller.AproveProvideServices(UserID, RequestID);
            var viewResult = Assert.IsType<ViewResult>(resultC);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(4, "1")]
        [InlineData(-3, "1")]
        [InlineData(0, "1")]
        public async void AproveProvideServices_ReturnsNotFoundRequest(int RequestID, string UserID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var resultC = await Controller.AproveProvideServices(UserID, RequestID);
            var viewResult = Assert.IsType<ViewResult>(resultC);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(2, "1")]
        [InlineData(3, "1")]
        public async void AproveProvideServices_ReturnsNotFoundAuthorized(int RequestID, string UserID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            var resultC = await Controller.AproveProvideServices(UserID, RequestID);
            var viewResult = Assert.IsType<ViewResult>(resultC);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(2, "2")]
        [InlineData(3, "2")]
        public async void AproveProvideServices_ReturnsNotFoundUserWrong(int RequestID, string UserID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var resultC = await Controller.AproveProvideServices(UserID, RequestID);
            var viewResult = Assert.IsType<ViewResult>(resultC);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public async void ReproveProvideServices_ReturnsViewResult(int RequestID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var resultC = await Controller.ReproveProvideServicesAsync(RequestID);
            var viewResult = Assert.IsType<ViewResult>(resultC);
            var model = Assert.IsAssignableFrom<Request>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(RequestID, model.RequestID);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(4)]
        public async void ReproveProvideServices_ReturnsNotFound(int RequestID)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var resultC = await Controller.ReproveProvideServicesAsync(RequestID);
            var viewResult = Assert.IsType<ViewResult>(resultC);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public async void ReproveProvideServices_ReturnsNotFoundNotAuthorized(int RequestID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            var resultC = await Controller.ReproveProvideServicesAsync(RequestID);
            var viewResult = Assert.IsType<ViewResult>(resultC);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public async void ReproveProvideServicesPost_ReturnsRedirectToActionIndex(int RequestID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var resultC = await Controller.ReproveProvideServices(RequestID, Context.Request.Find(RequestID));
            _ = Assert.IsType<RedirectToActionResult>(resultC);
            Assert.True(UserManager.IsInRoleAsync(Cliente, ClienteRole.Name).Result);
            Assert.False(UserManager.IsInRoleAsync(Cliente, ModeradoresRole.Name).Result);
            Assert.Equal(RequestStatus.Rejected, Context.Request.Find(RequestID).RequestStatus);

            //Confirmar o Indice
            var resultI = await Controller.Index(1);
            var viewResultI = Assert.IsType<ViewResult>(resultI);
            var modelI = Assert.IsAssignableFrom<IEnumerable<Request>>(viewResultI.ViewData.Model);
            Assert.NotNull(modelI);
            Assert.Equal(Context.Request.Count(), modelI.Count());
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(5)]
        public async void ReproveProvideServicesPost_ReturnsNotFound(int RequestID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var resultC = await Controller.ReproveProvideServices(RequestID, Context.Request.Find(RequestID));
            var viewResult = Assert.IsType<ViewResult>(resultC);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public async void ReproveProvideServicesPost_ReturnsNotFoundNotAuthorized(int RequestID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            var resultC = await Controller.ReproveProvideServices(RequestID, Context.Request.Find(RequestID));
            var viewResult = Assert.IsType<ViewResult>(resultC);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async void ShowPendingRequest_ReturnsViewResult()
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var resultC = await Controller.ShowPendingRequestAsync();
            var viewResult = Assert.IsType<ViewResult>(resultC);
            var model = Assert.IsAssignableFrom<IEnumerable<Request>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.Request.Where(r => r.RequestStatus == RequestStatus.Analyzing ||
                r.RequestStatus == RequestStatus.WaitingApproval).Count(), model.Count());
        }
        
        [Fact]
        public async void ShowPendingRequest_ReturnsNotFoundNotAuthorized()
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            var resultC = await Controller.ShowPendingRequestAsync();
            var viewResult = Assert.IsType<ViewResult>(resultC);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }
    }
}