using DoktarPlanning.Infrastructure.DTOs;

using FluentValidation;

namespace DoktarPlanning.Application.Validation
{
    public class UserDtoValidator : AbstractValidator<UserDto>
    {
        public UserDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.DisplayName).MaximumLength(200).When(x => x.DisplayName != null);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        }
    }
}