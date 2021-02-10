using System;
using System.Threading.Tasks;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Rocky.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public MailJetSettings _mailJetSettings { get; set; }

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(email, subject, htmlMessage);
        }

        public async Task<MailjetResponse> Execute(string email, string subject, string body)
        {
            _mailJetSettings = _configuration.GetSection("MailJet").Get<MailJetSettings>();

            var client = new MailjetClient(_mailJetSettings.apiKey, _mailJetSettings.secretKey);

            var request = new MailjetRequest { Resource = Send.Resource }
                .Property(Send.FromEmail, _mailJetSettings.fromEmail)
                .Property(Send.FromName, _mailJetSettings.fromName)
                .Property(Send.Subject, subject)
                .Property(Send.HtmlPart, body)
                .Property(Send.Recipients, new JArray
                {
                    new JObject
                    {
                        { "Email", email }
                     }
                });

                return await client.PostAsync(request);

        }
    }
}

/*


 
 
 */