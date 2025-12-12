using DoktarPlanning.Infrastructure.DTOs;

using FluentValidation;

namespace DoktarPlanning.Application.Validation
{
    public class SubTaskDtoValidator : AbstractValidator<SubTaskDto>
    {
        public SubTaskDtoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        }
    }
}