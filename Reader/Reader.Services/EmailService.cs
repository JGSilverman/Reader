using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Reader.Services
{
    public class EmailService : IEmailService
    {
        ILogger<EmailService> _logger;
        private readonly IConfiguration _config;
        private readonly string _apiKey;
        private readonly string _apiSecret;
        public EmailService(ILogger<EmailService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            _apiKey = _config.GetSection("MailJetSettings").GetValue<string>("ApiKey");
            _apiSecret = _config.GetSection("MailJetSettings").GetValue<string>("ApiSecret");
        }

        public async Task SendEmailAsync(string fromEmail, string toEmail, string subject, string message)
        {
            try
            {
                var response = await SendMailJetEmail(fromEmail, toEmail, subject, message);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(response.GetErrorMessage());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private async Task<MailjetResponse> SendMailJetEmail(string fromEmail, string toEmail, string subject, string message)
        {
            MailjetClient client = new MailjetClient(_apiKey, _apiSecret);

            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
            .Property(Send.FromEmail, fromEmail)
            .Property(Send.FromName, fromEmail)
            .Property(Send.Subject, subject)
            .Property(Send.HtmlPart, message)
            .Property(Send.Recipients, new JArray {
                new JObject {
                    {"Email", toEmail}
                }
             });

            return await client.PostAsync(request);
        }
    }
}
