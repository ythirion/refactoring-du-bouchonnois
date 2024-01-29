package bouchonnois.tests.unit;

import bouchonnois.service.exceptions.QuandCestFiniCestFini;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import java.util.concurrent.atomic.AtomicReference;

import static bouchonnois.tests.assertions.PartieDeChasseAssert.assertPartieDeChasse;
import static bouchonnois.tests.builders.ChasseurBuilder.*;
import static bouchonnois.tests.builders.PartieDeChasseBuilder.surUnTerrainRicheEnGalinettes;
import static org.assertj.core.api.Assertions.assertThat;

class TerminerLaPartieDeChasse extends PartieDeChasseServiceTests {
    @Test
    void quand_la_partie_est_en_cours_et_1_seul_chasseur_gagne() {
        given(
                unePartieDeChasseExistante(
                        surUnTerrainRicheEnGalinettes()
                                .avec(dédé(), bernard(), robert().ayantTué(2))
                ));

        var winner = new AtomicReference<String>();
        when(id -> winner.set(partieDeChasseService.terminerLaPartie(id)));

        then(savedPartieDeChasse -> assertPartieDeChasse(savedPartieDeChasse)
                        .hasEmittedEvent(now, "La partie de chasse est terminée, vainqueur : Robert - 2 galinettes"),
                () -> assertThat(winner.get()).isEqualTo(ROBERT));
    }

    @Test
    void quand_la_partie_est_en_cours_et_1_seul_chasseur_dans_la_partie() {
        given(
                unePartieDeChasseExistante(
                        surUnTerrainRicheEnGalinettes()
                                .avec(robert().ayantTué(2))
                ));

        var winner = new AtomicReference<String>();
        when(id -> winner.set(partieDeChasseService.terminerLaPartie(id)));

        then(savedPartieDeChasse -> assertPartieDeChasse(savedPartieDeChasse)
                        .hasEmittedEvent(now, "La partie de chasse est terminée, vainqueur : Robert - 2 galinettes"),
                () -> assertThat(winner.get()).isEqualTo(ROBERT));
    }

    @Test
    void quand_la_partie_est_en_cours_et_2_chasseurs_ex_aequo() {
        given(
                unePartieDeChasseExistante(
                        surUnTerrainRicheEnGalinettes()
                                .avec(dédé().ayantTué(2), bernard().ayantTué(2))
                ));

        var winner = new AtomicReference<String>();
        when(id -> winner.set(partieDeChasseService.terminerLaPartie(id)));

        then(savedPartieDeChasse -> assertPartieDeChasse(savedPartieDeChasse)
                        .hasEmittedEvent(now, "La partie de chasse est terminée, vainqueur : Dédé - 2 galinettes, Bernard - 2 galinettes"),
                () -> assertThat(winner.get()).isEqualTo("Dédé, Bernard"));
    }

    @Test
    void quand_la_partie_est_en_cours_et_tout_le_monde_brocouille() {
        given(
                unePartieDeChasseExistante(
                        surUnTerrainRicheEnGalinettes()
                ));

        var winner = new AtomicReference<String>();
        when(id -> winner.set(partieDeChasseService.terminerLaPartie(id)));

        then(savedPartieDeChasse -> assertPartieDeChasse(savedPartieDeChasse)
                        .hasEmittedEvent(now, "La partie de chasse est terminée, vainqueur : Brocouille"),
                () -> assertThat(winner.get()).isEqualTo("Brocouille"));
    }

    @Test
    void quand_les_chasseurs_sont_a_lapéro_et_tous_ex_aequo() {
        given(
                unePartieDeChasseExistante(
                        surUnTerrainRicheEnGalinettes()
                                .avec(dédé().ayantTué(3), bernard().ayantTué(3), robert().ayantTué(3))
                ));

        var winner = new AtomicReference<String>();
        when(id -> winner.set(partieDeChasseService.terminerLaPartie(id)));

        then(savedPartieDeChasse -> assertPartieDeChasse(savedPartieDeChasse)
                        .hasEmittedEvent(now, "La partie de chasse est terminée, vainqueur : Dédé - 3 galinettes, Bernard - 3 galinettes, Robert - 3 galinettes"),
                () -> assertThat(winner.get()).isEqualTo("Dédé, Bernard, Robert"));
    }

    @Nested
    class Echoue {
        @Test
        void si_la_partie_est_déja_terminée() {
            given(unePartieDeChasseExistante(
                    surUnTerrainRicheEnGalinettes()
                            .terminée()
            ));

            when(id -> partieDeChasseService.terminerLaPartie(id));

            thenThrow(QuandCestFiniCestFini.class, savedPartieDeChasse -> assertThat(savedPartieDeChasse).isNull());
        }
    }
}