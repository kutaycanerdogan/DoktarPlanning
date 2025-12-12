using DoktarPlanning.Infrastructure.DTOs;
using DoktarPlanning.Infrastructure.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoktarPlanning.Api.Controllers
{
    [Authorize]
    public class RecurrenceController : ApiControllerBase
    {
        private readonly IRecurrenceService _recurrence;

        public RecurrenceController(IRecurrenceService recurrence)
        {
            _recurrence = recurrence;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RecurrenceDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = GetUserId();
            var created = await _recurrence.CreateRuleAsync(userId, dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var list = await _recurrence.ListRulesAsync(userId, cancellationToken);
            var item = System.Linq.Enumerable.FirstOrDefault(list, r => r.Id == id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet]
        public async Task<IActionResult> List(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var list = await _recurrence.ListRulesAsync(userId, cancellationToken);
            return Ok(list);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RecurrenceDto dto, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var updated = await _recurrence.UpdateRuleAsync(userId, id, dto, cancellationToken);
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            await _recurrence.DeleteRuleAsync(userId, id, cancellationToken);
            return NoContent();
        }

        [HttpGet("{id:guid}/instances")]
        public async Task<IActionResult> GenerateInstances(Guid id, [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var instances = await _recurrence.GenerateInstancesAsync(userId, id, from, to, cancellationToken);
            return Ok(instances);
        }
    }
}