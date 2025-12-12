using DoktarPlanning.Api.Extensions;
using DoktarPlanning.Infrastructure.DTOs;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoktarPlanning.Api.Controllers
{
    [Authorize]
    public class TasksController : ApiControllerBase
    {
        private readonly ITaskService _tasks;

        public TasksController(ITaskService tasks)
        {
            _tasks = tasks;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaskDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = GetUserId();
            var created = await _tasks.CreateAsync(userId, dto, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TaskDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = GetUserId();
            var updated = await _tasks.UpdateAsync(userId, id, dto, cancellationToken);
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            await _tasks.DeleteAsync(userId, id, cancellationToken);
            return NoContent();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var dto = await _tasks.GetByIdAsync(userId, id, cancellationToken);
            return Ok(dto);
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] TaskQueryParameters query, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var list = await _tasks.ListAsync(userId, query, cancellationToken);
            return Ok(list);
        }

        [HttpPut("{id:guid}/complete")]
        public async Task<IActionResult> MarkComplete(Guid id, [FromQuery] bool isComplete = true, CancellationToken cancellationToken = default)
        {
            var userId = GetUserId();
            await _tasks.MarkCompleteAsync(userId, id, isComplete, cancellationToken);
            return NoContent();
        }

        [HttpGet("day")]
        public async Task<IActionResult> ForDay([FromQuery] DateTime? day, CancellationToken cancellationToken)
        {
            var userId = GetUserId();

            if (day is null)
            {
                day = DateTime.Now;
            }
            var list = await _tasks.GetForDayAsync(userId, (DateTime)day, cancellationToken);
            return Ok(list);
        }

        [HttpGet("summary/today")]
        public async Task<IActionResult> GetTodaySummary(CancellationToken ct)
        {
            var userId = User.GetUserId();
            var today = DateTime.Now.Date;

            var tasks = await _tasks.ListAsync(
                userId,
                new TaskQueryParameters
                {
                    From = today,
                    To = today.AddDays(1).AddTicks(-1),
                    Priority = 0,
                    IsCompleted = null,
                },
                ct);

            return Ok(tasks);
        }
    }
}