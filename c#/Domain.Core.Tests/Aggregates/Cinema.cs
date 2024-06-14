namespace Domain.Core.Tests.Aggregates;

public class Cinema : Aggregate
{
    private string? _name;
    private string? _city;
    private Cinema(Guid id, Func<DateTime> timeProvider) : base(timeProvider, true) => Id = id;

    public Cinema(Guid id, Func<DateTime> timeProvider, string name, string city) : this(id, timeProvider)
        => RaiseEvent(new CinemaCreated(id, Time(), name, city));

    [EventSourced]
    private void Apply(CinemaCreated @event)
    {
        _name = @event.Name;
        _city = @event.City;
    }
}

public record CinemaCreated(Guid Id, DateTime Date, string Name, string City) : Event(Id, 1, Date);