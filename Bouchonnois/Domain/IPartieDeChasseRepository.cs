using Domain.Core;
using LanguageExt;

namespace Bouchonnois.Domain
{
    public interface IPartieDeChasseRepository
    {
        Task Save(PartieDeChasse partieDeChasse);
        OptionAsync<PartieDeChasse> GetById(Guid partieDeChasseId);
        OptionAsync<Seq<IEvent>> EventsFor(Guid partieDeChasseId);
    }
}