using Bouchonnois.Domain;
using LanguageExt;

namespace Bouchonnois.Tests.Doubles
{
    public class PartieDeChasseRepositoryForTests : IPartieDeChasseRepository
    {
        private Map<Guid, PartieDeChasse> _partiesDeChasse = Map<Guid, PartieDeChasse>.Empty;
        private PartieDeChasse? _savedPartieDeChasse;

        public void Save(PartieDeChasse partieDeChasse)
        {
            _savedPartieDeChasse = partieDeChasse;
            Add(partieDeChasse);
        }

        public Option<PartieDeChasse> GetById(Guid partieDeChasseId) => _partiesDeChasse.Find(partieDeChasseId);

        public void Add(PartieDeChasse partieDeChasse) =>
            _partiesDeChasse = _partiesDeChasse.AddOrUpdate(partieDeChasse.Id, partieDeChasse);

        public PartieDeChasse? SavedPartieDeChasse() => _savedPartieDeChasse;
    }
}