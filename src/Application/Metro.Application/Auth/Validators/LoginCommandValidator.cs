using FluentValidation;
using Metro.Application.Auth.Commands;

namespace Metro.Application.Auth.Validators
{
    public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(command => command.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email is invalid.");

            RuleFor(command => command.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
