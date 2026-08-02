using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Application.Features.Auth.Register;

public sealed class RegisterCustomerCommandHandler
    : ICommandHandler<RegisterCustomerCommand, Result<RegisterCustomerResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IOtpGenerator _otpGenerator;
    private readonly IRedisOtpStore _otpStore;

    public RegisterCustomerCommandHandler(
        IApplicationDbContext dbContext,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IOtpGenerator otpGenerator,
        IRedisOtpStore otpStore)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _otpGenerator = otpGenerator;
        _otpStore = otpStore;
    }

    public async Task<Result<RegisterCustomerResponse>> Handle(
        RegisterCustomerCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Check if email already exists
        var emailExists = await _dbContext.Users
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
        {
            return Result.Failure<RegisterCustomerResponse>(AuthErrors.EmailAlreadyExists);
        }

        // 2. Create User entity
        var now = DateTime.UtcNow;
        var user = new User
        {
            Userid = Guid.NewGuid(),
            Email = request.Email,
            Passwordhash = _passwordHasher.Hash(request.Password),
            Fullname = request.FullName,
            Role = "Customer",
            Isemailverified = false,
            Isactive = true,
            Createdat = now,
            Updatedat = now
        };

        _dbContext.Users.Add(user);

        // 3. Generate OTP and store it
        var otpCode = _otpGenerator.Generate6Digits();
        await _otpStore.StoreAsync(
            user.Userid,
            otpCode,
            TimeSpan.FromMinutes(5),
            cancellationToken);

        // 4. Raise domain event (will be dispatched during SaveChangesAsync)
        user.RaiseDomainEvent(new CustomerRegisteredDomainEvent(
            user.Userid,
            user.Email,
            otpCode));

        // 5. Save to DB (domain events are published here)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Return response
        var response = new RegisterCustomerResponse(
            user.Userid,
            user.Email,
            user.Fullname!,
            user.Role,
            user.Createdat);

        return Result.Success(response);
    }
}
