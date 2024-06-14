using Bouchonnois.Domain.Terminer;
using Domain.Core;
using LanguageExt;

namespace Bouchonnois.Domain;

public sealed partial class PartieDeChasse
{
    public Either<Error, string> Terminer()
    {
        if (DéjàTerminée())
        {
            return Error.AnError("Quand c'est fini, c'est fini");
        }

        var classement = Classement();
        var (winners, nbGalinettes) = TousBrocouilles(classement)
            ? (new List<string> {"Brocouille"}, 0)
            : (classement[0].Map(c => c.Nom), classement[0].First().NbGalinettes);

        RaiseEvent((id, time) => new PartieTerminée(id, time, winners.ToSeq(), nbGalinettes));

        return String.Join(", ", winners);
    }

    private List<IGrouping<int, Chasseur>> Classement()
        => _chasseurs
            .GroupBy(c => c.NbGalinettes)
            .OrderByDescending(g => g.Key)
            .ToList();

    private static bool TousBrocouilles(IEnumerable<IGrouping<int, Chasseur>> classement) =>
        classement.All(group => group.Key == 0);

    [EventSourced]
    private void Apply(PartieTerminée @event) => _status = PartieStatus.Terminée;
}