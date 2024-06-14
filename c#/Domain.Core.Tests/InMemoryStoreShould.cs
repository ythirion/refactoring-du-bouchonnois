using Domain.Core.Tests.Aggregates;
using FluentAssertions;
using FluentAssertions.LanguageExt;
using static Domain.Core.Tests.Data.Cinemas;
using static Domain.Core.Tests.Data.Movies;

namespace Domain.Core.Tests
{
    public class InMemoryStoreShould
    {
        private readonly InMemoryEventStore _eventStore;

        public InMemoryStoreShould() => _eventStore = new InMemoryEventStore(Data.TimeProvider);

        [Fact]
        public async Task store_in_an_empty_store()
        {
            var id = Guid.NewGuid();
            var movie = Oppenheimer.Movie(id);

            movie.UncommittedEvents().Should().HaveCount(1);

            await _eventStore.Save(movie);

            movie.UncommittedEvents().Should().HaveCount(0);
            _eventStore
                .GetEventsById<Movie>(id)
                .Should()
                .BeSome(events =>
                {
                    events.Should().HaveCount(1);
                    events.Single().Should()
                        .Be(new MovieCreated(id, Data.Now, Oppenheimer.Title, Oppenheimer.ReleaseDate));
                });
        }

        [Fact]
        public async Task retrieve_aggregate_in_a_store_containing_other_aggregates()
        {
            var latestScorseseId = Guid.NewGuid();

            await _eventStore.Save(Oppenheimer.Movie(Guid.NewGuid()));
            await _eventStore.Save(Grand_Rex.Cinema(Guid.NewGuid()));
            await _eventStore.Save(Killers_of_the_Flower_Moon.Movie(latestScorseseId));

            _eventStore
                .GetById<Movie>(latestScorseseId)
                .Should()
                .BeSome(movie =>
                {
                    movie.Id.Should().Be(latestScorseseId);
                    movie.Version.Should().Be(1);
                    movie._title.Should().Be(Killers_of_the_Flower_Moon.Title);
                    movie._releaseDate.Should().Be(Killers_of_the_Flower_Moon.ReleaseDate);
                    (movie as IAggregate).GetUncommittedEvents().Should().BeEmpty();
                });
        }
    }
}