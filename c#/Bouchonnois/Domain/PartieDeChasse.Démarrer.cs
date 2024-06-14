using Bouchonnois.Domain.Démarrer;
using Domain.Core;
using LanguageExt;

namespace Bouchonnois.Domain
{
    public sealed partial class PartieDeChasse
    {
        private PartieDeChasse(Guid id, Func<DateTime> timeProvider) : base(timeProvider) => Id = id;

        private PartieDeChasse(Guid id,
            Func<DateTime> timeProvider,
            Terrain terrain,
            Chasseur[] chasseurs) : this(id, timeProvider)
        {
            RaiseEvent((_, time) =>
                new PartieDeChasseDémarrée(id,
                    time,
                    new TerrainCréé(terrain.Nom, terrain.NbGalinettes),
                    chasseurs.Map(c => new ChasseurCréé(c.Nom, c.BallesRestantes)).ToArray()
                )
            );
        }

        public static Either<Error, PartieDeChasse> Create(
            Func<DateTime> timeProvider,
            DemarrerPartieDeChasse demarrerPartieDeChasse)
        {
            if (!IsTerrainValide(demarrerPartieDeChasse.TerrainDeChasse))
            {
                return Error.AnError("Impossible de démarrer une partie de chasse sur un terrain sans galinettes");
            }

            if (!ContainsChasseurs(demarrerPartieDeChasse.Chasseurs.ToArray()))
            {
                return Error.AnError("Impossible de démarrer une partie de chasse sans chasseurs...");
            }

            if (AuMoinsUnChasseurNaPasDeBalles(demarrerPartieDeChasse.Chasseurs.ToArray()))
            {
                return Error.AnError("Impossible de démarrer une partie de chasse avec un chasseur sans balle(s)...");
            }

            return new PartieDeChasse(
                Guid.NewGuid(),
                timeProvider,
                new Terrain(
                    demarrerPartieDeChasse.TerrainDeChasse.Nom,
                    demarrerPartieDeChasse.TerrainDeChasse.NbGalinettes
                ),
                demarrerPartieDeChasse.Chasseurs.Select(c => new Chasseur(c.Nom, c.NbBalles)).ToArray()
            );
        }

        [EventSourced]
        private void Apply(PartieDeChasseDémarrée @event)
        {
            Id = @event.Id;
            _chasseurs = @event.Chasseurs.Map(c => new Chasseur(c.Nom, c.BallesRestantes)).ToArray();
            _terrain = new Terrain(@event.Terrain.Nom, @event.Terrain.NbGalinettes);
            _status = PartieStatus.EnCours;
        }

        private static bool IsTerrainValide(TerrainDeChasse terrainDeChasse) => terrainDeChasse.NbGalinettes > 0;
        private static bool ContainsChasseurs(Démarrer.Chasseur[] chasseurs) => chasseurs.Any();

        private static bool AuMoinsUnChasseurNaPasDeBalles(Démarrer.Chasseur[] chasseurs)
            => chasseurs.Exists(c => c.NbBalles == 0);
    }
}