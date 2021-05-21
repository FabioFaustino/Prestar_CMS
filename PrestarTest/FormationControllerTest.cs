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

    public class FormationControllerTest : IClassFixture<ApplicationDbContextFixture>
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
        public FormationsController Controller { get; private set; }


        public FormationControllerTest(ApplicationDbContextFixture contextFixture)
        {
            Context = contextFixture.DbContext;
            RoleManager = contextFixture.RoleManager;
            UserManager = contextFixture.UserManager;

            Controller = new FormationsController(Context, UserManager);

            Moderador = contextFixture.Moderador;
            Cliente = contextFixture.Cliente;          

            ModeradorLog = contextFixture.ModeradorLog;
            ClientLog = contextFixture.ClientLog;
       
            ClienteRole = contextFixture.ClienteRole;
            ModeradoresRole = contextFixture.ModeradoresRole;
            
        }

        [Fact]
        public async void Index_ReturnsViewResult()
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Index();
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Formation>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.Formation.Count(), model.Count());
        }


        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void Edit_ReturnViewResult(int id)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Edit(id);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Formation>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(id, model.FormationID);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async void Edit_ReturnNotFound(int id)
        {
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Edit(id);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }


        [Theory]
        [InlineData(4)]
        [InlineData(-1)]
        public async void EditPost_ReturnNotFound(int id)
        {
            var formationUpdate = new Formation
            {
                FormationID = 2,
                NumberOfRegistrations = 0,
                DurationMinutes = 60,
                Date = DateTime.Now.AddDays(15),
                Title = "Test Edit",
                Content = "Venha aprender a função de moderador...",
                MaxEnrollment = 20,
                Local = "Sede da bela vista",
                ResponsibleID = "1"
            };

            Assert.Equal("Formação na plataforma para moderadores", Context.Formation.Find(2).Title);
            Assert.NotEqual(Context.Formation.Find(2).Title, formationUpdate.Title);

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await Controller.Edit(id, formationUpdate);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }


        [Theory]
        [InlineData(1)]
        public async void DeleteConfirmed_ReturnsRedirectToActionIndex(int formationID)
        {
            var size = Context.Formation.Count();

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            var result = await Controller.DeleteConfirmed(formationID);
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", viewResult.ActionName);

            //Confirmar o Indice
            var resultI = await Controller.Index();
            var viewResultI = Assert.IsType<ViewResult>(resultI);
            var modelI = Assert.IsAssignableFrom<IEnumerable<Formation>>(viewResultI.ViewData.Model);
            Assert.NotNull(modelI);
            Assert.Equal(size -1 , modelI.Count());
        }

        [Theory]
        [InlineData(-10)]
        [InlineData(10)]
        public async void DeleteConfirmed_ReturnsNotFound(int formationID)
        {

            var result = await Controller.DeleteConfirmed(formationID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            //Confirmar o Indice
            var resultI = await Controller.Index();
            var viewResultI = Assert.IsType<ViewResult>(resultI);
            var modelI = Assert.IsAssignableFrom<IEnumerable<Formation>>(viewResultI.ViewData.Model);
            Assert.NotNull(modelI);
            Assert.Equal(Context.Request.Count(), modelI.Count());
        }


        
        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public async void Enrollment_ReturnRedirecToAction(int formationID)
        {
            
            var currentEnrollements= Context.Formation.Find(formationID).NumberOfRegistrations;
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            var resultC = await Controller.Enrollment(formationID);
            var resultAction = Assert.IsType<RedirectToActionResult>(resultC);
            Assert.Equal("Details", resultAction.ActionName);

            Assert.Equal(currentEnrollements + 1, Context.Formation.Find(formationID).NumberOfRegistrations);

            //Confirmar adição aos enrollments
            var resultI = await Controller.Participants(formationID);
            var viewResultI = Assert.IsType<ViewResult>(resultI);
            var model = Assert.IsAssignableFrom<IEnumerable<Enrollment>>(viewResultI.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.Formation.Find(formationID).Enrollments.Count(), model.Count());
            
        }

        [Theory]
        [InlineData(10)]
        [InlineData(-1)]
        public async void Enrollment_ReturnsNotFound(int formationID)
        {
            
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            var resultC = await Controller.Enrollment(formationID);
            var viewResult = Assert.IsType<ViewResult>(resultC);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public async void RemoveEnrollment_ReturnRedirecToAction(int formationID)
        {

            var currentEnrollements = Context.Formation.Find(formationID).NumberOfRegistrations;
            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            var resultC = await Controller.RemoveEnrollment(formationID);
            _ = Assert.IsType<RedirectToActionResult>(resultC);

            Assert.Equal(currentEnrollements - 1, Context.Formation.Find(formationID).NumberOfRegistrations);

            //Confirmar adição aos enrollments
            var resultI = await Controller.Participants(formationID);
            var viewResultI = Assert.IsType<ViewResult>(resultI);
            var model = Assert.IsAssignableFrom<IEnumerable<Enrollment>>(viewResultI.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.Formation.Find(formationID).Enrollments.Count(), model.Count());
        }

        [Theory]
        [InlineData(10)]
        [InlineData(-1)]
        public async void RemoveEnrollment_ReturnsNotFound(int formationID)
        {

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            var resultC = await Controller.Enrollment(formationID);
            var viewResult = Assert.IsType<ViewResult>(resultC);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(2)]
        public async void Participants_ReturnViewResult(int formationID)
        {
            var currentEnrollements = Context.Formation.Find(formationID).NumberOfRegistrations;

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            var resultC = await Controller.Participants(formationID);
            var viewResult = Assert.IsType<ViewResult>(resultC);
            var model = Assert.IsAssignableFrom<IEnumerable<Enrollment>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.Formation.Find(formationID).Enrollments.Count(), model.Count());
        }

        [Theory]
        [InlineData(10)]
        [InlineData(-1)]
        public async void Participants_ReturnNotFound(int formationID)
        {

            Controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            var resultC = await Controller.Participants(formationID);
            var viewResult = Assert.IsType<ViewResult>(resultC);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }



    }
}