using Bouchonnois.Domain.Tirer;
using LanguageExt;

namespace Bouchonnois.Domain;

public sealed partial class PartieDeChasse
{
    public Either<Error, Unit> TirerSurUneGalinette(string chasseur)
        => _terrain is {NbGalinettes: 0}
            ? RaiseEventAndReturnAnError((id, time) => new ChasseurACruTiréSurGalinette(id, time, chasseur))
            : Tirer(chasseur,
                intention: "veut tirer sur une galinette",
                c => RaiseEvent((id, time) => new ChasseurATiréSurUneGalinette(id, time, chasseur)));

    private void Apply(ChasseurATiréSurUneGalinette @event)
    {
        var chasseur = RetrieveChasseur(@event.Chasseur);
        chasseur.ATiré();
        chasseur.ATué();
        _terrain!.UneGalinetteEnMoins();
    }
}