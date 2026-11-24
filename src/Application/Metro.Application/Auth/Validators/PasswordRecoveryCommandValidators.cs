using FluentValidation;
using Metro.Application.Auth.Commands;
using Metro.Domain.Auth;

namespace Metro.Application.Auth.Validators
{
    public sealed class RequestPasswordRecoveryCommandValidator : AbstractValidator<RequestPasswordRecoveryCommand>
    {
        public RequestPasswordRecoveryCommandValidator()
        {
            RuleFor(command => command.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email is invalid.");
        }
    }

    public sealed class VerifyPasswordRecoveryCodeCommandValidator : AbstractValidator<VerifyPasswordRecoveryCodeCommand>
    {
        public VerifyPasswordRecoveryCodeCommandValidator()
        {
            RuleFor(command => command.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email is invalid.");

            RuleFor(command => command.Code)
                .NotEmpty().WithMessage("Recovery code is required.")
                .Length(6).WithMessage("Recovery code must be 6 digits.");
        }
    }

    public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(command => command.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email is invalid.");

            RuleFor(command => command.Code)
                .NotEmpty().WithMessage("Recovery code is required.")
                .Length(6).WithMessage("Recovery code must be 6 digits.");

            RuleFor(command => command.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .Must(PasswordPolicy.IsValid).WithMessage(PasswordPolicy.RequirementsMessage);
        }
    }
}
