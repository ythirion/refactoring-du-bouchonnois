package bouchonnois.tests.unit;

import bouchonnois.service.exceptions.LaChasseEstDéjaEnCours;
import bouchonnois.service.exceptions.LaPartieDeChasseNexistePas;
import bouchonnois.service.exceptions.QuandCestFiniCestFini;
import bouchonnois.tests.assertions.PartieDeChasseAssert;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import static bouchonnois.tests.builders.PartieDeChasseBuilder.surUnTerrainRicheEnGalinettes;
import static bouchonnois.tests.builders.PartieDeChasseBuilder.unePartieDeChasseInexistante;
import static org.assertj.core.api.Assertions.assertThat;

class ReprendreLaPartieDeChasse extends PartieDeChasseServiceTests {
    @Test
    void quand_lapéro_est_en_cours() {
        given(
                unePartieDeChasseExistante(
                        surUnTerrainRicheEnGalinettes()
                                .aLapéro()
                ));

        when(id -> partieDeChasseService.reprendreLaPartie(id));

        then(savedPartieDeChasse -> PartieDeChasseAssert.assertPartieDeChasse(savedPartieDeChasse)
                .hasEmittedEvent(now, "Reprise de la chasse")
                .beEnCours()
        );
    }

    @Nested
    class Echoue {
        @Test
        void car_partie_nexiste_pas() {
            given(unePartieDeChasseInexistante());

            when(id -> partieDeChasseService.reprendreLaPartie(id));

            thenThrow(LaPartieDeChasseNexistePas.class, savedPartieDeChasse -> assertThat(savedPartieDeChasse).isNull());
        }

        @Test
        void si_la_chasse_est_en_cours() {
            given(unePartieDeChasseExistante(
                    surUnTerrainRicheEnGalinettes()
            ));

            when(id -> partieDeChasseService.reprendreLaPartie(id));

            thenThrow(LaChasseEstDéjaEnCours.class, savedPartieDeChasse -> assertThat(savedPartieDeChasse).isNull());
        }

        @Test
        void si_la_partie_de_chasse_est_terminée() {
            given(unePartieDeChasseExistante(
                    surUnTerrainRicheEnGalinettes()
                            .terminée()
            ));

            when(id -> partieDeChasseService.reprendreLaPartie(id));

            thenThrow(QuandCestFiniCestFini.class, savedPartieDeChasse -> assertThat(savedPartieDeChasse).isNull());
        }
    }
}