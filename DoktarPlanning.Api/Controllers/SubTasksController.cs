using DoktarPlanning.Infrastructure.DTOs;
using DoktarPlanning.Infrastructure.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoktarPlanning.Api.Controllers
{
    [Authorize]
    public class SubTasksController : ApiControllerBase
    {
        private readonly ISubTaskService _subTasks;

        public SubTasksController(ISubTaskService subTasks)
        {
            _subTasks = subTasks;
        }

        [HttpPost("tasks/{taskId:guid}/subtasks")]
        public async Task<IActionResult> Create(Guid taskId, [FromBody] SubTaskDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = GetUserId();
            var created = await _subTasks.CreateAsync(userId, taskId, dto, cancellationToken);
            return CreatedAtAction(nameof(GetByTask), new { taskId }, created);
        }

        [HttpGet("tasks/{taskId:guid}/subtasks")]
        public async Task<IActionResult> GetByTask(Guid taskId, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var list = await _subTasks.ListByTaskAsync(userId, taskId, cancellationToken);
            return Ok(list);
        }

        [HttpPut("tasks/{taskId:guid}/subtasks/{subTaskId:guid}")]
        public async Task<IActionResult> Update(Guid taskId, Guid subTaskId, [FromBody] SubTaskDto dto, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var updated = await _subTasks.UpdateAsync(userId, taskId, subTaskId, dto, cancellationToken);
            return Ok(updated);
        }

        [HttpDelete("tasks/{taskId:guid}/subtasks/{subTaskId:guid}")]
        public async Task<IActionResult> Delete(Guid taskId, Guid subTaskId, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            await _subTasks.DeleteAsync(userId, taskId, subTaskId, cancellationToken);
            return NoContent();
        }

        [HttpPost("tasks/{taskId:guid}/subtasks/{subTaskId:guid}/complete")]
        public async Task<IActionResult> MarkComplete(Guid taskId, Guid subTaskId, [FromQuery] bool complete = true, CancellationToken cancellationToken = default)
        {
            var userId = GetUserId();
            await _subTasks.MarkCompleteAsync(userId, taskId, subTaskId, complete, cancellationToken);
            return NoContent();
        }
    }
}