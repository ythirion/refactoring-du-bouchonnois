using Bouchonnois.Domain;
using Domain.Core;
using LanguageExt;

namespace Bouchonnois.Tests.Doubles
{
    public class PartieDeChasseRepositoryForTests : IPartieDeChasseRepository
    {
        private readonly IEventStore _eventStore;
        private PartieDeChasse? _savedPartieDeChasse;
        private Seq<IEvent> _emittedEvents;

        public PartieDeChasseRepositoryForTests(IEventStore eventStore)
            => _eventStore = eventStore;

        public Task Save(PartieDeChasse partieDeChasse)
        {
            _emittedEvents = ((IAggregate) partieDeChasse).GetUncommittedEvents().ToSeq();
            _savedPartieDeChasse = partieDeChasse;

            return _eventStore.Save(partieDeChasse);
        }

        public OptionAsync<PartieDeChasse> GetById(Guid partieDeChasseId) =>
            _eventStore.GetById<PartieDeChasse>(partieDeChasseId);

        public OptionAsync<Seq<IEvent>> EventsFor(Guid partieDeChasseId)
            => _eventStore
                .GetEventsById<PartieDeChasse>(partieDeChasseId)
                .Map(events => events.OrderByDescending(e => e.Date).ToSeq());

        public PartieDeChasse? SavedPartieDeChasse() => _savedPartieDeChasse;

        public IEvent LastEvent() => _emittedEvents.Last;
    }
}