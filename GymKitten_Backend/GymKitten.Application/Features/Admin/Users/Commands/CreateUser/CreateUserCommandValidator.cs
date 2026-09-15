using FluentValidation;

namespace GymKitten.Application.Features.Admin.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(r => string.Equals(r, "Customer", System.StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(r, "Admin", System.StringComparison.OrdinalIgnoreCase))
            .WithMessage("Role must be 'Customer' or 'Admin'.");

        RuleFor(x => x.FullName)
            .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.");
    }
}
