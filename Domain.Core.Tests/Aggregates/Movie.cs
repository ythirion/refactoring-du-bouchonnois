using LanguageExt;

namespace Domain.Core.Tests.Aggregates;

public class Movie : Aggregate
{
    // public only for testing purpose
    public string? _title;
    public DateTime? _releaseDate;
    public Seq<string> _casting = Seq<string>.Empty;
    private Movie(Guid id, Func<DateTime> timeProvider) : base(timeProvider, true) => Id = id;

    public Movie(Guid id, Func<DateTime> timeProvider, string title, DateTime releaseDate) : this(id, timeProvider)
        => RaiseEvent(new MovieCreated(id, Time(), title, releaseDate));

    [EventSourced]
    private void Apply(MovieCreated @event)
    {
        _title = @event.Title;
        _releaseDate = @event.ReleaseDate;
    }

    public void ChangeCast(Seq<string> casting) => RaiseEvent(new CastingHasChanged(Id, Time(), casting));

    [EventSourced]
    private void ApplyEvent(CastingHasChanged @event) => _casting = @event.Casting;

    public void NotWellImplementedBehavior() => RaiseEvent(new NotWellImplementedDomainBehaviorRaised(Id, Time()));
}

public record MovieCreated(Guid Id, DateTime Date, string Title, DateTime ReleaseDate) : Event(Id, 1, Date);

public record CastingHasChanged(Guid Id, DateTime Date, Seq<string> Casting) : Event(Id, 1, Date);

public record NotWellImplementedDomainBehaviorRaised(Guid Id, DateTime Date) : Event(Id, 1, Date);