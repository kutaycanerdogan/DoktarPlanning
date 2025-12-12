using DoktarPlanning.Api.ViewModels;
using DoktarPlanning.Domain.Common;
using DoktarPlanning.Infrastructure.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoktarPlanning.Api.Controllers
{
    [Authorize]
    public class RemindersController : ApiControllerBase
    {
        private readonly IReminderService _reminder;

        public RemindersController(IReminderService reminder)
        {
            _reminder = reminder;
        }

        [HttpPost("tasks/{taskId:guid}/reminder")]
        public async Task<IActionResult> Schedule(Guid taskId, [FromBody] ScheduleRequest req, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = GetUserId();
            await _reminder.ScheduleReminderAsync(userId, taskId, req.RemindAt, req.Channel, req.ChannelTarget, cancellationToken);
            return NoContent();
        }

        [HttpDelete("tasks/{taskId:guid}/reminder")]
        public async Task<IActionResult> Cancel(Guid taskId, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            await _reminder.CancelReminderAsync(userId, taskId, cancellationToken);
            return NoContent();
        }

        [HttpPost("tasks/{taskId:guid}/reminder/send")]
        public async Task<IActionResult> SendNow(Guid taskId, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            await _reminder.SendReminderNowAsync(userId, taskId, cancellationToken);
            return NoContent();
        }
    }
}