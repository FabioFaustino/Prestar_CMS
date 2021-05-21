using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
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
   

    public class CommentsAndEvaluationControllerTest : IClassFixture<ApplicationDbContextFixture>
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
        public CommentAndEvaluationsController Controller { get; set; }
        public IEmailSender EmailSender { get; set; }

        public CommentsAndEvaluationControllerTest(ApplicationDbContextFixture contextFixture)
        {
            Context = contextFixture.DbContext;
            RoleManager = contextFixture.RoleManager;
            UserManager = contextFixture.UserManager;

            Controller = new CommentAndEvaluationsController(Context, UserManager,EmailSender);

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
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };
            var result = await Controller.Index();
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<CommentAndEvaluation>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.CommentAndEvaluation.Count(), model.Count());
        }

        [Fact]
        public async void Create_ReturnRedirect()
        {
            var size = Context.CommentAndEvaluation.Count();

            var comment = new CommentAndEvaluation() { CommentAndEvaluationID = 5,  ServiceID = 4, UserCommentingID = "2", Comment = "Serviço impecável", Evaluation = 4, CreationDate = DateTime.Now };
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await Controller.Create(comment);
            Assert.IsType<RedirectToActionResult>(result);

            //Confirmar Comments
            var resultIndex = await Controller.Index();
            var viewResultIndex = Assert.IsType<ViewResult>(resultIndex);
            var modelIndex = Assert.IsAssignableFrom<IEnumerable<CommentAndEvaluation>>(viewResultIndex.ViewData.Model);
            Assert.NotNull(modelIndex);
            Assert.Equal(size + 1, modelIndex.Count());
        }


        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void Edit_ReturnPartialViewResult(int commentID)
        {            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await Controller.Edit(commentID);
            var viewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsAssignableFrom<CommentAndEvaluation>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(commentID, model.CommentAndEvaluationID);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async void Edit_ReturnNotFound(int commentID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await Controller.Edit(commentID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }


        [Theory]
        [InlineData(3)]
        [InlineData(-1)]
        public async void EditPost_ReturnNotFound(int commentID)
        {
            var commentUpdate = new CommentAndEvaluation() { CommentAndEvaluationID = 5, ServiceID = 4, UserCommentingID = "2", Comment = "Serviço impecável", Evaluation = 3, CreationDate = DateTime.Now };
            Assert.Equal("Serviço impecável", Context.CommentAndEvaluation.Find(5).Comment);
            Assert.NotEqual(Context.CommentAndEvaluation.Find(4).Comment, commentUpdate.Comment);

            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await Controller.Edit(commentID, commentUpdate);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(4)]
        public async void Delete_ReturnServiceViewResult(int commentID)
        {
            var size = Context.CommentAndEvaluation.Count();
            var comment = Context.CommentAndEvaluation.Find(commentID);

            
            var servicesController = new ServicesController(Context, UserManager);

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            servicesController.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };

            var result = await Controller.Delete(commentID);
            Assert.IsType<ViewResult>(servicesController.Details(comment.ServiceID).Result);

            //Confirmar Index
            var resultIndex = await Controller.Index();
            var viewResultIndex = Assert.IsType<ViewResult>(resultIndex);
            var modelIndex = Assert.IsAssignableFrom<IEnumerable<CommentAndEvaluation>>(viewResultIndex.ViewData.Model);
            Assert.NotNull(modelIndex);
            Assert.Equal(size - 1, modelIndex.Count());
        }

        [Theory]
        [InlineData(10)]
        [InlineData(-11)]
        public async void Delete_ReturnNotFound(int commentID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await Controller.Delete(commentID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }
    }


}
