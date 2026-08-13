namespace GymKitten.Application.Abstractions.Jobs;

public interface IOrderAutoCancelService
{
    void ScheduleAutoCancel(Guid orderId, TimeSpan delay);
}
