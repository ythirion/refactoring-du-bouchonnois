package bouchonnois.tests.unit;

import bouchonnois.domain.Chasseur;
import bouchonnois.domain.Event;
import bouchonnois.domain.PartieDeChasse;
import bouchonnois.domain.PartieStatus;
import bouchonnois.service.PartieDeChasseService;
import bouchonnois.service.Terrain;
import bouchonnois.service.exceptions.LaPartieDeChasseNexistePas;
import bouchonnois.tests.doubles.PartieDeChasseRepositoryForTests;
import org.junit.jupiter.api.Test;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.UUID;

import static java.lang.System.lineSeparator;
import static java.time.Month.APRIL;
import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;

class ConsulterStatus extends PartieDeChasseServiceTests {
    @Test
    void quand_la_partie_vient_de_démarrer() throws LaPartieDeChasseNexistePas {
        var id = UUID.randomUUID();
        var repository = new PartieDeChasseRepositoryForTests();

        repository.add(new PartieDeChasse() {
            {
                setId(id);
                setChasseurs(new ArrayList<>() {{
                    add(new Chasseur() {{
                        setNom("Dédé");
                        setBallesRestantes(20);
                    }});
                    add(new Chasseur() {{
                        setNom("Bernard");
                        setBallesRestantes(8);
                    }});
                    add(new Chasseur() {{
                        setNom("Robert");
                        setBallesRestantes(12);
                    }});
                }});

                setTerrain(new Terrain("Pitibon sur Sauldre") {{
                    setNbGalinettes(3);
                }});
                setStatus(PartieStatus.EN_COURS);
                setEvents(new ArrayList<>() {{
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 9, 0, 12),
                            "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"));
                }});
            }
        });
        var service = new PartieDeChasseService(repository, LocalDateTime::now);

        var status = service.consulterStatus(id);

        assertThat(status)
                .isEqualTo("09:00 - La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)");
    }

    @Test
    void quand_la_partie_est_terminée() throws LaPartieDeChasseNexistePas {
        var id = UUID.randomUUID();
        var repository = new PartieDeChasseRepositoryForTests();

        repository.add(new PartieDeChasse() {
            {
                setId(id);
                setChasseurs(new ArrayList<>() {{
                    add(new Chasseur() {{
                        setNom("Dédé");
                        setBallesRestantes(20);
                    }});
                    add(new Chasseur() {{
                        setNom("Bernard");
                        setBallesRestantes(8);
                    }});
                    add(new Chasseur() {{
                        setNom("Robert");
                        setBallesRestantes(12);
                        setNbGalinettes(2);
                    }});
                }});

                setTerrain(new Terrain("Pitibon sur Sauldre") {{
                    setNbGalinettes(3);
                }});
                setStatus(PartieStatus.EN_COURS);
                setEvents(new ArrayList<>() {{
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 9, 0, 12), "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 9, 10, 0), "Dédé tire"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 9, 40, 0), "Robert tire sur une galinette"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 10, 0, 0), "Petit apéro"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 11, 0, 0), "Reprise de la chasse"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 11, 2, 0), "Bernard tire"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 11, 3, 0), "Bernard tire"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 11, 4, 0), "Dédé tire sur une galinette"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 11, 30, 0), "Robert tire sur une galinette"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 11, 40, 0), "Petit apéro"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 14, 30, 0), "Reprise de la chasse"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 0), "Bernard tire"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 1), "Bernard tire"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 2), "Bernard tire"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 3), "Bernard tire"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 4), "Bernard tire"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 5), "Bernard tire"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 6), "Bernard tire"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 7), "Bernard tire -> T'as plus de balles mon vieux, chasse à la main"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 15, 0, 0), "Robert tire sur une galinette"));
                    add(new Event(LocalDateTime.of(2024, APRIL, 25, 15, 30, 0), "La partie de chasse est terminée, vainqueur :  Robert - 3 galinettes"));
                }});
            }
        });
        var service = new PartieDeChasseService(repository, LocalDateTime::now);
        var status = service.consulterStatus(id);

        assertThat(status)
                .isEqualTo("15:30 - La partie de chasse est terminée, vainqueur :  Robert - 3 galinettes" + lineSeparator() +
                        "15:00 - Robert tire sur une galinette" + lineSeparator() +
                        "14:41 - Bernard tire -> T'as plus de balles mon vieux, chasse à la main" + lineSeparator() +
                        "14:41 - Bernard tire" + lineSeparator() +
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

    @Test
    void echoue_car_partie_nexiste_pas() {
        var id = UUID.randomUUID();
        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, LocalDateTime::now);

        assertThatThrownBy(() -> service.consulterStatus(id))
                .isInstanceOf(LaPartieDeChasseNexistePas.class);
        assertThat(repository.getSavedPartieDeChasse()).isNull();
    }
}
