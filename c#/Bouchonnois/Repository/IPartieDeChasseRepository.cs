using Bouchonnois.Domain;

namespace Bouchonnois.Repository
{
    public interface IPartieDeChasseRepository
    {
        void Save(PartieDeChasse partieDeChasse);
        PartieDeChasse GetById(Guid partieDeChasseId);
    }
}