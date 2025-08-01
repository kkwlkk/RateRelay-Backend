namespace RateRelay.Domain.Interfaces;

public interface IHangfireJob
{
    Task ExecuteAsync();
}