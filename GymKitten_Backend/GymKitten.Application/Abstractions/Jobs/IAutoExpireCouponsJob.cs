namespace GymKitten.Application.Abstractions.Jobs;

public interface IAutoExpireCouponsJob
{
    void ScheduleAutoExpireCoupons();
    Task ExecuteAsync();
}
