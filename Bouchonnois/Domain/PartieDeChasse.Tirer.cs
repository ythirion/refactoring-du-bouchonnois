using Bouchonnois.Domain.Tirer;
using LanguageExt;

namespace Bouchonnois.Domain;

public sealed partial class PartieDeChasse
{
    public Either<Error, Unit> Tirer(string chasseur)
        => Tirer(chasseur,
            intention: "tire",
            _ => RaiseEvent((id, time) => new ChasseurATiré(id, time, chasseur)));

    private void Apply(ChasseurATiré @event) => RetrieveChasseur(@event.Chasseur).ATiré();
}