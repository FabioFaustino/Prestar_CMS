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
using System.Threading.Tasks;
using System.Diagnostics;

namespace PrestarTest
{
    public class ApplicationDbContextFixtureComplaints
    {
        public ApplicationDbContext DbContext { get; private set; }
        public RoleManager<IdentityRole> RoleManager { get; private set; }
        public UserManager<User> UserManager { get; private set; }

        public Complaint ClientComplaint { get; private set; }
        public Complaint ProviderComplaint { get; private set; }
        public Complaint ServiceComplaint { get; private set; }

        public ClaimsPrincipal ModeradorLog { get; private set; }
        public ClaimsPrincipal ClientLog { get; private set; }
        public ClaimsPrincipal PrestadorLog { get; private set; }
        public ClaimsPrincipal AdministradorLog { get; private set; }


        public User Administrador { get; private set; }
        public User Moderador { get; private set; }
        public User Cliente { get; private set; }
        public User Prestador { get; private set; }

        public IdentityRole ClienteRole { get; private set; }
        public IdentityRole ModeradoresRole { get; private set; }
        public IdentityRole PrestadorRole { get; private set; }
        public IdentityRole AdministradorRole { get; private set; }

        public ServiceCategory CleaningCategory { get; private set; }
        public Service CleaningService { get; private set; }


        public ApplicationDbContextFixtureComplaints()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;

            DbContext = new ApplicationDbContext(options);
            DbContext.Database.EnsureCreated();

            CreateRoles_Users();


            //Creates and adds a Category to the context
            CleaningCategory = new ServiceCategory()
            {
                CategoryID = 1,
                Name = "Limpeza",
                IsSubcategory = false
            };
            DbContext.Add(CleaningCategory);
            DbContext.SaveChanges();


            //Creates and adds a Service to the context
            CleaningService = new Service()
            {
                ServiceID = 1,
                UserID = Prestador.Id,
                ServiceCategoryID = 1,
                ServiceName = "Limpezas ao domicilio",
                Description = "Faço limpezas ao domicilio durante a semana"
            };
            DbContext.Add(CleaningService);
            DbContext.SaveChanges();


            //Creates and Adds complaints to the context
            ClientComplaint = new Complaint()
            {
                ComplaintID = 1,
                CreationDate = DateTime.Now,
                ComplaintType = ComplaintType.ReportClient,
                Reason = "O cliente não pagou",
                UserComplaining = Prestador,
                ComplaintTargetUser = Cliente
            };

            ServiceComplaint = new Complaint()
            {
                ComplaintID = 2,
                CreationDate = DateTime.Now,
                ComplaintType = ComplaintType.ReportService,
                Reason = "O serviço é uma farsa",
                UserComplaining = Cliente,
                ComplaintTargetUser = Prestador
            };

            ProviderComplaint = new Complaint()
            {
                ComplaintID = 3,
                CreationDate = DateTime.Now,
                ComplaintType = ComplaintType.ReportServiceProvider,
                Reason = "O prestador nao apareceu",
                UserComplaining = Cliente,
                ComplaintTargetUser = Prestador
            };

            DbContext.ServiceRequisition.Add(new ServiceRequisition()
            {
                ServiceRequisitionID = 1,
                ServiceID = 1,
                ServiceRequisitionStatus = ServiceRequisitionStatus.Pending,
                CreationDate = DateTime.Now,
                ConclusionDate = DateTime.Now.AddDays(5),
                AdditionalRequestInfo = "Preciso do serviço urgentemente",
                RequisitionerID = "1",
                LastUpdatedBy = "1"
            }
               );

            DbContext.Add(ClientComplaint);
            DbContext.Add(ProviderComplaint);
            DbContext.Add(ServiceComplaint);
            DbContext.SaveChanges();

        }

        public void Dispose()
        {
            Dispose();
        }

        public async void CreateRoles_Users()
        {
            RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(DbContext), null, null, null, new Mock<ILogger<RoleManager<IdentityRole>>>().Object);
            var clienteRole = new IdentityRole("Cliente");
            var prestadorRole = new IdentityRole("Prestador");
            var moderadorRole = new IdentityRole("Moderador");
            var administradorRole = new IdentityRole("Administrador");

            if (!await RoleManager.RoleExistsAsync(clienteRole.Name))
            {
                await RoleManager.CreateAsync(clienteRole);
            }
            if (!await RoleManager.RoleExistsAsync(prestadorRole.Name))
            {
                await RoleManager.CreateAsync(prestadorRole);
            }
            if (!await RoleManager.RoleExistsAsync(moderadorRole.Name))
            {
                await RoleManager.CreateAsync(moderadorRole);
            }
            if (!await RoleManager.RoleExistsAsync(administradorRole.Name))
            {
                await RoleManager.CreateAsync(administradorRole);
            }

