package bouchonnois.tests.acceptance;

import bouchonnois.service.PartieDeChasseService;
import bouchonnois.service.exceptions.*;
import bouchonnois.tests.doubles.PartieDeChasseRepositoryForTests;
import io.vavr.Tuple2;
import org.junit.jupiter.api.Test;

import java.time.LocalDateTime;
import java.util.ArrayList;

import static java.lang.System.lineSeparator;
import static java.time.Month.APRIL;
import static org.assertj.core.api.Assertions.assertThat;

class ScenarioTests {
    private LocalDateTime time = LocalDateTime.of(2024, APRIL, 25, 9, 0, 0);

    @Test
    void dérouler_une_partie() throws ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle, ImpossibleDeDémarrerUnePartieSansChasseur, ImpossibleDeDémarrerUnePartieSansGalinettes, OnTirePasPendantLapéroCestSacré, ChasseurInconnu, TasTropPicoléMonVieuxTasRienTouché, OnTirePasQuandLaPartieEstTerminée, TasPlusDeBallesMonVieuxChasseALaMain, LaPartieDeChasseNexistePas, OnPrendPasLapéroQuandLaPartieEstTerminée, OnEstDéjaEnTrainDePrendreLapéro, LaChasseEstDéjaEnCours, QuandCestFiniCestFini {

        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, () -> time);
        var chasseurs = new ArrayList<Tuple2<String, Integer>>() {{
            add(new Tuple2<>("Dédé", 20));
            add(new Tuple2<>("Bernard", 8));
            add(new Tuple2<>("Robert", 12));
        }};
        var terrainDeChasse = new Tuple2<>("Pitibon sur Sauldre", 4);

        var id = service.démarrer(
                terrainDeChasse,
                chasseurs
        );

        time = time.plusMinutes(10);
        service.tirer(id, "Dédé");

        time = time.plusMinutes(30);
        service.tirerSurUneGalinette(id, "Robert");

        time = time.plusMinutes(20);
        service.prendreLapéro(id);

        time = time.plusHours(1);
        service.reprendreLaPartie(id);

        time = time.plusMinutes(2);
        service.tirer(id, "Bernard");

        time = time.plusMinutes(1);
        service.tirer(id, "Bernard");

        time = time.plusMinutes(1);
        service.tirerSurUneGalinette(id, "Dédé");

        time = time.plusMinutes(26);
        service.tirerSurUneGalinette(id, "Robert");

        time = time.plusMinutes(10);
        service.prendreLapéro(id);

        time = time.plusMinutes(170);
        service.reprendreLaPartie(id);

        time = time.plusMinutes(11);
        service.tirer(id, "Bernard");

        time = time.plusSeconds(1);
        service.tirer(id, "Bernard");

        time = time.plusSeconds(1);
        service.tirer(id, "Bernard");

        time = time.plusSeconds(1);
        service.tirer(id, "Bernard");

        time = time.plusSeconds(1);
        service.tirer(id, "Bernard");

        time = time.plusSeconds(1);
        service.tirer(id, "Bernard");

        time = time.plusSeconds(1);

        try {
            service.tirer(id, "Bernard");
        } catch (TasPlusDeBallesMonVieuxChasseALaMain e) {
        }

        time = time.plusMinutes(19);
        service.tirerSurUneGalinette(id, "Robert");

        time = time.plusMinutes(30);
        service.terminerLaPartie(id);

        assertThat(service.consulterStatus(id))
                .isEqualTo("15:30 - La partie de chasse est terminée, vainqueur : Robert - 3 galinettes" + lineSeparator() +
                        "15:00 - Robert tire sur une galinette" + lineSeparator() +
                        "14:41 - Bernard tire -> T'as plus de balles mon vieux, chasse à la main" + lineSeparator() +
                        "14:41 - Bernard tire" + lineSeparator() +
                        "14:41 - Bernard tire" + lineSeparator() +
                        "14:41 - Bernard tire" + lineSeparator() +
                        "14:41 - Bernard tire" + lineSeparator() +
                        "14:41 - Bernard tire" + lineSeparator() +
                        "14:41 - Bernard tire" + lineSeparator() +
                        "14:30 - Reprise de la chasse" + lineSeparator() +
                        "11:40 - Petit apéro" + lineSeparator() +
                        "11:30 - Robert tire sur une galinette" + lineSeparator() +
                        "11:04 - Dédé tire sur une galinette" + lineSeparator() +
                        "11:03 - Bernard tire" + lineSeparator() +
                        "11:02 - Bernard tire" + lineSeparator() +
                        "11:00 - Reprise de la chasse" + lineSeparator() +
                        "10:00 - Petit apéro" + lineSeparator() +
                        "09:40 - Robert tire sur une galinette" + lineSeparator() +
                        "09:10 - Dédé tire" + lineSeparator() +
                        "09:00 - La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"
                );
    }
}
