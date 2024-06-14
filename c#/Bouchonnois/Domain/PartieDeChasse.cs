using Bouchonnois.Domain.Tirer;
using Domain.Core;
using LanguageExt;
using static Bouchonnois.Domain.Error;
using static Bouchonnois.Domain.PartieStatus;

namespace Bouchonnois.Domain
{
    public sealed partial class PartieDeChasse : Aggregate
    {
        private PartieStatus _status;
        private Arr<Chasseur> _chasseurs = Arr<Chasseur>.Empty;
        private Terrain? _terrain;

        private Either<Error, Unit> Tirer(
            string chasseur,
            string intention,
            Action<Chasseur>? continueWith = null)
        {
            if (DuringApéro())
            {
                return RaiseEventAndReturnAnError((id, time) =>
                    new ChasseurAVouluTiréPendantLApéro(id, time, chasseur));
            }

            if (DéjàTerminée())
            {
                return RaiseEventAndReturnAnError((id, time) =>
                    new ChasseurAVouluTiréQuandPartieTerminée(id, time, chasseur));
            }

            if (!ChasseurExiste(chasseur))
            {
                return RaiseEventAndReturnAnError((id, time) => new ChasseurInconnuAVouluTiré(id, time, chasseur));
            }

            var chasseurQuiTire = RetrieveChasseur(chasseur);

            if (!chasseurQuiTire.AEncoreDesBalles())
            {
                return RaiseEventAndReturnAnError((id, time) =>
                    new ChasseurSansBallesAVouluTiré(id, time, chasseur, intention));
            }

            continueWith?.Invoke(chasseurQuiTire);

            return Unit.Default;
        }

        private bool DuringApéro() => _status == PartieStatus.Apéro;
        private bool DéjàTerminée() => _status == Terminée;
        private bool DéjàEnCours() => _status == EnCours;
        private bool ChasseurExiste(string chasseur) => _chasseurs.Exists(c => c.Nom == chasseur);
        private Chasseur RetrieveChasseur(string chasseur) => _chasseurs.ToList().Find(c => c.Nom == chasseur)!;

        private IEvent RaiseEvent(Func<Guid, DateTime, IEvent> eventFactory)
            => eventFactory(Id, Time())
                .Let(RaiseEvent);

        private Error RaiseEventAndReturnAnError(Func<Guid, DateTime, IEvent> eventFactory)
            => AnError(RaiseEvent(eventFactory).ToString()!);
    }
}