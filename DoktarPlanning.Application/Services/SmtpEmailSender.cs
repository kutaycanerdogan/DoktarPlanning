using DoktarPlanning.Infrastructure.Interfaces;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System.Net;
using System.Net.Mail;

namespace DoktarPlanning.Application.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            var host = _config["Smtp:Host"];
            var port = int.Parse(_config["Smtp:Port"] ?? "25");
            var user = _config["Smtp:User"];
            var pass = _config["Smtp:Password"];
            var from = _config["Smtp:From"];
            var enableSsl = bool.Parse(_config["Smtp:EnableSsl"] ?? "false");

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(user, pass)
            };

            var msg = new MailMessage(from, to, subject, htmlBody) { IsBodyHtml = true };
            await client.SendMailAsync(msg, cancellationToken);
            _logger.LogInformation("Sent email to {To} subject {Subject}", to, subject);
        }
    }
}