using FluentValidation;
using Metro.Application.Users.Commands;
using Metro.Domain.Auth;
using Metro.Domain.Users;

namespace Metro.Application.Users.Validators
{
    public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(command => command.Name)
                .MaximumLength(200).WithMessage("Name must be at most 200 characters.");

            RuleFor(command => command.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email is invalid.")
                .MaximumLength(256).WithMessage("Email must be at most 256 characters.");

            RuleFor(command => command.Document)
                .MaximumLength(50).WithMessage("Document must be at most 50 characters.");

            RuleFor(command => command.Phone)
                .MaximumLength(30).WithMessage("Phone must be at most 30 characters.");

            RuleFor(command => command.Password)
                .NotEmpty().WithMessage("Password is required.")
                .Must(PasswordPolicy.IsValid).WithMessage(PasswordPolicy.RequirementsMessage);

            RuleFor(command => command.Role)
                .Must(UserRoles.IsValid)
                .WithMessage(UserRoles.ValidationMessage);
        }
    }
}
