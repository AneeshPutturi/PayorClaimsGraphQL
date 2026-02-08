using System.Collections.Concurrent;
using System.Reactive.Subjects;
using PayorClaims.Application.Events;

namespace PayorClaims.Infrastructure.Events;

public class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, object> _subjects = new();

    private Subject<T> GetOrAddSubject<T>() where T : class
    {
        var key = typeof(T);
        return (Subject<T>)_subjects.GetOrAdd(key, _ => new Subject<T>());
    }

    public void Publish<T>(T evt) where T : class
    {
        GetOrAddSubject<T>().OnNext(evt);
    }

    public IObservable<T> Subscribe<T>() where T : class
    {
        return GetOrAddSubject<T>();
    }
}
