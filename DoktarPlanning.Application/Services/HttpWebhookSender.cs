using DoktarPlanning.Infrastructure.Interfaces;

using Microsoft.Extensions.Logging;

using System.Text;
using System.Text.Json;

namespace DoktarPlanning.Application.Services
{
    public class HttpWebhookSender : IWebhookSender
    {
        private readonly HttpClient _http;
        private readonly ILogger<HttpWebhookSender> _logger;

        public HttpWebhookSender(HttpClient http, ILogger<HttpWebhookSender> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task SendAsync(string url, object payload, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _http.PostAsync(url, content, cancellationToken);
            res.EnsureSuccessStatusCode();
            _logger.LogInformation("Webhook posted to {Url}", url);
        }
    }
}