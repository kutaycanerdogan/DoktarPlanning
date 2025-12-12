using DoktarPlanning.Infrastructure.DTOs;

using FluentValidation;

namespace DoktarPlanning.Application.Validation
{
    public class RecurrenceDtoValidator : AbstractValidator<RecurrenceDto>
    {
        public RecurrenceDtoValidator()
        {
            RuleFor(x => x.Interval).GreaterThan(0);
            RuleFor(x => x.Frequency).IsInEnum();
        }
    }
}