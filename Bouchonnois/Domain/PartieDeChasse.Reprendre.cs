using Bouchonnois.Domain.Reprendre;
using Domain.Core;
using LanguageExt;

namespace Bouchonnois.Domain;

public sealed partial class PartieDeChasse
{
    public Either<Error, Unit> Reprendre()
    {
        if (DéjàEnCours())
        {
            return Error.AnError("La partie de chasse est déjà en cours");
        }

        if (DéjàTerminée())
        {
            return Error.AnError("La partie de chasse est déjà terminée");
        }

        RaiseEvent((id, time) => new PartieReprise(id, time));

        return Unit.Default;
    }

    [EventSourced]
    private void Apply(PartieReprise @event) => _status = PartieStatus.EnCours;
}