            UserManager = new UserManager<User>(new UserStore<User>(DbContext), null, new PasswordHasher<User>(), null, null, null, null, null, new Mock<ILogger<UserManager<User>>>().Object);
            var moderador = new User { FirstName = "Moderador", LastName = "Teste", PhoneNumber = "123456989", UserName = "ModeradorTeste", Email = "moderador@prestar.pt", EmailConfirmed = true, Id = "1", Birthdate = new DateTime(2002, 10, 31), AccountCreationDate = DateTime.Now, LastSeen = DateTime.Now };
            if (UserManager.FindByNameAsync("moderador@prestar.pt").Result == null)
            {
                var result = await UserManager.CreateAsync(moderador, "123456");
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(moderador, moderadorRole.Name);
                }
            }
            UserManager = new UserManager<User>(new UserStore<User>(DbContext), null, new PasswordHasher<User>(), null, null, null, null, null, new Mock<ILogger<UserManager<User>>>().Object);
            var prestador = new User { FirstName = "Prestador", LastName = "Teste", PhoneNumber = "123456989", UserName = "PrestadorTeste", Email = "moderador@prestar.pt", EmailConfirmed = true, Id = "2", Birthdate = new DateTime(2002, 10, 31), AccountCreationDate = DateTime.Now, LastSeen = DateTime.Now };
            if (UserManager.FindByNameAsync("moderador@prestar.pt").Result == null)
            {
                var result = await UserManager.CreateAsync(prestador, "123456");
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(prestador, prestadorRole.Name);
                }
            }
            UserManager = new UserManager<User>(new UserStore<User>(DbContext), null, new PasswordHasher<User>(), null, null, null, null, null, new Mock<ILogger<UserManager<User>>>().Object);
            var administrador = new User { FirstName = "Administrador", LastName = "Teste", PhoneNumber = "123456989", UserName = "AdministradorTeste", Email = "moderador@prestar.pt", EmailConfirmed = true, Id = "3", Birthdate = new DateTime(2002, 10, 31), AccountCreationDate = DateTime.Now, LastSeen = DateTime.Now };
            if (UserManager.FindByNameAsync("moderador@prestar.pt").Result == null)
            {
                var result = await UserManager.CreateAsync(administrador, "123456");
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(administrador, administradorRole.Name);
                }
            }

            var cliente = new User { Id = "4", FirstName = "Inês", LastName = "Botelho", Birthdate = new DateTime(2002, 10, 31), AccountCreationDate = DateTime.Now, LastSeen = DateTime.Now, Email = "ines.botelho@gmail.com", EmailConfirmed = true, UserName = "InêsBotelho" };
            if (UserManager.FindByNameAsync("InêsBotelho").Result == null)
            {
                var result = await UserManager.CreateAsync(cliente, "123456");
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(cliente, clienteRole.Name);
                }
            }
            DbContext.SaveChanges();

            Moderador = UserManager.FindByNameAsync("ModeradorTeste").Result;
            Cliente = UserManager.FindByNameAsync("InêsBotelho").Result;
            Prestador = UserManager.FindByNameAsync("PrestadorTeste").Result;
            Administrador = UserManager.FindByNameAsync("AdministradorTeste").Result;

            ClienteRole = RoleManager.FindByNameAsync("Cliente").Result;
            ModeradoresRole = RoleManager.FindByNameAsync("Moderador").Result;
            PrestadorRole = RoleManager.FindByNameAsync("Prestador").Result;
            AdministradorRole = RoleManager.FindByNameAsync("Administrador").Result;

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
            AdministradorLog = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, Administrador.UserName),
                    new Claim(ClaimTypes.Email, Administrador.Email),
                    new Claim(ClaimTypes.NameIdentifier, Administrador.Id),
                    new Claim(ClaimTypes.Role, AdministradorRole.Name),
                }, "Test"));
            PrestadorLog = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, Prestador.UserName),
                    new Claim(ClaimTypes.Email, Prestador.Email),
                    new Claim(ClaimTypes.NameIdentifier, Prestador.Id),
                    new Claim(ClaimTypes.Role, PrestadorRole.Name),
                }, "Test"));
        }
    }


    public class ComplaintsControllerTest : IClassFixture<ApplicationDbContextFixtureComplaints>
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

        public Complaint ClientComplaint { get; private set; }
        public Complaint ProviderComplaint { get; private set; }
        public Complaint ServiceComplaint { get; private set; }

        public ServiceCategory CleaningCategory { get; private set; }
        public Service CleaningService { get; private set; }

        public ComplaintsControllerTest(ApplicationDbContextFixtureComplaints contextFixture)
        {
            Context = contextFixture.DbContext;
            RoleManager = contextFixture.RoleManager;
            UserManager = contextFixture.UserManager;

            ClientComplaint = contextFixture.ClientComplaint;
            ProviderComplaint = contextFixture.ProviderComplaint;
            ServiceComplaint = contextFixture.ServiceComplaint;

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

            CleaningService = contextFixture.CleaningService;
            CleaningCategory = contextFixture.CleaningCategory;

        }


        [Fact]
        public async void Index_ReturnsViewResult()
        {
            var controller = new ComplaintsController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = await controller.Index(1);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Complaint>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.Complaint.Count(), model.Count());
            
        }

        
        [Theory]
        [InlineData(2)]
        public async void Details_ReturnsViewResult(int ComplaintID)
        {
            //Arrange
            var controller = new ComplaintsController(Context, UserManager);
            controller.ModelState.Clear();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            //Act
            var result = await controller.Details(ComplaintID);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Complaint>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(ComplaintID, model.ComplaintID);
        }
        
        [Theory]
        [InlineData(20)]
        public async void Details_ReturnsNotFound(int ComplaintID)
        {
            var controller = new ComplaintsController(Context, UserManager);

            var result = await controller.Details(ComplaintID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        
        [Fact]// Id do serviço
        public async void Create_ReturnsViewResult()
        {
            //Arrange
            var newProviderComplaint = new Complaint()
            {
                ComplaintID = 4,
                CreationDate = DateTime.Now,
                ComplaintType = ComplaintType.ReportServiceProvider,
                Reason = "O prestador nao apareceu"
            };


            var controller = new ComplaintsController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            //Act
            var result = await controller.Create(newProviderComplaint, 1);

            //Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.NotNull(Context.Complaint.Where(c=>c.ComplaintID == 4).FirstOrDefault());
        }
        
        [Theory]
        [InlineData(2)]
        public void Edit_ReturnsViewResult(int ComplaintID)
        {
            var controller = new ComplaintsController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = controller.Edit(ComplaintID);
            var viewResult = Assert.IsType<ViewResult>(result.Result);
            var model = Assert.IsAssignableFrom<Complaint>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(ComplaintID, model.ComplaintID);

        }
        
        [Fact]
        public async void EditConfirmed_ReturnsRedirectToActionResult()
        {
            //Arrange
            var complaint = Context.Complaint.Find(2);


            complaint.Reason = "Motivo editado";

            var controller = new ComplaintsController(Context, UserManager);

            //Act
            var result = await controller.Edit(2, complaint);

            //Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            
            
            Assert.Equal("Motivo editado", Context.Complaint.Find(2).Reason);
        }
        
        [Fact]
        public async void EditConfirmed_ReturnsNotFound()
        {
            //Arrange
            var controller = new ComplaintsController(Context, UserManager);

            //Act
            var result = await controller.Edit(1, Context.Complaint.Find(2));

            //Assert
            
            var viewResult =  Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);

        }

        [Theory]
        [InlineData(-3)]
        public async void Delete_ReturnsNotFound(int ComplaintID)
        {
            var controller = new ComplaintsController(Context, UserManager);
            var result = await controller.Delete(ComplaintID);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(1)]
        public void Delete_ReturnsViewResult(int ComplaintID)
        {
            var controller = new ComplaintsController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };
            var result = controller.Delete(ComplaintID);
            var viewResult = Assert.IsType<ViewResult>(result.Result);
            var model = Assert.IsAssignableFrom<Complaint>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(ComplaintID, model.ComplaintID);
        }

        [Theory]
        [InlineData(9)]
        public async void DeleteConfirmed_ReturnsNotFound(int ComplaintID)
        {
            var controller = new ComplaintsController(Context, UserManager);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };
            var result = await controller.DeleteConfirmed(ComplaintID);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(1)]
        public void DeleteConfirmed_ReturnsViewResult(int ComplaintID)
        {
            var controller = new ComplaintsController(Context, UserManager);
            var result = controller.DeleteConfirmed(ComplaintID);          
            Assert.IsType<RedirectToActionResult>(result.Result);
        }

    }
}
