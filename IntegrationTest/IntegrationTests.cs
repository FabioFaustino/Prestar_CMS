using Microsoft.AspNetCore.Mvc.Testing;
using Prestar;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Prestar.Models;
using System.Collections.Generic;
using System;

namespace IntegrationTest
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Startup> _factory;

        public IntegrationTests(WebApplicationFactory<Startup> factory)
        {
            Environment.SetEnvironmentVariable("Authentication_Facebook_AppId", "400928241014722");
            Environment.SetEnvironmentVariable("Authentication_Facebook_AppSecret", "58e35a6db5be8b0a51679589bd8d2fb3");
            Environment.SetEnvironmentVariable("Authentication_Google_ClientId", "179854523322-33tua08hb3gkmp2h5nhfjrdvpghppv2q.apps.googleusercontent.com");
            Environment.SetEnvironmentVariable("Authentication_Google_ClientSecret", "UFYzNmgiapU2A4XrIXwj");
            _factory = factory;
            _client = factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });
        }

        [Theory]
        [InlineData("/Identity/Account/Login")]
        [InlineData("/Identity/Account/Register")]
        [InlineData("/UserManuals/Details")]
        [InlineData("/")]
        [InlineData("/Services")]
        public async Task NotAuthenticatedUsersCanAcessSomePages(string url)
        {
            //Arrange
            var client = _client;

            // Act
            var response = await client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        
        [Theory]
        [InlineData("/UserAccess")]
        [InlineData("/Notifications")]
        public async Task CannotAcessSomePages_WhenNotAuthenticatedUsers(string url)
        {
            //Arrange
            var client = _client;

           // Act
            var response = await client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }
        
        [Theory]
        [InlineData("/Identity/Account/Manage")]
        [InlineData("/Notifications?error=False")]
        public async Task CanAcessSomePages_WhenAuthenticatedCliente(string url)
        {
            //Arrange
            Authentication userLogin = new(_client);
            userLogin.AuthenticateUser("client@prestar.pt", "123456");

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        
        [Fact]
        public async Task AlterTypeOfCountToProvider_CannotInClient()
        {
            Authentication clientLogin = new(_client);
            clientLogin.AuthenticateUser("client@prestar.pt", "123456");

            var response = await _client.GetAsync("/UserAccess");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            response = await _client.GetAsync("/Requests");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            //Get Id's
            response = await _client.GetAsync("/UserAccess/GetIdByNameRole?name=Cliente");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            response = await _client.GetAsync("/UserAccess/GetIdByEmail?email=client@prestar.pt");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        
        [Fact]
        public async Task AlterTypeOfCountToProvider_CanInModerador()
        {
            Authentication login = new(_client);
            login.AuthenticateUser("moderator@prestar.pt", "123456");
            
            var response = await _client.GetAsync("/UserAccess");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            response = await _client.GetAsync("/Requests");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            //Get Id's
            response = await _client.GetAsync("/UserAccess/GetIdByNameRole?name=Cliente");
            var id_cliente = await response.Content.ReadAsStringAsync();
            response = await _client.GetAsync("/UserAccess/GetIdByNameRole?name=Moderador");
            var id_moderador = await response.Content.ReadAsStringAsync();
            response = await _client.GetAsync("/UserAccess/GetIdByNameRole?name=Prestador");
            var id_prestador = await response.Content.ReadAsStringAsync();
            response = await _client.GetAsync("/UserAccess/GetIdByNameRole?name=Administrador");
            var id_admin = await response.Content.ReadAsStringAsync();

            //Get User Cliente
            response = await _client.GetAsync("/UserAccess/GetIdByEmail?email=client@prestar.pt");
            var clienteUser = await response.Content.ReadAsStringAsync();

            //Turn Client in Provider
            var json = new Dictionary<string, string>
            {
                { "[0].RoleID", id_cliente },
                { "[0].RoleName", "Cliente" },
                { "[1].RoleID", id_moderador },
                { "[1].RoleName", "Moderador" },
                { "[2].RoleID", id_prestador },
                { "[2].RoleName", "Prestador" },
                { "[2].Selected", true.ToString() },
                { "[3].RoleID", id_admin },
                { "[3].RoleName", "Administrador" }
            };
            response = await _client.PostAsync("/UserAccess/Manage?userId=" + clienteUser, new FormUrlEncodedContent(json));
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);

            //Turn Client in just Client
            json = new Dictionary<string, string>
            {
                { "[0].RoleID", id_cliente },
                { "[0].RoleName", "Cliente" },
                { "[0].Selected", true.ToString() },
                { "[1].RoleID", id_moderador },
                { "[1].RoleName", "Moderador" },
                { "[2].RoleID", id_prestador },
                { "[2].RoleName", "Prestador" },
                { "[3].RoleID", id_admin },
                { "[3].RoleName", "Administrador" }
            };
            response = await _client.PostAsync("/UserAccess/Manage?userId=" + clienteUser, new FormUrlEncodedContent(json));
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        }
        
        [Theory]
        [InlineData("admin@prestar.pt", "123456")]
        [InlineData("moderator@prestar.pt", "123456")]
        public async Task FormationCreateEditDelete_CanInAdminOrModerator(string email, string password)
        {
            Authentication user = new(_client);
            user.AuthenticateUser(email, password);

            //Aceder à Criação - GET
            var response = await _client.GetAsync("/Formations/Create");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            //Criar uma Formação - POST
            var postRequest = new HttpRequestMessage(HttpMethod.Post, "/Formations/Create");
            var formation = new Dictionary<string, string>
            {
                { "NumberOfRegistrations", "0" },
                { "Title", "TesteA" },
                { "Content", "Contextualização sobre o que é o COVID e formação como lidar com o seu serviço em tempos de pandemia." },
                { "Illustration", null },
                { "Date", DateTime.Now.AddDays(3).ToString("d") },
                { "DurationMinutes", "90" },
                { "Local",  "Sede do Bairro da Ponte"},
                { "MaxEnrollment",  "100"}
            };
            postRequest.Content = new FormUrlEncodedContent(formation);
            response = await _client.SendAsync(postRequest);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            response = await _client.GetAsync("/Formations/GetIDByTitle?title=TesteA");
            var id = await response.Content.ReadAsStringAsync();
            
            //Aceder à Edição - GET
            response = await _client.GetAsync("/Formations/Edit/" + id);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            //Get User Cliente
            response = await _client.GetAsync("/UserAccess/GetIdByEmail?email=" + email);
            var createdBy = await response.Content.ReadAsStringAsync();

            //Editar uma Formação - POST
            formation = new Dictionary<string, string>
            {
                { "FormationID", id },
                { "ResponsibleID", createdBy },
                { "NumberOfRegistrations", "0" },
                { "Title", "A verdade sobre o COVID" },
                { "Content", "Contextualização sobre o que é o COVID e formação como lidar com o seu serviço em tempos de pandemia." },
                { "Illustration", null },
                { "Date", DateTime.Now.AddDays(5).ToString("d") },
                { "DurationMinutes", "90" },
                { "Local",  "Sede do Bairro da Ponte"},
                { "MaxEnrollment",  "100"},
            };
            postRequest = new HttpRequestMessage(HttpMethod.Post, "/Formations/Edit/" + id)
            {
                Content = new FormUrlEncodedContent(formation)
            };
            response = await _client.SendAsync(postRequest);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            //Aceder aos Detalhes de uma Formação - GET
            response = await _client.GetAsync("/Formations/Details/" + id);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            //Eliminar uma Formação - POST
            postRequest = new HttpRequestMessage(HttpMethod.Post, "/Formations/Delete/" + id);
            response = await _client.SendAsync(postRequest);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        
        [Theory]
        [InlineData("provider@prestar.pt", "123456")]
        [InlineData("client@prestar.pt", "123456")]
        public async Task FormationCreateEditDelete_CannotInProviderOrClient(string email, string password)
        {
            Authentication user = new(_client);
            user.AuthenticateUser(email, password);

            var response = await _client.GetAsync("/Formations/Edit/5");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            response = await _client.GetAsync("/Formations/Create");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        [Theory]
        [InlineData("admin@prestar.pt", "123456")]
        public async Task UserManualCreateEditDelete_CanInAdminOrModerator(string email, string password)
        {
            Authentication user = new(_client);
            user.AuthenticateUser(email, password);

            //Aceder à Criação - GET
            var response = await _client.GetAsync("/UserManuals/Create");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            //Criar um Manual de Utilizador - POST
            var userManual = new Dictionary<string, string>
            {
                { "Role", "Moderador" },
            };
            var postRequest = new HttpRequestMessage(HttpMethod.Post, "/UserManuals/Create")
            {
                Content = new FormUrlEncodedContent(userManual)
            };
            response = await _client.SendAsync(postRequest);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            response = await _client.GetAsync("/UserManuals/GetIdByRole?role=Moderador");
            var id = await response.Content.ReadAsStringAsync();

            //Aceder à Edição - GET
            response = await _client.GetAsync("/UserManuals/Edit/" + id);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            //Eliminar Manual de Utilizador
            postRequest = new HttpRequestMessage(HttpMethod.Post, "/UserManuals/Delete/" + id);
            response = await _client.SendAsync(postRequest);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }
        
        [Theory]
        [InlineData("provider@prestar.pt", "123456")]
        [InlineData("client@prestar.pt", "123456")]
        public async Task UserManualCreateEditDelete_CannotInProviderOrClient(string email, string password)
        {
            Authentication user = new(_client);
            user.AuthenticateUser(email, password);

            var response = await _client.GetAsync("/UserManuals/Details");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync("/UserManuals/Edit/5");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            response = await _client.GetAsync("/UserManuals/Create");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }
        
        [Theory]
        [InlineData("provider@prestar.pt", "123456")]
        [InlineData("client@prestar.pt", "123456")]
        [InlineData("admin@prestar.pt", "123456")]
        [InlineData("moderator@prestar.pt", "123456")]
        public async Task SeeService_WithAllRoles(string email, string password)
        {
            Authentication user = new(_client);
            user.AuthenticateUser(email, password);

            var response = await _client.GetAsync("/Services");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("provider@prestar.pt", "123456")]
        public async Task ServiceCreateEditDelete_CanInProvider(string email, string password)
        {
            Authentication user = new(_client);
            user.AuthenticateUser(email, password);
            
            //Aceder à Criação - GET
            var response = await _client.GetAsync("/Services/Create");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            //Get User Prestador
            response = await _client.GetAsync("/UserAccess/GetIdByEmail?email=provider@prestar.pt");
            var providerUser = await response.Content.ReadAsStringAsync();

            response = await _client.GetAsync("/ServiceCategories/CreateCategoriesToTest");
            var categories = await response.Content.ReadAsStringAsync();
            string category_id = categories.Split("|")[0];
            string subcategory_id = categories.Split("|")[1];

            //Criar um Serviço - POST
            var postRequest = new HttpRequestMessage(HttpMethod.Post, "/Services/Create");
            var service = new Dictionary<string, string>
            {
                { "UserID", providerUser },
                { "ServiceCategoryID", subcategory_id },
                { "ServiceName", "TesteA" },
                { "Description", "Limpeza de paredes interiores" },
                { "IsActive", "true" },
                { "IsBlocked",  "false"},
                { "Illustration",  null}
            };
            postRequest.Content = new FormUrlEncodedContent(service);
            response = await _client.SendAsync(postRequest);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            response = await _client.GetAsync("/Services/GetIdByTitle?title=TesteA");
            var id = await response.Content.ReadAsStringAsync();

            //Aceder à Edição - GET
            response = await _client.GetAsync("/Services/Edit/" + id);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            //Editar um Serviço - POST
            service = new Dictionary<string, string>
            {
                { "ServiceID", id },
                { "UserID", providerUser },
                { "ServiceCategoryID", subcategory_id },
                { "ServiceName", "TesteA" },
                { "Description", "Limpeza de Paredes Interiores e Exteriores" },
                { "IsActive", "true" },
                { "IsBlocked",  "false"},
                { "Illustration",  null}
            };
            postRequest = new HttpRequestMessage(HttpMethod.Post, "/Services/Edit/" + id)
            {
                Content = new FormUrlEncodedContent(service)
            };
            response = await _client.SendAsync(postRequest);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            //Aceder aos Detalhes de um Serviço - GET
            response = await _client.GetAsync("/Services/Details/" + id);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            //Eliminar um Serviço - POST
            postRequest = new HttpRequestMessage(HttpMethod.Post, "/Services/Delete/" + id);
            response = await _client.SendAsync(postRequest);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            //Eliminar categorias - POST
            postRequest = new HttpRequestMessage(HttpMethod.Post, "/ServiceCategories/DeleteCategoriesToTest?c=" + category_id + "&s=" + subcategory_id);
            response = await _client.SendAsync(postRequest);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        
        [Fact]
        public async Task RequisitionsCommentAndEvaluations_CanInProvider()
        {
            Authentication user = new(_client);
            user.AuthenticateUser("provider@prestar.pt", "123456");

            //GetPoints Inicial
            var response = await _client.GetAsync("/CommentAndEvaluations/GetPoints");
            int atualPoints = Int32.Parse(response.Content.ReadAsStringAsync().Result);

            //Get User Prestador
            response = await _client.GetAsync("/UserAccess/GetIdByEmail?email=provider@prestar.pt");
            var providerUser = await response.Content.ReadAsStringAsync();

            response = await _client.GetAsync("/ServiceCategories/CreateCategoriesToTest");
            var categories = await response.Content.ReadAsStringAsync();
            string category_id = categories.Split("|")[0];
            string subcategory_id = categories.Split("|")[1];

            //Criar um Serviço - POST
            var postRequest = new HttpRequestMessage(HttpMethod.Post, "/Services/Create");
            var service = new Dictionary<string, string>
            {
                { "UserID", providerUser },
                { "ServiceCategoryID", subcategory_id },
                { "ServiceName", "TesteA" },
                { "Description", "Limpeza de paredes interiores" },
                { "IsActive", "true" },
                { "IsBlocked",  "false"},
                { "Illustration",  null}
            };
            postRequest.Content = new FormUrlEncodedContent(service);
            response = await _client.SendAsync(postRequest);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            response = await _client.GetAsync("/Services/GetIdByTitle?title=TesteA");
            var serviceID = await response.Content.ReadAsStringAsync();

            user.AuthenticateUser("client@prestar.pt", "123456");

            //Aceder à Criação - GET
            response = await _client.GetAsync("/ServiceRequisitions/Create/" + serviceID);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            //Criar um Serviço - POST
            postRequest = new HttpRequestMessage(HttpMethod.Post, "/ServiceRequisitions/Create/" + serviceID);
            service = new Dictionary<string, string>
            {
                { "ServiceID", serviceID },
                { "ServiceRequisitionStatus", ServiceRequisitionStatus.Pending.ToString() },
                { "AdditionalRequestInfo", "Limpeza de Prédio - 2x por semana" }
            };
            postRequest.Content = new FormUrlEncodedContent(service);
            response = await _client.SendAsync(postRequest);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            response = await _client.GetAsync("/ServiceRequisitions/GetIdByServiceID?serviceID=" + serviceID);
            var id = await response.Content.ReadAsStringAsync();

            //Aceder à Conclusão - GET
            response = await _client.GetAsync("/ServiceRequisitions/ConcludeService/" + id);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            
            //Criar um Comentário e Avaliação - POST
            postRequest = new HttpRequestMessage(HttpMethod.Post, "/CommentAndEvaluations/Create");
            var comment_eval = new Dictionary<string, string>
            {
                { "ServiceID", serviceID },
                { "UserCommentingID", "34e604e0-c465-4611-b06c-aee4ffe7efc1" },
                { "Evaluation", "4" },
                { "Comment", "Gostei imenso do serviço" }
            };
            postRequest.Content = new FormUrlEncodedContent(comment_eval);
            response = await _client.SendAsync(postRequest);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            //Aceder aos Detalhes de uma Requisições - GET
            response = await _client.GetAsync("/ServiceRequisitions/Details/" + id);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            //GetPoints Final
            user.AuthenticateUser("provider@prestar.pt", "123456");
            response = await _client.GetAsync("/CommentAndEvaluations/GetPoints");
            int finalPoints = Int32.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.True(finalPoints > atualPoints);

            //Eliminar um Serviço - POST
            postRequest = new HttpRequestMessage(HttpMethod.Post, "/Services/Delete/" + serviceID);
            response = await _client.SendAsync(postRequest);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            //Eliminar categorias - POST
            postRequest = new HttpRequestMessage(HttpMethod.Post, "/ServiceCategories/DeleteCategoriesToTest?c=" + category_id + "&s=" + subcategory_id);
            response = await _client.SendAsync(postRequest);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}