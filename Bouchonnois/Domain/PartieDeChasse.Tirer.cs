using Bouchonnois.Domain.Tirer;
using Domain.Core;
using LanguageExt;

namespace Bouchonnois.Domain;

public sealed partial class PartieDeChasse
{
    public Either<Error, Unit> Tirer(string chasseur)
        => Tirer(chasseur,
            intention: "tire",
            _ => RaiseEvent((id, time) => new ChasseurATiré(id, time, chasseur)));

    [EventSourced]
    private void Apply(ChasseurATiré @event) => RetrieveChasseur(@event.Chasseur).ATiré();
}