using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace IntegrationTest
{
    class Authentication
    {
        protected static string AUTHENTICATION_COOKIE = ".AspNetCore.Identity.";

        protected SetCookieHeaderValue _antiforgeryCookie;
        protected string _antiforgeryToken;
        protected HttpClient _client;

        public Authentication(HttpClient client)
        {
            _client = client;
        }

        protected static Regex AntiforgeryFormFieldRegex = new Regex(@"\<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" \/\>");

        protected async Task<string> EnsureAntiforgeryToken()
        {
            if (_antiforgeryToken != null) return _antiforgeryToken;

            var response = await _client.GetAsync("/Identity/Account/Login");
            response.EnsureSuccessStatusCode();
            if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string> values))
            {
                _antiforgeryCookie = SetCookieHeaderValue.ParseList(values.ToList()).SingleOrDefault(c => c.Name.StartsWith(".AspNetCore.AntiForgery.", StringComparison.InvariantCultureIgnoreCase));
            }
            Assert.NotNull(_antiforgeryCookie);
            _client.DefaultRequestHeaders.Add("Cookie", new CookieHeaderValue(_antiforgeryCookie.Name, _antiforgeryCookie.Value).ToString());

            var responseHtml = await response.Content.ReadAsStringAsync();
            var match = AntiforgeryFormFieldRegex.Match(responseHtml);
            _antiforgeryToken = match.Success ? match.Groups[1].Captures[0].Value : null;
            Assert.NotNull(_antiforgeryToken);

            return _antiforgeryToken;
        }

        protected SetCookieHeaderValue _authenticationCookie;

        protected async Task<Dictionary<string, string>> EnsureAntiforgeryTokenForm(Dictionary<string, string> formData = null)
        {
            if (formData == null) formData = new Dictionary<string, string>();

            formData.Add("__RequestVerificationToken", await EnsureAntiforgeryToken());
            return formData;
        }

        private string _email;
        private string _password;
        public void AuthenticateUser(string email, string password)
        {
            _email = email;
            _password = password;
            EnsureAuthenticationCookie().Wait();
        }

        public async Task EnsureAuthenticationCookie()
        {
            if (_authenticationCookie != null) return;
            var formData = await EnsureAntiforgeryTokenForm(new Dictionary<string, string>
            {
                { "Email", _email },
                { "Password", _password }
            });
            var response = await _client.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(formData));
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string> values))
            {
                _authenticationCookie = SetCookieHeaderValue.ParseList(values.ToList()).SingleOrDefault(c => c.Name.StartsWith(AUTHENTICATION_COOKIE, StringComparison.InvariantCultureIgnoreCase));
            }
            Assert.NotNull(_authenticationCookie);
            _client.DefaultRequestHeaders.Add("Cookie", new CookieHeaderValue(_authenticationCookie.Name, _authenticationCookie.Value).ToString());

            // The current pair of antiforgery cookie-token is not valid anymore
            // Since the tokens are generated based on the authenticated user!
            // We need a new token after authentication (The cookie can stay the same)
            _antiforgeryToken = null;
        }
    }
}
