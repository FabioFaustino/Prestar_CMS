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


    public class NotificationsControllerTest : IClassFixture<ApplicationDbContextFixture>
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

        public NotificationsController Controller { get; set; }

        public NotificationsControllerTest(ApplicationDbContextFixture contextFixture)
        {

            Context = contextFixture.DbContext;
            RoleManager = contextFixture.RoleManager;
            UserManager = contextFixture.UserManager;

            Controller = new NotificationsController(Context, UserManager);

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

        
        // Utilizar o Prestador como utilizador logado, que é o destinatario das notificações em contexto.
        [Fact]
        public async void Index_ReturnViewResult()
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };

            var result = await Controller.Index("False");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Notification>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.Notification.Where(n => n.DestinaryID == Prestador.Id).Count(), model.Count());
        }


      
        [Fact]
        public async void Create_ReturnRedirecToAction()
        {
            var size = Context.Notification.Where(n => n.DestinaryID == Prestador.Id).Count();
            
            //Create with destinatary id = 2, which is the provider id

            var notification = new Notification()
            {
                NotificationID = 4,
                Subject = "Subject 4",
                Content = "notification 4",
                DestinaryID = "2",
                CreationDate = DateTime.Now,
                IsRead = false
                
            };

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await Controller.Create(notification);
           
            var actionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", actionResult.ActionName);
            Assert.Equal(size + 1, Context.Notification.Where(n => n.DestinaryID == Prestador.Id).Count());
        }

        [Theory]
        [InlineData(1)]
        public async void UpdateGiveSight_ReturnRedirectToAction(int notificationID)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await Controller.UpdateGiveSight(notificationID);
            var actionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", actionResult.ActionName);
            Assert.True(Context.Notification.Find(notificationID).IsRead);

        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async void UpdateGiveSight_ReturnNotFound(int notificationID)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };

            var result = await Controller.UpdateGiveSight(notificationID);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }


      

        [Theory]
        [InlineData(3)]
        public async void ClearNotification_ReturnRedirect(int id)
        {
            var size = Context.Notification.Count();


            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await Controller.ClearNotification(id);
            var actionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", actionResult.ActionName);
            
            Assert.Equal(size - 1, Context.Notification.Count());
        }

        [Theory]
        [InlineData(10)]
        [InlineData(-11)]
        public async void ClearNotification_ReturnNotFound(int notificationID)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };

            var result = await Controller.ClearNotification(notificationID);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }
        // estão 2 mensagens em nome do prestador, a 3 ficou com destinatario diferente pois é apagada
        [Fact]
        public async void AllRead_ReturnRedirectToAction()
        {
            var unreadNotifications = Context.Notification.Where(n => n.IsRead == false && n.DestinaryID == Prestador.Id).Count();

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };

            var result = await Controller.AllRead();

            
            var actionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", actionResult.ActionName);
            Assert.Equal(0, Context.Notification.Where(n => n.IsRead == false && n.DestinaryID == Prestador.Id).Count());
        }

        [Theory]
        [InlineData(1, "Services/Details/3")]
        public async void ViewDetails_InvalidURLRedirectToAction(int id, string url)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };

            var result = await Controller.ViewDetails(id, url);
            var actionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", actionResult.ActionName);

        }

    }
}
