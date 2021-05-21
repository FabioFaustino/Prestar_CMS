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
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PrestarTest
{
    public class ApplicationDbContextFixtureServiceRequisitions
    {
        public ApplicationDbContext DbContext { get; private set; }
        public RoleManager<IdentityRole> RoleManager { get; private set; }
        public UserManager<User> UserManager { get; private set; }
        public IEmailSender EmailSender { get; private set; }
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

        public ServiceRequisition PendingServiceRequisition { get; private set; }
        public ServiceRequisition AcceptedServiceRequisition { get; private set; }
        public ServiceRequisition AcceptedServiceRequisition2 { get; private set; }
        public ServiceRequisition ConcludedServiceRequisition { get; private set; }
        public ServiceRequisition CancelledServiceRequisition { get; private set; }
        public ServiceRequisition RejectedServiceRequisition { get; private set; }

        public ServiceCategory CleaningCategory { get; private set; }

        public Service CleaningService { get; private set; }



        public ApplicationDbContextFixtureServiceRequisitions()
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
                ServiceName = "Faço limpezas ao domicilio",
                Description = "Faço limpezas ao domicilio durante a semana de manha"
            };
            DbContext.Add(CleaningService);
            DbContext.SaveChanges();


            //Creates and Adds service requisitions to the context
            PendingServiceRequisition = new ServiceRequisition()
            {
                ServiceRequisitionID = 1,
                RequisitionerID = Cliente.Id,
                ServiceID = CleaningService.ServiceID,
                ServiceRequisitionStatus = ServiceRequisitionStatus.Pending,
                AdditionalRequestInfo = "Limpeza de Prédio - 2x por semana",
                CreationDate = DateTime.Now,
                LastUpdatedTime = DateTime.Now,
                LastUpdatedBy = Cliente.UserName,
                Service = CleaningService,
                Requisitioner = Cliente
            };

            AcceptedServiceRequisition = new ServiceRequisition()
            {
                ServiceRequisitionID = 2,
                RequisitionerID = Cliente.Id,
                ServiceID = CleaningService.ServiceID,
                ServiceRequisitionStatus = ServiceRequisitionStatus.Accepted,
                AdditionalRequestInfo = "Limpeza de Prédio - 2x por semana",
                CreationDate = PendingServiceRequisition.CreationDate,
                LastUpdatedTime = DateTime.Now,
                LastUpdatedBy = Prestador.UserName,
                Service = CleaningService,
                Requisitioner = Cliente
            };

            ConcludedServiceRequisition = new ServiceRequisition()
            {
                ServiceRequisitionID = 3,
                RequisitionerID = Cliente.Id,
                ServiceID = CleaningService.ServiceID,
                ServiceRequisitionStatus = ServiceRequisitionStatus.Concluded,
                AdditionalRequestInfo = "Limpeza de Prédio - 2x por semana",
                CreationDate = PendingServiceRequisition.CreationDate,
                LastUpdatedTime = DateTime.Now,
                LastUpdatedBy = Prestador.UserName,
                Service = CleaningService,
                Requisitioner = Cliente
            };

            CancelledServiceRequisition = new ServiceRequisition()
            {
                ServiceRequisitionID = 4,
                RequisitionerID = Cliente.Id,
                ServiceID = CleaningService.ServiceID,
                ServiceRequisitionStatus = ServiceRequisitionStatus.Cancelled,
                AdditionalRequestInfo = "Limpeza de Prédio - 2x por semana",
                CreationDate = PendingServiceRequisition.CreationDate,
                LastUpdatedTime = DateTime.Now,
                LastUpdatedBy = Prestador.UserName,
                Service = CleaningService,
                Requisitioner = Cliente
            };

            RejectedServiceRequisition = new ServiceRequisition()
            {
                ServiceRequisitionID = 5,
                RequisitionerID = Cliente.Id,
                ServiceID = CleaningService.ServiceID,
                ServiceRequisitionStatus = ServiceRequisitionStatus.Rejected,
                AdditionalRequestInfo = "Limpeza de Prédio - 2x por semana",
                CreationDate = PendingServiceRequisition.CreationDate,
                LastUpdatedTime = DateTime.Now,
                LastUpdatedBy = Prestador.UserName,
                Service = CleaningService,
                Requisitioner = Cliente
            };

            AcceptedServiceRequisition2 = new ServiceRequisition()
            {
                ServiceRequisitionID = 6,
                RequisitionerID = Cliente.Id,
                ServiceID = CleaningService.ServiceID,
                ServiceRequisitionStatus = ServiceRequisitionStatus.Accepted,
                AdditionalRequestInfo = "Limpeza de Prédio - 2x por semana",
                CreationDate = PendingServiceRequisition.CreationDate,
                LastUpdatedTime = DateTime.Now,
                LastUpdatedBy = Prestador.UserName,
                ConclusionDate = DateTime.Now,
                Service = CleaningService,
                Requisitioner = Cliente
            };

            DbContext.Add(PendingServiceRequisition);
            DbContext.Add(AcceptedServiceRequisition);
            DbContext.Add(CancelledServiceRequisition);
            DbContext.Add(ConcludedServiceRequisition);
            DbContext.Add(RejectedServiceRequisition);
            DbContext.Add(AcceptedServiceRequisition2);
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

    public class ServiceRequisitionsControllerTest : IClassFixture<ApplicationDbContextFixtureServiceRequisitions>
    {
        public ApplicationDbContext Context { get; private set; }
        public RoleManager<IdentityRole> RoleManager { get; private set; }
        public UserManager<User> UserManager { get; private set; }
        public IEmailSender EmailSender { get; private set; }
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

        public ServiceRequisition PendingServiceRequisition { get; private set; }
        public ServiceRequisition AcceptedServiceRequisition { get; private set; }
        public ServiceRequisition CancelledServiceRequisition { get; private set; }
        public ServiceRequisition ConcludedServiceRequisition { get; private set; }
        public ServiceRequisition RejectedServiceRequisition { get; private set; }
        public ServiceRequisition AcceptedServiceRequisition2 { get; private set; }

        public ServiceCategory CleaningCategory { get; private set; }
        public Service CleaningService { get; private set; }

        public ServiceRequisitionsControllerTest(ApplicationDbContextFixtureServiceRequisitions contextFixture)
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

            CleaningService = contextFixture.CleaningService;
            CleaningCategory = contextFixture.CleaningCategory;

            PendingServiceRequisition = contextFixture.PendingServiceRequisition;
            AcceptedServiceRequisition = contextFixture.AcceptedServiceRequisition;
            CancelledServiceRequisition = contextFixture.CancelledServiceRequisition;
            ConcludedServiceRequisition = contextFixture.ConcludedServiceRequisition;
            RejectedServiceRequisition = contextFixture.RejectedServiceRequisition;
            AcceptedServiceRequisition2 = contextFixture.AcceptedServiceRequisition2;
        }

        private ServiceRequisitionsController SetController()
        {
            return new ServiceRequisitionsController(Context, UserManager, EmailSender);
        }

        [Fact]
        public async void Index_ReturnsViewResult()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            //Act
            var result = await controller.Index("");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ServiceRequisition>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.ServiceRequisition.Count(), model.Count());
        }

        [Fact]
        public async void Index_Pending_ReturnsViewResult()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            //Act
            var result = await controller.Index("Pendente");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ServiceRequisition>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.ServiceRequisition.Where(sr=>sr.ServiceRequisitionStatus == ServiceRequisitionStatus.Pending).Count(), model.Count());
        }

        [Fact]
        public async void Index_Cancelled_ReturnsViewResult()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            //Act
            var result = await controller.Index("Cancelado");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ServiceRequisition>>(viewResult.ViewData.Model);
            Assert.NotNull(model); 
            Assert.Equal(Context.ServiceRequisition.Where(sr => sr.ServiceRequisitionStatus == ServiceRequisitionStatus.Cancelled).Count(), model.Count());
        }

        [Fact]
        public async void Index_Accepted_ReturnsViewResult()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            //Act
            var result = await controller.Index("Aceite");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ServiceRequisition>>(viewResult.ViewData.Model);
            Assert.NotNull(model); 
            Assert.Equal(Context.ServiceRequisition.Where(sr => sr.ServiceRequisitionStatus == ServiceRequisitionStatus.Accepted).Count(), model.Count());
        }

        [Fact]
        public async void Index_Concluded_ReturnsViewResult()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            //Act
            var result = await controller.Index("Concluído");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ServiceRequisition>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.ServiceRequisition.Where(sr => sr.ServiceRequisitionStatus == ServiceRequisitionStatus.Concluded).Count(), model.Count());
        }

        [Fact]
        public async void Index_Rejected_ReturnsViewResult()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            //Act
            var result = await controller.Index("Rejeitado");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ServiceRequisition>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.ServiceRequisition.Where(sr => sr.ServiceRequisitionStatus == ServiceRequisitionStatus.Rejected).Count(), model.Count());
        }

        [Fact]
        public async void GetServiceRequisitions_ReturnsViewResult()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };

            //Act
            var result = await controller.GetServiceRequisitions(1, "");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ServiceRequisition>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.ServiceRequisition.Count(), model.Count());
        }

        [Fact]
        public async void GetServiceRequisitions_Accepted_ReturnsViewResult()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };

            //Act
            var result = await controller.GetServiceRequisitions(1, "Aceite");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ServiceRequisition>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.ServiceRequisition.Where(sr => sr.ServiceRequisitionStatus == ServiceRequisitionStatus.Accepted).Count(), model.Count());
        }

        [Fact]
        public async void GetServiceRequisitions_ReturnsNotFound()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            //Act
            var result = await controller.GetServiceRequisitions(1, "");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async void GetServiceRequisitions_Moderator_ReturnsNotFound()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            //Act
            var result = await controller.GetServiceRequisitions(1, "");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async void GetServiceRequisitions_Admin_ReturnsNotFound()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = AdministradorLog };

            //Act
            var result = await controller.GetServiceRequisitions(1, "");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async void GetAllServiceRequisitions_ReturnsViewResult()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };

            //Act
            var result = await controller.GetAllServiceRequisitions("");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ServiceRequisition>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.ServiceRequisition.Count(), model.Count());
        }

        [Fact]
        public async void GetAllServiceRequisitions_Accepted_ReturnsViewResult()
        {
            ///Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };

            //Act
            var result = await controller.GetAllServiceRequisitions("Aceite");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ServiceRequisition>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.ServiceRequisition.Where(sr => sr.ServiceRequisitionStatus == ServiceRequisitionStatus.Accepted).Count(), model.Count());
        }

        [Fact]
        public async void GetAllServiceRequisitions_Concluded_ReturnsViewResult()
        {
            ///Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };

            //Act
            var result = await controller.GetAllServiceRequisitions("Concluído");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ServiceRequisition>>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(Context.ServiceRequisition.Where(sr => sr.ServiceRequisitionStatus == ServiceRequisitionStatus.Concluded).Count(), model.Count());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async void Details_ReturnsViewResult(int ServiceRequisitionID)
        {
            //Arrange
            var controller = SetController();
            controller.ModelState.Clear();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };

            //Act
            var result = await controller.Details(ServiceRequisitionID);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ServiceRequisition>(viewResult.ViewData.Model);
            var contextResult = Context.ServiceRequisition.Where(sr => sr.ServiceRequisitionID == ServiceRequisitionID).FirstOrDefault();
            Assert.NotNull(model);
            Assert.Equal(ServiceRequisitionID, model.ServiceRequisitionID);
            Assert.Equal(contextResult.AdditionalRequestInfo, model.AdditionalRequestInfo);
            Assert.Equal(contextResult.LastUpdatedBy, model.LastUpdatedBy);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async void Details_Moderator_ReturnsViewResult(int ServiceRequisitionID)
        {
            //Arrange
            var controller = SetController();
            controller.ModelState.Clear();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            //Act
            var result = await controller.Details(ServiceRequisitionID);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ServiceRequisition>(viewResult.ViewData.Model);
            var contextResult = Context.ServiceRequisition.Where(sr => sr.ServiceRequisitionID == ServiceRequisitionID).FirstOrDefault();
            Assert.NotNull(model);
            Assert.Equal(ServiceRequisitionID, model.ServiceRequisitionID);
            Assert.Equal(contextResult.AdditionalRequestInfo, model.AdditionalRequestInfo);
            Assert.Equal(contextResult.LastUpdatedBy, model.LastUpdatedBy);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async void Details_Client_ReturnsViewResult(int ServiceRequisitionID)
        {
            //Arrange
            var controller = SetController();
            controller.ModelState.Clear();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            //Act
            var result = await controller.Details(ServiceRequisitionID);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ServiceRequisition>(viewResult.ViewData.Model);
            var contextResult = Context.ServiceRequisition.Where(sr => sr.ServiceRequisitionID == ServiceRequisitionID).FirstOrDefault();
            Assert.NotNull(model);
            Assert.Equal(ServiceRequisitionID, model.ServiceRequisitionID);
            Assert.Equal(contextResult.AdditionalRequestInfo, model.AdditionalRequestInfo);
            Assert.Equal(contextResult.LastUpdatedBy, model.LastUpdatedBy);
        }

        [Fact]
        public async void Details_NullID_ReturnsNotFound()
        {
            //Arrange
            var controller = SetController();
            controller.ModelState.Clear();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            //Act
            var result = await controller.Details(null);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(7)]
        [InlineData(80)]
        public async void Details_NotExistingID_ReturnsNotFound(int ServiceRequisitionID)
        {
            //Arrange
            var controller = SetController();
            controller.ModelState.Clear();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ModeradorLog };

            //Act
            var result = await controller.Details(ServiceRequisitionID);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async void Create_ReturnsViewResult()
        {
            //Arrange
            var controller = new ServiceRequisitionsController(Context, UserManager,EmailSender);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            //Act
            var result = await controller.CreateAsync(1);

            //Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void Create_NullId_ReturnsNotFound()
        {
            //Arrange
            var controller = new ServiceRequisitionsController(Context, UserManager,EmailSender);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            //Act
            var result = await controller.CreateAsync(null);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(0)]
        [InlineData(100)]
        public async void Create_NonExistingService_ReturnsNotFound(int ServiceID)
        {
            //Arrange
            var controller = new ServiceRequisitionsController(Context, UserManager,EmailSender);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            //Act
            var result = await controller.CreateAsync(ServiceID);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async void Create_Confirmation_NotExistingService_ReturnsNotFound()
        {
            //Arrange
            var newServiceRequisition = new ServiceRequisition()
            {
                ServiceRequisitionID = 9,
                RequisitionerID = Cliente.Id,
                ServiceID = CleaningService.ServiceID,
                ServiceRequisitionStatus = ServiceRequisitionStatus.Pending,
                AdditionalRequestInfo = "Limpeza de Prédio - 2x por semana",
                CreationDate = DateTime.Now,
                LastUpdatedTime = DateTime.Now,
                LastUpdatedBy = Prestador.UserName.Split('@')[0]
            };
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            //Act
            var result = await controller.Create(10, newServiceRequisition);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
            Assert.Null(Context.ServiceRequisition.Where(sr => sr.ServiceRequisitionID == 9).FirstOrDefault());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(6)]
        public void Edit_ReturnsViewResult(int ServiceRequisitionID)
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };

            //Act
            var result = controller.Edit(ServiceRequisitionID);

            //Assert
            Assert.IsType<ViewResult>(result.Result);
        }

        [Fact]
        public async void Edit_NullId_ReturnsNotFound()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            //Act
            var result = await controller.Edit(null);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(6)]
        public void Delete_ReturnsViewResult(int ServiceRequisitionID)
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };

            //Act
            var result = controller.Delete(ServiceRequisitionID);

            //Assert
            Assert.IsType<ViewResult>(result.Result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(6)]
        public void Delete_Client_ReturnsViewResult(int ServiceRequisitionID)
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            //Act
            var result = controller.Delete(ServiceRequisitionID);

            //Assert
            Assert.IsType<ViewResult>(result.Result);
        }

        [Fact]
        public async void Delete_NullId_ReturnsNotFound()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            //Act
            var result = await controller.Delete(null);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async void Delete_NonExistingRequisition_ReturnsNotFound()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = ClientLog };

            //Act
            var result = await controller.Delete(100);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Fact]
        public void ConcludeService_ReturnsViewResult()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };

            //Act
            var result = controller.ConcludeService(6);

            //Assert
            Assert.IsType<ViewResult>(result.Result);
        }

        [Fact]
        public async void ConcludeService_NullId_ReturnsNotFound()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };

            //Act
            var result = await controller.ConcludeService(null);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async void ConcludeService_NonExistingRequisition_ReturnsNotFound()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };

            //Act
            var result = await controller.ConcludeService(100);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/Shared/NotFound.cshtml", viewResult.ViewName);
        }

        [Fact]
        public void ConcludeService_AlreadyConcludedService_ReturnsNotFound()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };

            //Act
            var result = controller.ConcludeService(3);

            //Assert
            Assert.IsType<RedirectToActionResult>(result.Result);
        }

        [Fact]
        public void ConcludeService_RejectedService_ReturnsNotFound()
        {
            //Arrange
            var controller = SetController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = PrestadorLog };

            //Act
            var result = controller.ConcludeService(5);

            //Assert
            Assert.IsType<RedirectToActionResult>(result.Result);
        }
    }
}
