using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;


namespace DoktarPlanning.Data.Interceptors
{
    public class LoggingSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly ILogger<LoggingSaveChangesInterceptor> _logger;

        public LoggingSaveChangesInterceptor(ILogger<LoggingSaveChangesInterceptor> logger)
        {
            _logger = logger;
        }

        public override void SaveChangesFailed(DbContextErrorEventData eventData)
        {
            Console.WriteLine("*********************************************************************");
            _logger.LogError(eventData.Exception, "SaveChanges failed");
            Console.WriteLine("eventData", eventData);
            Console.WriteLine("*********************************************************************");
            base.SaveChangesFailed(eventData);
        }
        public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken)
        {
            Console.WriteLine("*********************************************************************");
            _logger.LogError(eventData.Exception, "SaveChangesAsync failed");
            Console.WriteLine("eventData", eventData);
            Console.WriteLine("*********************************************************************");
            await base.SaveChangesFailedAsync(eventData, cancellationToken);
        }
    }
}
