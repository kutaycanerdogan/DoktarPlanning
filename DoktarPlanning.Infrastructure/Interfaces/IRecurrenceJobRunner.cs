using System;
using System.Collections.Generic;
using System.Text;

namespace DoktarPlanning.Infrastructure.Interfaces
{
    public interface IRecurrenceJobRunner
    {
        public Task RunAsync(Guid userId, Guid ruleId, CancellationToken cancellationToken = default);
    }
}
