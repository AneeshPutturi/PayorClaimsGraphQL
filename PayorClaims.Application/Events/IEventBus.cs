namespace PayorClaims.Application.Events;

public interface IEventBus
{
    void Publish<T>(T evt) where T : class;
    IObservable<T> Subscribe<T>() where T : class;
}
