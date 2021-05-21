using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MimeKit.Text;
using MimeKit.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Prestar.Services
{
    /// <summary>
    /// This class represents an email sender that inherits from the IEmailSender interface.
    /// </summary>
    /// <see cref="IEmailSender"/>
    public class EmailSender : IEmailSender
    {
        // Our private configuration variables
        private string host;
        private int port;
        private bool enableSSL;
        private string userName;
        private string password;

        /// <summary>
        /// Constructor of EmailSender, where we get the parameterized configuration.
        /// </summary>
        /// <param name="host">Email host</param>
        /// <param name="port">Email port</param>
        /// <param name="enableSSL">Check to enableSSL</param>
        /// <param name="userName">User username</param>
        /// <param name="password">User password</param>
        public EmailSender(string host, int port, bool enableSSL, string userName, string password)
        {
            this.host = host;
            this.port = port;
            this.enableSSL = enableSSL;
            this.userName = userName;
            this.password = password;
        }

        /// <summary>
        /// Sends an email assynchronous.
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="subject">Email subject</param>
        /// <param name="htmlMessage">Email content</param>
        /// <returns>Returns a Task</returns>
        /// <see cref="Task"/>
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Equipa Prestar", "prestar@outlook.pt"));
            message.To.Add(new MailboxAddress(userName, email));
            message.Subject = subject;
            var builder = new BodyBuilder();

            // Set the plain-text version of the message text
            builder.TextBody = @"Este email foi gerado automaticamente, por favor não responda para este endereço.
                        
                    " + htmlMessage;

            // add the logo to the email
            var prestarLogo = builder.LinkedResources.Add(@"./wwwroot/images/logo-nome.png");
            prestarLogo.ContentId = MimeUtils.GenerateMessageId();
    


            // Set the html version of the message text
            builder.HtmlBody = string.Format(@"<p>Este email foi gerado automaticamente, por favor não responda para este endereço.<br><br>
            <p> "+ htmlMessage + @"
            <br><br><br> 
            A equipa o Nosso Bairro Nossa Cidade e a Câmara Municipal de Setúbal agradecem o seu apoio nesta iniciativa!
            <br><br><br>           
            <center><img src=""cid:{0}""></center>", prestarLogo.ContentId);

            // Now we just need to set the message body and we're done
            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await client.ConnectAsync(host, port, enableSSL);

                // Note: only needed if the SMTP server requires authentication
                await client.AuthenticateAsync(userName, password);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}



