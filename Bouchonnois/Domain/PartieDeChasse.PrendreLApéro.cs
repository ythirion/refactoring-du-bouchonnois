using Bouchonnois.Domain.Apéro;
using LanguageExt;

namespace Bouchonnois.Domain;

public sealed partial class PartieDeChasse
{
    public Either<Error, Unit> PrendreLapéro()
    {
        if (DuringApéro())
        {
            return Error.AnError("On est déjà en plein apéro");
        }

        if (DéjàTerminée())
        {
            return Error.AnError("La partie de chasse est déjà terminée");
        }

        RaiseEvent((id, time) => new ApéroDémarré(id, time));

        return Unit.Default;
    }

    private void Apply(ApéroDémarré @event) => _status = PartieStatus.Apéro;
}