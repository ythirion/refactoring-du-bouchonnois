package bouchonnois.tests.unit;

import bouchonnois.service.exceptions.LaPartieDeChasseNexistePas;
import bouchonnois.service.exceptions.OnEstDéjaEnTrainDePrendreLapéro;
import bouchonnois.service.exceptions.OnPrendPasLapéroQuandLaPartieEstTerminée;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import static bouchonnois.tests.assertions.PartieDeChasseAssert.assertPartieDeChasse;
import static bouchonnois.tests.builders.PartieDeChasseBuilder.surUnTerrainRicheEnGalinettes;
import static bouchonnois.tests.builders.PartieDeChasseBuilder.unePartieDeChasseInexistante;
import static org.assertj.core.api.Assertions.assertThat;

class PrendreLApéro extends PartieDeChasseServiceTests {
    @Test
    void quand_la_partie_est_en_cours() {
        given(
                unePartieDeChasseExistante(
                        surUnTerrainRicheEnGalinettes()
                )
        );

        when(id -> partieDeChasseService.prendreLapéro(id));

        then(savedPartieDeChasse ->
                assertPartieDeChasse(savedPartieDeChasse)
                        .hasEmittedEvent(now, "Petit apéro")
                        .beInApéro()
        );
    }

    @Nested
    class Echoue {
        @Test
        void car_partie_nexiste_pas() {
            given(unePartieDeChasseInexistante());

            when(id -> partieDeChasseService.prendreLapéro(id));

            thenThrow(LaPartieDeChasseNexistePas.class, savedPartieDeChasse -> assertThat(savedPartieDeChasse).isNull());
        }

        @Test
        void si_les_chasseurs_sont_déjà_en_apéro() {
            given(unePartieDeChasseExistante(
                    surUnTerrainRicheEnGalinettes()
                            .aLapéro()
            ));

            when(id -> partieDeChasseService.prendreLapéro(id));

            thenThrow(OnEstDéjaEnTrainDePrendreLapéro.class, savedPartieDeChasse -> assertThat(savedPartieDeChasse).isNull());
        }

        @Test
        void si_la_partie_de_chasse_est_terminée() {
            given(unePartieDeChasseExistante(
                    surUnTerrainRicheEnGalinettes()
                            .terminée()
            ));

            when(id -> partieDeChasseService.prendreLapéro(id));

            thenThrow(OnPrendPasLapéroQuandLaPartieEstTerminée.class, savedPartieDeChasse -> assertThat(savedPartieDeChasse).isNull());
        }
    }
}