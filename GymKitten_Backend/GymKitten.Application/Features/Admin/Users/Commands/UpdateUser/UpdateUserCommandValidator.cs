using FluentValidation;

namespace GymKitten.Application.Features.Admin.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Role)
            .Must(r => string.IsNullOrEmpty(r) ||
                       string.Equals(r, "Customer", System.StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(r, "Admin", System.StringComparison.OrdinalIgnoreCase))
            .WithMessage("Role must be 'Customer' or 'Admin'.");

        RuleFor(x => x.FullName)
            .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.");
    }
}
