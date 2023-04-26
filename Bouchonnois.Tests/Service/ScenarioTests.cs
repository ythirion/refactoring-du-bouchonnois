using Bouchonnois.Service;
using Bouchonnois.Service.Exceptions;
using Bouchonnois.Tests.Doubles;

namespace Bouchonnois.Tests.Service;

public class ScenarioTests
{
    [Fact]
    public void DéroulerUnePartie()
    {
        var time = new DateTime(2024, 4, 25, 9, 0, 0);
        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, () => time);
        var chasseurs = new List<(string, int)>
        {
            ("Dédé", 20),
            ("Bernard", 8),
            ("Robert", 12)
        };
        var terrainDeChasse = ("Pitibon sur Sauldre", 4);
        var id = service.Demarrer(
            terrainDeChasse,
            chasseurs
        );

        time = time.Add(TimeSpan.FromMinutes(10));
        service.Tirer(id, "Dédé");

        time = time.Add(TimeSpan.FromMinutes(30));
        service.TirerSurUneGalinette(id, "Robert");

        time = time.Add(TimeSpan.FromMinutes(20));
        service.PrendreLapéro(id);

        time = time.Add(TimeSpan.FromHours(1));
        service.ReprendreLaPartie(id);

        time = time.Add(TimeSpan.FromMinutes(2));
        service.Tirer(id, "Bernard");

        time = time.Add(TimeSpan.FromMinutes(1));
        service.Tirer(id, "Bernard");

        time = time.Add(TimeSpan.FromMinutes(1));
        service.TirerSurUneGalinette(id, "Dédé");

        time = time.Add(TimeSpan.FromMinutes(26));
        service.TirerSurUneGalinette(id, "Robert");

        time = time.Add(TimeSpan.FromMinutes(10));
        service.PrendreLapéro(id);

        time = time.Add(TimeSpan.FromMinutes(170));
        service.ReprendreLaPartie(id);

        time = time.Add(TimeSpan.FromMinutes(11));
        service.Tirer(id, "Bernard");

        time = time.Add(TimeSpan.FromSeconds(1));
        service.Tirer(id, "Bernard");

        time = time.Add(TimeSpan.FromSeconds(1));
        service.Tirer(id, "Bernard");

        time = time.Add(TimeSpan.FromSeconds(1));
        service.Tirer(id, "Bernard");

        time = time.Add(TimeSpan.FromSeconds(1));
        service.Tirer(id, "Bernard");

        time = time.Add(TimeSpan.FromSeconds(1));
        service.Tirer(id, "Bernard");

        time = time.Add(TimeSpan.FromSeconds(1));

        try
        {
            service.Tirer(id, "Bernard");
        }
        catch (TasPlusDeBallesMonVieuxChasseALaMain e)
        {
        }

        time = time.Add(TimeSpan.FromMinutes(19));
        service.TirerSurUneGalinette(id, "Robert");

        time = time.Add(TimeSpan.FromMinutes(30));
        service.TerminerLaPartie(id);

        service.ConsulterStatus(id)
            .Should()
            .BeEquivalentTo(
                @"15:30 - La partie de chasse est terminée, vainqueur : Robert - 3 galinettes
15:00 - Robert tire sur une galinette
14:41 - Bernard tire -> T'as plus de balles mon vieux, chasse à la main
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:30 - Reprise de la chasse
11:40 - Petit apéro
11:30 - Robert tire sur une galinette
11:04 - Dédé tire sur une galinette
11:03 - Bernard tire
11:02 - Bernard tire
11:00 - Reprise de la chasse
10:00 - Petit apéro
09:40 - Robert tire sur une galinette
09:10 - Dédé tire
09:00 - La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"
            );
    }
}