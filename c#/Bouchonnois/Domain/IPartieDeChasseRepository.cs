using LanguageExt;

namespace Bouchonnois.Domain
{
    public interface IPartieDeChasseRepository
    {
        void Save(PartieDeChasse partieDeChasse);
        Option<PartieDeChasse> GetById(Guid partieDeChasseId);
    }
}