using LanguageExt;

namespace Bouchonnois.Domain
{
    public interface IPartieDeChasseRepository
    {
        void Save(PartieDeChasse partieDeChasse);
        PartieDeChasse GetById(Guid partieDeChasseId);
        Option<PartieDeChasse> GetByIdOption(Guid partieDeChasseId);
    }
}