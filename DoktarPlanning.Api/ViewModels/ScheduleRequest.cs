using DoktarPlanning.Domain.Common;

namespace DoktarPlanning.Api.ViewModels
{
    public class ScheduleRequest
    {
        public DateTime RemindAt { get; set; }
        public ReminderChannel Channel { get; set; }
        public string? ChannelTarget { get; set; }
    }
}
