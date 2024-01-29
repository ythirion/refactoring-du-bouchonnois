package bouchonnois.tests.unit;

import bouchonnois.domain.PartieStatus;
import bouchonnois.service.exceptions.ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle;
import bouchonnois.service.exceptions.ImpossibleDeDémarrerUnePartieSansChasseur;
import bouchonnois.service.exceptions.ImpossibleDeDémarrerUnePartieSansGalinettes;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import java.util.ArrayList;

import static bouchonnois.tests.assertions.PartieDeChasseAssert.assertPartieDeChasse;
import static bouchonnois.tests.builders.CommandBuilder.demarrerUnePartieDeChasse;
import static io.vavr.Tuple.of;
import static org.assertj.core.api.Assertions.assertThat;

class DemarrerUnePartieDeChasse extends PartieDeChasseServiceTests {
    @Test
    void avec_plusieurs_chasseurs() {
        var command = demarrerUnePartieDeChasse()
                .avec(of(DÉDÉ, 20), of(BERNARD, 8), of(ROBERT, 12))
                .surUnTerrainRicheEnGalinettes();

        var id = partieDeChasseService.démarrer(
                command.getTerrain(),
                command.getChasseurs()
        );

        var savedPartieDeChasse = repository.getSavedPartieDeChasse();
        assertThat(savedPartieDeChasse.getId()).isNotNull().isEqualTo(id);
        assertThat(savedPartieDeChasse.getStatus()).isEqualTo(PartieStatus.EN_COURS);
        assertThat(savedPartieDeChasse.getTerrain().getNom()).isEqualTo("Pitibon sur Sauldre");
        assertThat(savedPartieDeChasse.getTerrain().getNbGalinettes()).isEqualTo(3);
        assertThat(savedPartieDeChasse.getChasseurs()).hasSize(3);
        assertThat(savedPartieDeChasse.getChasseurs().get(0).getNom()).isEqualTo("Dédé");
        assertThat(savedPartieDeChasse.getChasseurs().get(0).getBallesRestantes()).isEqualTo(20);
        assertThat(savedPartieDeChasse.getChasseurs().get(0).getNbGalinettes()).isZero();
        assertThat(savedPartieDeChasse.getChasseurs().get(1).getNom()).isEqualTo("Bernard");
        assertThat(savedPartieDeChasse.getChasseurs().get(1).getBallesRestantes()).isEqualTo(8);
        assertThat(savedPartieDeChasse.getChasseurs().get(1).getNbGalinettes()).isZero();
        assertThat(savedPartieDeChasse.getChasseurs().get(2).getNom()).isEqualTo("Robert");
        assertThat(savedPartieDeChasse.getChasseurs().get(2).getBallesRestantes()).isEqualTo(12);
        assertThat(savedPartieDeChasse.getChasseurs().get(2).getNbGalinettes()).isZero();

        assertPartieDeChasse(savedPartieDeChasse)
                .hasEmittedEvent(now,
                        "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"
                );
    }

    @Nested
    class Echoue {
        @Test
        void sans_chasseurs() {
            executeAndAssertThrow(ImpossibleDeDémarrerUnePartieSansChasseur.class,
                    service -> service.démarrer(of("Pitibon sur Sauldre", 3), new ArrayList<>()),
                    partieDeChasse -> assertThat(partieDeChasse).isNull());
        }

        @Test
        void avec_un_terrain_sans_galinettes() {
            executeAndAssertThrow(ImpossibleDeDémarrerUnePartieSansGalinettes.class,
                    service -> service.démarrer(of("Pitibon sur Sauldre", 0), new ArrayList<>()),
                    partieDeChasse -> assertThat(partieDeChasse).isNull());
        }

        @Test
        void si_un_chasseur_sans_balle() {
            var command = demarrerUnePartieDeChasse()
                    .avec(of(DÉDÉ, 20), of(BERNARD, 0))
                    .surUnTerrainRicheEnGalinettes();

            executeAndAssertThrow(ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle.class,
                    service -> service.démarrer(command.getTerrain(), command.getChasseurs()),
                    partieDeChasse -> assertThat(partieDeChasse).isNull());
        }
    }
}