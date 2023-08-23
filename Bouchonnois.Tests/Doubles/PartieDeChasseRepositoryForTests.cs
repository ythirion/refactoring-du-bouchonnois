using Bouchonnois.Domain;
using Domain.Core;
using LanguageExt;

namespace Bouchonnois.Tests.Doubles
{
    public class PartieDeChasseRepositoryForTests : IPartieDeChasseRepository
    {
        private readonly IEventStore _eventStore;
        private Map<Guid, PartieDeChasse> _partiesDeChasse = Map<Guid, PartieDeChasse>.Empty;
        private PartieDeChasse? _savedPartieDeChasse;

        public PartieDeChasseRepositoryForTests(IEventStore eventStore)
            => _eventStore = eventStore;

        public void Save(PartieDeChasse partieDeChasse)
        {
            ((IAggregate) partieDeChasse).GetUncommittedEvents().ToSeq();
            AsyncHelper.RunSync(() => _eventStore.Save(partieDeChasse));

            _savedPartieDeChasse = partieDeChasse;
            Add(partieDeChasse);
        }

        public OptionAsync<Seq<IEvent>> EventsFor(Guid partieDeChasseId)
            => _eventStore
                .GetEventsById<PartieDeChasse>(partieDeChasseId)
                .Map(events => events.OrderByDescending(e => e.Date).ToSeq());

        public Option<PartieDeChasse> GetById(Guid partieDeChasseId) => _partiesDeChasse.Find(partieDeChasseId);

        public void Add(PartieDeChasse partieDeChasse) =>
            _partiesDeChasse = _partiesDeChasse.AddOrUpdate(partieDeChasse.Id, partieDeChasse);

        public PartieDeChasse? SavedPartieDeChasse() => _savedPartieDeChasse;
    }
}