using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prestar.Data;
using Prestar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace PrestarTest
{
    public class ApplicationDbContextFixture
    {
        public ApplicationDbContext DbContext { get; private set; }
        public RoleManager<IdentityRole> RoleManager { get; private set; }
        public UserManager<User> UserManager { get; private set; }
        public IEmailSender EmailSender { get; private set; }
       
        public User Administrador { get; private set; }
        public User Moderador { get; private set; }
        public User Cliente { get; private set; }
        public User Prestador { get; private set; }

        public IdentityRole ClienteRole { get; private set; }
        public IdentityRole ModeradoresRole { get; private set; }
        public IdentityRole PrestadorRole { get; private set; }
        public IdentityRole AdministradorRole { get; private set; }
        public ClaimsPrincipal ModeradorLog { get; private set; }
        public ClaimsPrincipal ClientLog { get; private set; }
        public ClaimsPrincipal PrestadorLog { get; private set; }
        public ClaimsPrincipal AdministradorLog { get; private set; }

        public ApplicationDbContextFixture()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;

            DbContext = new ApplicationDbContext(options);
            DbContext.Database.EnsureCreated();

            //users and roles context
            CreateRoles_Users();

            //Requests Context
            CreateContextRequests();

            //Service Categories Context
            CreateContexServiceCategories();

            //Services Context
            CreateContextServices();

            //UserManuals Context
            CreateContextManuals();

            //Sections Context 
            CreateContextSectionManuals();

            //CommentsAndEvaluation Context
            CreateContextCommentsAndEvaluation();

            //Formation Context
            CreateContextFormation();

            //PrivacyPolicySection Context
            CreateContextPrivacyPolicySection();

            //TermsAndConditions Context
            CreateContextTermsAndConditions();

            //About Context
            CreateContextAbout();
          
            //News Context
            CreateContextNews();

            //Gamification Context
            CreateContextGamification();

            //Notification Context
            CreateContextNotification();

            //ServiceRequisition Context
            CreateContextServiceRequisition();
        }

        private void CreateContextServiceRequisition()
        {
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

        }

        public void CreateContextRequests()
        {
            DbContext.Request.Add(new Request()
        {
            RequestID = 1,
                RequisitionerID = "4",
                RequestType = RequestType.ProvideServices,
                Description = "Para efeitos de teste",
                CreationDateTime = DateTime.Now,
                RequestStatus = RequestStatus.Analyzing
            });
            DbContext.Request.Add(new Request()
        {
            RequestID = 2,
                RequisitionerID = "4",
                RequestType = RequestType.ProvideServices,
                Description = "Para efeitos de teste 2",
                CreationDateTime = DateTime.Now,
                RequestStatus = RequestStatus.Analyzing
            });
            DbContext.Request.Add(new Request()
        {
            RequestID = 3,
                RequisitionerID = "4",
                RequestType = RequestType.ProvideServices,
                Description = "Para efeitos de teste 3",
                CreationDateTime = DateTime.Now,
                RequestStatus = RequestStatus.Analyzing
            });
            DbContext.SaveChanges();
        }
        public void CreateContexServiceCategories()
        {
            DbContext.ServiceCategory.Add(new ServiceCategory
            {
                CategoryID = 1,
                IsSubcategory = true,
                Name = "Limpeza de Paredes"
            });
            DbContext.ServiceCategory.Add(new ServiceCategory
            {
                CategoryID = 2,
                IsSubcategory = true,
                Name = "Limpeza de Chaminés"
            });
            DbContext.ServiceCategory.Add(new ServiceCategory
            {
                CategoryID = 3,
                IsSubcategory = true,
                Name = "Limpeza de Chão"
            });
            DbContext.SaveChanges();

            List<ServiceCategory> subcategories = new()
            {
                DbContext.ServiceCategory.Find(1),
                DbContext.ServiceCategory.Find(2),
                DbContext.ServiceCategory.Find(3)
            };

            DbContext.ServiceCategory.Add(new ServiceCategory
            {
                CategoryID = 4,
                IsSubcategory = false,
                Name = "Limpeza",
                ServiceCategories = new List<ServiceCategory>(subcategories)
            });
            DbContext.ServiceCategory.Add(new ServiceCategory
            {
                CategoryID = 5,
                IsSubcategory = false,
                Name = "Construção",
                ServiceCategories = new List<ServiceCategory>()
            });
            DbContext.ServiceCategory.Add(new ServiceCategory
            {
                CategoryID = 6,
                IsSubcategory = false,
                Name = "Tomar Conta de Bebés",
                ServiceCategories = new List<ServiceCategory>()
            });
            DbContext.SaveChanges();
        }

        public void CreateContextServices()
        {
            DbContext.Service.Add(new Service()
            {
                ServiceID = 1,
                UserID = "2",
                ServiceCategoryID = 1,
                ServiceName = "Limpeza de Paredes",
                Description = "Limpeza de paredes interiores",
                IsActive = true,
                IsBlocked = false
            });
            DbContext.Service.Add(new Service()
            {
                ServiceID = 2,
                UserID = "2",
                ServiceCategoryID = 2,
                ServiceName = "Limpeza de Chaminés",
                Description = "Limpeza de chaminés com equipamento especial",
                IsActive = true,
                IsBlocked = false
            });
            DbContext.Service.Add(new Service()
            {
                ServiceID = 4,
                UserID = "2",
                ServiceCategoryID = 1,
                ServiceName = "Limpeza de Portas",
                Description = "Limpeza de portas com produtos adequados",
                IsActive = true,
                IsBlocked = false
            });
            DbContext.Service.Add(new Service()
            {
                ServiceID = 5,
                UserID = "2",
                ServiceCategoryID = 2,
                ServiceName = "Limpeza de Sofás",
                Description = "Para eliminar as manchas que ainda nao tenham entranhado no tecido",
                IsActive = true,
                IsBlocked = false
            });
            DbContext.SaveChanges();
        }
        public void CreateContextManuals()
        {
            DbContext.UserManual.Add(new UserManual
            {
                UserManualID = 1,
                Role = "Prestador",
                LastUpdate = DateTime.Now,
                LastUpdateUserID = "1"
            });
            DbContext.UserManual.Add(new UserManual
            {
                UserManualID = 2,
                Role = "Cliente",
                LastUpdate = DateTime.Now,
                LastUpdateUserID = "1"
            });
            DbContext.UserManual.Add(new UserManual
            {
                UserManualID = 3,
                Role = "Moderador",
                LastUpdate = DateTime.Now,
                LastUpdateUserID = "1"
            });
            DbContext.SaveChanges();
        }
        public void CreateContextSectionManuals()
        {
            DbContext.Section.Add(new Section()
            {
                SectionID = 1,
                Title = "Criar um serviço",
                Content = "Para criar um serviço voce deve primeiro pedir para ser prestador ...",
                UserManualID = 1
            });
            DbContext.Section.Add(new Section()
            {
                SectionID = 2,
                Title = "Requisitar um serviço",
                Content = "Para requisitar um serviço voce deve primeiro selecionar o serviço ...",
                UserManualID = 2
            });
            DbContext.Section.Add(new Section()
            {
                SectionID = 3,
                Title = "Promover cliente a prestador",
                Content = "Para promover um cliente a prestador deve ir a secção de pedidos ...",
                UserManualID = 3
            });
            DbContext.Section.Add(new Section()
            {
                SectionID = 4,
                Title = "Denuncias - Serviço",
                Content = "Para consultar as denuncias de serviços deve ir a secção de denuncias e...",
                UserManualID = 3
            });
            DbContext.Section.Add(new Section()
            {
                SectionID = 5,
                Title = "Denuncias - Prestador",
                Content = "Para consultar as denuncias de prestadores deve ir a secção de denuncias e...",
                UserManualID = 3
            });

            DbContext.SaveChanges();
        }

        public void CreateContextCommentsAndEvaluation() 
        {
            DbContext.CommentAndEvaluation.Add(new CommentAndEvaluation()
            {
                CommentAndEvaluationID = 1,
                ServiceID = 1,
                UserCommentingID = "2",
                Comment = "Serviço impecável",
                Evaluation = 4,
                CreationDate = DateTime.Now
            });
            DbContext.CommentAndEvaluation.Add(new CommentAndEvaluation()
            {
                CommentAndEvaluationID = 2,
                ServiceID = 1,
                UserCommentingID = "2",
                Comment = "Serviço espetacular",
                Evaluation = 3,
                CreationDate = DateTime.Now
            });
            DbContext.CommentAndEvaluation.Add(new CommentAndEvaluation()
            {
                CommentAndEvaluationID = 3,
                ServiceID = 1,
                UserCommentingID = "2",
                Comment = "Mas que belo serviço",
                Evaluation = 3,
                CreationDate = DateTime.Now
            });
            DbContext.CommentAndEvaluation.Add(new CommentAndEvaluation()
            {
                CommentAndEvaluationID = 4,
                ServiceID = 1,
                UserCommentingID = "2",
                Comment = "Não gostei, as paredes continuam sujas",
                Evaluation = 1,
                CreationDate = DateTime.Now
            });
            DbContext.SaveChanges();
        }
        public void CreateContextFormation() 
        {
            DbContext.Formation.Add(new Formation()
            {
                FormationID = 1,
                NumberOfRegistrations = 0,
                DurationMinutes = 30,
                Date = DateTime.Now.AddDays(10),
                Title = "Formação na plataforma",
                Content = "Venha aprender a tirar o maximo proveito...",
                MaxEnrollment = 20,
                Local = "Sede da bela vista",
                ResponsibleID = "1"
            });

            DbContext.Formation.Add(new Formation()
            {
                FormationID = 2,
                NumberOfRegistrations = 0,
                DurationMinutes = 60,
                Date = DateTime.Now.AddDays(15),
                Title = "Formação na plataforma para moderadores",
                Content = "Venha aprender a função de moderador...",
                MaxEnrollment = 20,
                Local = "Sede da bela vista",
                ResponsibleID = "1"
            });

            DbContext.Formation.Add(new Formation()
            {
                FormationID = 3,
                NumberOfRegistrations = 0,
                DurationMinutes = 30,
                Date = DateTime.Now.AddDays(10),
                Title = "Formação na plataforma para clientes",
                Content = "Venha aprender como requisitar serviços...",
                MaxEnrollment = 20,
                Local = "Sede da bela vista",
                ResponsibleID = "1"
            });
            DbContext.SaveChanges();
        }
        
        public void CreateContextPrivacyPolicySection() 
        {
            DbContext.PrivacyPolicySection.Add(new PrivacyPolicySection()
            {
                PrivacyPolicySectionID = 1,
                Title = "Proteção de Dados Pessoais",
                Content = "So serão mostrados os dados necessários para a utilização da aplicação",
                PrivacyPolicySectionLastUpdate = DateTime.Now,
                LastUpdateUserID = "1"
            });
            DbContext.SaveChanges();

            DbContext.PrivacyPolicySection.Add(new PrivacyPolicySection()
            {
                PrivacyPolicySectionID = 2,
                Title = "Divuldação de Dados Pessoais",
                Content = "Não serão divulgados",
                PrivacyPolicySectionLastUpdate = DateTime.Now,
                LastUpdateUserID = "1"

            });
            DbContext.SaveChanges();
        }

        public void CreateContextTermsAndConditions() 
        {
            DbContext.TermsAndConditionsSection.Add(new TermsAndConditionsSection()
            {
                TermsAndConditionsSectionID = 1,
                Title = "Uso indevido ",
                Content = "O uso pretendido da aplicação é a divulgação de serviços fidedignos",
                LastUpdate = DateTime.Now,
                LastUpdateUserID = "1"
            });

            DbContext.TermsAndConditionsSection.Add(new TermsAndConditionsSection()
            {
                TermsAndConditionsSectionID = 2,
                Title = "Partes envolvidas",
                Content = "O uso da aplicação para motivos ilegais não é da responsabilidade da camara nem dos desenvolvedores",
                LastUpdate = DateTime.Now,
                LastUpdateUserID = "1"
            });
            DbContext.TermsAndConditionsSection.Add(new TermsAndConditionsSection()
            {
                TermsAndConditionsSectionID = 3,
                Title = "Partes envolvidas - Continuação",
                Content = "Serão responsabilizados os utilizadores identificados",
                LastUpdate = DateTime.Now,
                LastUpdateUserID = "1"
            });
            DbContext.SaveChanges();
        }
        public void CreateContextAbout() 
        {
            DbContext.About.Add(new About()
            {
                AboutID = 1,
                Title = "Acerca de - Desenvolvedores",
                Content = "Esta plataforma foi desenvolvida por..:",
                AboutLastUpdate = DateTime.Now,
                LastUpdateUserID = "1"
            });
            DbContext.About.Add(new About()
            {
                AboutID = 2,
                Title = "Acerca de - Professores",
                Content = "O desenvolvimento teve o apoio de:",
                AboutLastUpdate = DateTime.Now,
                LastUpdateUserID = "1"
            });
            DbContext.About.Add(new About()
            {
                AboutID = 3,
                Title = "Acerca de - Nosso Bairro Nossa Cidade",
                Content = "Para ajudar na iniciativa..:",
                AboutLastUpdate = DateTime.Now,
                LastUpdateUserID = "1"
            });
            DbContext.SaveChanges();
        }

        public void CreateContextNews()
        {
            DbContext.New.Add(new New()
            {
                NewsID = 1,
                Title = "Noticia 1",
                SecondTitle = "Titulo secundário 1",
                Description = "Descrição de noticia de teste 1",
                PrincipalNew = false,
                Visible = false,
                CreationDate = DateTime.Now,
                WriterID = "1"
            });

            DbContext.New.Add(new New()
            {
                NewsID = 2,
                Title = "Noticia 2",
                SecondTitle = "Titulo secundário 2",
                Description = "Descrição de noticia de teste 2",
                PrincipalNew = true,
                Visible = true,
                CreationDate = DateTime.Now,
                WriterID = "1"
            });

            DbContext.New.Add(new New()
            {
                NewsID = 3,
                Title = "Noticia 3",
                SecondTitle = "Titulo secundário 3",
                Description = "Descrição de noticia de teste 3",
                PrincipalNew = false,
                Visible = false,
                CreationDate = DateTime.Now,
                WriterID = "1"
            });
            DbContext.SaveChanges();
        }

        public void CreateContextGamification()
        {
            DbContext.Gamification.Add(new Gamification()
            {
                GamificationID = 1,
                PointsPerComment = 10,
                PointsPerEvaluation = 10,
                PointsPerService = 20,
                GamificationName = "Teste 1",
                IsActive = true
            });

            DbContext.Gamification.Add(new Gamification()
            {
                GamificationID = 2,
                PointsPerComment = 10,
                PointsPerEvaluation = 10,
                PointsPerService = 20,
                GamificationName = "Teste 2",
                IsActive = false
            });

            DbContext.Gamification.Add(new Gamification()
            {
                GamificationID = 3,
                PointsPerComment = 10,
                PointsPerEvaluation = 10,
                PointsPerService = 20,
                GamificationName = "Teste 3",
                IsActive = false
            });

            DbContext.SaveChanges();
        }

        // Using provider as recipient
        public void CreateContextNotification()
        {
            DbContext.Notification.Add(new Notification()
            {
                NotificationID = 1,
                DestinaryID = "2",
                Subject = "Notificação teste 1",
                Content = "Conteudo teste 1",
                IsRead = false,
                CreationDate = DateTime.Now,
                Action = "Service/Details/2"
            });

            DbContext.Notification.Add(new Notification()
            {
                NotificationID = 2,
                DestinaryID = "2",
                Subject = "Notificação teste 2",
                Content = "Conteudo teste 2",
                IsRead = false,
                CreationDate = DateTime.Now,
                Action = "Service/Details/1"
            });

            DbContext.Notification.Add(new Notification()
            {
                NotificationID = 3,
                DestinaryID = "1",
                Subject = "Notificação teste 3",
                Content = "Conteudo teste 3",
                IsRead = false,
                CreationDate = DateTime.Now,
                Action = "Service/Details/3"
            });
            DbContext.SaveChanges();

        }


        public async void CreateRoles_Users()
        {
            RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(DbContext),
                null, null, null, new Mock<ILogger<RoleManager<IdentityRole>>>().Object);
            UserManager = new UserManager<User>(new UserStore<User>(DbContext), null,
                new PasswordHasher<User>(), null, null, null, null, null, new Mock<ILogger<UserManager<User>>>().Object);

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

            var moderador = new User
            {
                FirstName = "Moderador",
                LastName = "Teste",
                PhoneNumber = "123456989",
                UserName = "ModeradorTeste",
                Email = "moderador@prestar.pt",
                EmailConfirmed = true,
                Id = "1",
                Birthdate = new DateTime(2002, 10, 31),
                AccountCreationDate = DateTime.Now,
                LastSeen = DateTime.Now,
                LockoutEnd = DateTimeOffset.Now
            };

            if (UserManager.FindByNameAsync("ModeradorTeste").Result == null)
            {
                var result = await UserManager.CreateAsync(moderador, "123456");
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(moderador, moderadorRole.Name);
                }
            }

            var prestador = new User
            {
                FirstName = "Prestador",
                LastName = "Teste",
                PhoneNumber = "123456989",
                UserName = "PrestadorTeste",
                Email = "moderador@prestar.pt",
                EmailConfirmed = true,
                Id = "2",
                Birthdate = new DateTime(2002, 10, 31),
                AccountCreationDate = DateTime.Now,
                LastSeen = DateTime.Now,
                LockoutEnd = DateTimeOffset.Now,
                ReceiveNotifications = true
            };

            if (UserManager.FindByNameAsync("PrestadorTeste").Result == null)
            {
                var result = await UserManager.CreateAsync(prestador, "123456");
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(prestador, prestadorRole.Name);
                }
            }

            var administrador = new User
            {
                FirstName = "Administrador",
                LastName = "Teste",
                PhoneNumber = "123456989",
                UserName = "AdministradorTeste",
                Email = "admin@prestar.pt",
                EmailConfirmed = true,
                Id = "3",
                Birthdate = new DateTime(2002, 10, 31),
                AccountCreationDate = DateTime.Now,
                LastSeen = DateTime.Now,
                LockoutEnd = DateTimeOffset.Now
            };

            if (UserManager.FindByNameAsync("AdministradorTeste").Result == null)
            {
                var result = await UserManager.CreateAsync(administrador, "123456");
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(administrador, administradorRole.Name);
                }
            }

            var cliente = new User
            {
                Id = "4",
                FirstName = "Inês",
                LastName = "Botelho",
                Birthdate = new DateTime(2002, 10, 31),
                AccountCreationDate = DateTime.Now,
                LastSeen = DateTime.Now,
                Email = "ines.botelho@gmail.com",
                EmailConfirmed = true,
                UserName = "InêsBotelho"
            };

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
}
