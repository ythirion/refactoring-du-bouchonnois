package bouchonnois.tests.unit;

import bouchonnois.domain.Event;
import bouchonnois.service.exceptions.LaPartieDeChasseNexistePas;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import java.time.LocalDateTime;
import java.util.concurrent.atomic.AtomicReference;

import static bouchonnois.tests.builders.ChasseurBuilder.*;
import static bouchonnois.tests.builders.PartieDeChasseBuilder.surUnTerrainRicheEnGalinettes;
import static bouchonnois.tests.builders.PartieDeChasseBuilder.unePartieDeChasseInexistante;
import static java.lang.System.lineSeparator;
import static java.time.Month.APRIL;
import static org.assertj.core.api.Assertions.assertThat;

class ConsulterStatus extends PartieDeChasseServiceTests {
    @Test
    void quand_la_partie_vient_de_démarrer() {
        given(
                unePartieDeChasseExistante(
                        surUnTerrainRicheEnGalinettes()
                                .avec(dédé(), bernard(), robert())
                                .events(new Event(LocalDateTime.of(2024, 4, 25, 9, 0, 12),
                                        "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"))
                )
        );

        final var status = new AtomicReference<String>();
        when(id -> status.set(partieDeChasseService.consulterStatus(id)));

        then(s -> assertThat(status.get()).isEqualTo("09:00 - La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"));
    }

    @Test
    void quand_la_partie_est_terminée() {
        given(
                unePartieDeChasseExistante(
                        surUnTerrainRicheEnGalinettes()
                                .avec(dédé(), bernard(), robert())
                                .events(
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 9, 0, 12), "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 9, 10, 0), "Dédé tire"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 9, 40, 0), "Robert tire sur une galinette"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 10, 0, 0), "Petit apéro"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 11, 0, 0), "Reprise de la chasse"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 11, 2, 0), "Bernard tire"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 11, 3, 0), "Bernard tire"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 11, 4, 0), "Dédé tire sur une galinette"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 11, 30, 0), "Robert tire sur une galinette"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 11, 40, 0), "Petit apéro"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 14, 30, 0), "Reprise de la chasse"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 0), "Bernard tire"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 1), "Bernard tire"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 2), "Bernard tire"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 3), "Bernard tire"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 4), "Bernard tire"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 5), "Bernard tire"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 6), "Bernard tire"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 7), "Bernard tire -> T'as plus de balles mon vieux, chasse à la main"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 15, 0, 0), "Robert tire sur une galinette"),
                                        new Event(LocalDateTime.of(2024, APRIL, 25, 15, 30, 0), "La partie de chasse est terminée, vainqueur :  Robert - 3 galinettes")
                                )
                )
        );

        final var status = new AtomicReference<String>();
        when(id -> status.set(partieDeChasseService.consulterStatus(id)));

        then(s -> assertThat(status.get())
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
                ));
    }

    @Nested
    class Echoue {
        @Test
        void car_partie_nexiste_pas() {
            given(unePartieDeChasseInexistante());
            when(id -> partieDeChasseService.consulterStatus(id));
            thenThrow(LaPartieDeChasseNexistePas.class, savedPartieDeChasse -> assertThat(savedPartieDeChasse).isNull());
        }
    }
}