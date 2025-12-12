using DoktarPlanning.Infrastructure.DTOs;

using FluentValidation;

namespace DoktarPlanning.Application.Validation
{
    public class TaskDtoValidator : AbstractValidator<TaskDto>
    {
        public TaskDtoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
            RuleFor(x => x.Description).MaximumLength(4000).When(x => x.Description != null);
            RuleFor(x => x.StartAt).LessThanOrEqualTo(x => x.EndAt).When(x => x.StartAt.HasValue && x.EndAt.HasValue);
            RuleFor(x => x.DueAt).GreaterThanOrEqualTo(DateTime.Now.AddYears(-1)).When(x => x.DueAt.HasValue);
            RuleFor(x => x.Priority).IsInEnum();
        }
    }
}