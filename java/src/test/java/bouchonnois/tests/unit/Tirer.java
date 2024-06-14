package bouchonnois.tests.unit;

import bouchonnois.service.exceptions.*;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import static bouchonnois.tests.assertions.PartieDeChasseAssert.assertPartieDeChasse;
import static bouchonnois.tests.builders.ChasseurBuilder.*;
import static bouchonnois.tests.builders.PartieDeChasseBuilder.surUnTerrainRicheEnGalinettes;
import static bouchonnois.tests.builders.PartieDeChasseBuilder.unePartieDeChasseInexistante;
import static org.assertj.core.api.Assertions.assertThat;

class Tirer extends PartieDeChasseServiceTests {
    @Test
    void avec_un_chasseur_ayant_des_balles_et_assez_de_galinettes_sur_le_terrain() {
        given(
                unePartieDeChasseExistante(
                        surUnTerrainRicheEnGalinettes()
                                .avec(bernard())
                ));

        when(id -> partieDeChasseService.tirer(id, BERNARD));

        then(savedPartieDeChasse -> assertPartieDeChasse(savedPartieDeChasse)
                .hasEmittedEvent(now, "Bernard tire")
                .chasseurATiré(BERNARD, 7)
                .galinettesSurLeTerrain(3)
        );
    }

    @Nested
    class Echoue {
        @Test
        void car_partie_nexiste_pas() {
            given(unePartieDeChasseInexistante());

            when(id -> partieDeChasseService.tirer(id, BERNARD));

            thenThrow(LaPartieDeChasseNexistePas.class, savedPartieDeChasse -> assertThat(savedPartieDeChasse).isNull());
        }

        @Test
        void avec_un_chasseur_nayant_plus_de_balles() {
            given(
                    unePartieDeChasseExistante(
                            surUnTerrainRicheEnGalinettes()
                                    .avec(dédé(), bernard().sansBalles(), robert())
                    ));

            when(id -> partieDeChasseService.tirer(id, BERNARD));

            thenThrow(TasPlusDeBallesMonVieuxChasseALaMain.class, savedPartieDeChasse ->
                    assertPartieDeChasse(savedPartieDeChasse)
                            .hasEmittedEvent(now, "Bernard tire -> T'as plus de balles mon vieux, chasse à la main")
            );
        }

        @Test
        void car_le_chasseur_n_est_pas_dans_la_partie() {
            given(
                    unePartieDeChasseExistante(
                            surUnTerrainRicheEnGalinettes()
                    ));

            when(id -> partieDeChasseService.tirer(id, CHASSEUR_INCONNU));

            thenThrow(ChasseurInconnu.class, savedPartieDeChasse -> assertThat(savedPartieDeChasse).isNull());
        }

        @Test
        void si_les_chasseurs_sont_en_apéro() {
            given(
                    unePartieDeChasseExistante(
                            surUnTerrainRicheEnGalinettes()
                                    .aLapéro()
                    ));

            when(id -> partieDeChasseService.tirer(id, CHASSEUR_INCONNU));

            thenThrow(OnTirePasPendantLapéroCestSacré.class, savedPartieDeChasse ->
                    assertPartieDeChasse(savedPartieDeChasse)
                            .hasEmittedEvent(now, "Chasseur inconnu veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!")
            );
        }

        @Test
        void si_la_partie_est_terminée() {
            given(
                    unePartieDeChasseExistante(
                            surUnTerrainRicheEnGalinettes()
                                    .terminée()
                    ));

            when(id -> partieDeChasseService.tirer(id, CHASSEUR_INCONNU));

            thenThrow(OnTirePasQuandLaPartieEstTerminée.class, savedPartieDeChasse ->
                    assertPartieDeChasse(savedPartieDeChasse)
                            .hasEmittedEvent(now, "Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée")
            );
        }
    }
}