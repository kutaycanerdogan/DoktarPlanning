namespace DoktarPlanning.Infrastructure.Interfaces
{
    public interface IWebhookSender
    {
        Task SendAsync(string url, object payload, CancellationToken cancellationToken = default);
    }
}
