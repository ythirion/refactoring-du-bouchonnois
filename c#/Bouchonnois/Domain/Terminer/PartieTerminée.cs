using LanguageExt;
using static System.String;

namespace Bouchonnois.Domain.Terminer
{
    public record PartieTerminée(Guid Id, DateTime Date, Seq<string> Winners, int NbGalinettes)
        : global::Domain.Core.Event(Id, 1, Date)
    {
        public PartieTerminée(Guid id, DateTime date, string winner, int nbGalinettes)
            : this(id, date, new Seq<string>().Add(winner), nbGalinettes)
        {
        }

        public PartieTerminée(Guid id, DateTime date, int nbGalinettes, params string[] winners)
            : this(id, date, winners.ToSeq(), nbGalinettes)
        {
        }

        public override string ToString() =>
            $"La partie de chasse est terminée, vainqueur : {Join(", ", Winners)} - {NbGalinettes} galinettes";
    }
}