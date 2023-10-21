package bouchonnois.tests.service;

import bouchonnois.domain.Chasseur;
import bouchonnois.domain.PartieDeChasse;
import bouchonnois.domain.PartieStatus;
import bouchonnois.service.PartieDeChasseService;
import bouchonnois.service.Terrain;
import bouchonnois.service.exceptions.*;
import bouchonnois.tests.doubles.PartieDeChasseRepositoryForTests;
import io.vavr.Tuple2;
import org.junit.jupiter.api.Test;

import java.time.LocalDate;
import java.util.ArrayList;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;

class PartieDeChasseServiceTests {
    static class DemarrerUnePartieDeChasse {
        @Test
        void avec_plusieurs_chasseurs() throws ImpossibleDeDémarrerUnePartieSansGalinettes, ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle, ImpossibleDeDémarrerUnePartieSansChasseur {
            var repository = new PartieDeChasseRepositoryForTests();
            var service = new PartieDeChasseService(repository, LocalDate::now);
            var chasseurs = new ArrayList<Tuple2<String, Integer>>() {{
                add(new Tuple2<>("Dédé", 20));
                add(new Tuple2<>("Bernard", 8));
                add(new Tuple2<>("Robert", 12));
            }};
            var terrainDeChasse = new Tuple2<>("Pitibon sur Sauldre", 3);

            var id = service.démarrer(
                    terrainDeChasse,
                    chasseurs
            );

            var savedPartieDeChasse = repository.getSavedPartieDeChasse();
            assertThat(savedPartieDeChasse.getId()).isEqualTo(id);
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
        }

        @Test
        void echoue_sans_chasseurs() {
            var repository = new PartieDeChasseRepositoryForTests();
            var service = new PartieDeChasseService(repository, LocalDate::now);
            var chasseurs = new ArrayList<Tuple2<String, Integer>>();
            var terrainDeChasse = new Tuple2<>("Pitibon sur Sauldre", 3);

            assertThatThrownBy(() -> service.démarrer(terrainDeChasse, chasseurs))
                    .isInstanceOf(ImpossibleDeDémarrerUnePartieSansChasseur.class);
            assertThat(repository.getSavedPartieDeChasse()).isNull();
        }

        @Test
        void echoue_avec_un_terrain_sans_galinettes() {
            var repository = new PartieDeChasseRepositoryForTests();
            var service = new PartieDeChasseService(repository, LocalDate::now);
            var chasseurs = new ArrayList<Tuple2<String, Integer>>();
            var terrainDeChasse = new Tuple2<>("Pitibon sur Sauldre", 0);

            assertThatThrownBy(() -> service.démarrer(terrainDeChasse, chasseurs))
                    .isInstanceOf(ImpossibleDeDémarrerUnePartieSansGalinettes.class);
            assertThat(repository.getSavedPartieDeChasse()).isNull();
        }

        @Test
        void echoue_si_un_chasseur_sans_balle() {
            var repository = new PartieDeChasseRepositoryForTests();
            var service = new PartieDeChasseService(repository, LocalDate::now);
            var chasseurs = new ArrayList<Tuple2<String, Integer>>() {{
                add(new Tuple2<>("Dédé", 20));
                add(new Tuple2<>("Bernard", 0));
            }};
            var terrainDeChasse = new Tuple2<>("Pitibon sur Sauldre", 3);

            assertThatThrownBy(() -> service.démarrer(terrainDeChasse, chasseurs))
                    .isInstanceOf(ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle.class);
            assertThat(repository.getSavedPartieDeChasse()).isNull();
        }
    }

    static class TirerSurUneGalinette {
        @Test
        void avec_un_chasseur_ayant_des_balles_et_assez_de_galinettes_sur_le_terrain() throws ChasseurInconnu, TasTropPicoledMonVieuxTasRienTouche, OnTirePasQuandLaPartieEstTerminee, TasPlusDeBallesMonVieuxChasseALaMain, LaPartieDeChasseNexistePas, OnTirePasPendantLapéroCestSacré {
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
                    setEvents(new ArrayList<>());
                }
            });
            var service = new PartieDeChasseService(repository, LocalDate::now);

            service.tirerSurUneGalinette(id, "Bernard");

            var savedPartieDeChasse = repository.getSavedPartieDeChasse();
            assertThat(savedPartieDeChasse.getId()).isEqualTo(id);
            assertThat(savedPartieDeChasse.getStatus()).isEqualTo(PartieStatus.EN_COURS);
            assertThat(savedPartieDeChasse.getTerrain().getNom()).isEqualTo("Pitibon sur Sauldre");
            assertThat(savedPartieDeChasse.getTerrain().getNbGalinettes()).isEqualTo(2);
            assertThat(savedPartieDeChasse.getChasseurs()).hasSize(3);
            assertThat(savedPartieDeChasse.getChasseurs().get(0).getNom()).isEqualTo("Dédé");
            assertThat(savedPartieDeChasse.getChasseurs().get(0).getBallesRestantes()).isEqualTo(20);
            assertThat(savedPartieDeChasse.getChasseurs().get(0).getNbGalinettes()).isZero();
            assertThat(savedPartieDeChasse.getChasseurs().get(1).getNom()).isEqualTo("Bernard");
            assertThat(savedPartieDeChasse.getChasseurs().get(1).getBallesRestantes()).isEqualTo(7);
            assertThat(savedPartieDeChasse.getChasseurs().get(1).getNbGalinettes()).isEqualTo(1);
            assertThat(savedPartieDeChasse.getChasseurs().get(2).getNom()).isEqualTo("Robert");
            assertThat(savedPartieDeChasse.getChasseurs().get(2).getBallesRestantes()).isEqualTo(12);
            assertThat(savedPartieDeChasse.getChasseurs().get(2).getNbGalinettes()).isZero();
        }

        @Test
        void echoue_car_partie_nexiste_pas() {
            var id = UUID.randomUUID();
            var repository = new PartieDeChasseRepositoryForTests();
            var service = new PartieDeChasseService(repository, LocalDate::now);

            assertThatThrownBy(() -> service.tirerSurUneGalinette(id, "Bernard"))
                    .isInstanceOf(LaPartieDeChasseNexistePas.class);
            assertThat(repository.getSavedPartieDeChasse()).isNull();
        }

        @Test
        void echoue_avec_un_chasseur_nayant_plus_de_balles() {
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
                            setBallesRestantes(0);
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
                    setEvents(new ArrayList<>());
                }
            });
            var service = new PartieDeChasseService(repository, LocalDate::now);

            assertThatThrownBy(() -> service.tirerSurUneGalinette(id, "Bernard"))
                    .isInstanceOf(TasPlusDeBallesMonVieuxChasseALaMain.class);
        }

        @Test
        void echoue_car_pas_de_galinettes_sur_le_terrain() {
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
                        setNbGalinettes(0);
                    }});
                    setStatus(PartieStatus.EN_COURS);
                    setEvents(new ArrayList<>());
                }
            });
            var service = new PartieDeChasseService(repository, LocalDate::now);

            assertThatThrownBy(() -> service.tirerSurUneGalinette(id, "Bernard"))
                    .isInstanceOf(TasTropPicoledMonVieuxTasRienTouche.class);
            assertThat(repository.getSavedPartieDeChasse()).isNull();
        }

        @Test
        void echoue_car_le_chasseur_n_est_pas_dans_la_partie() {
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
                    setEvents(new ArrayList<>());
                }
            });
            var service = new PartieDeChasseService(repository, LocalDate::now);

            assertThatThrownBy(() -> service.tirerSurUneGalinette(id, "Chasseur inconnu"))
                    .isInstanceOf(ChasseurInconnu.class);
            assertThat(repository.getSavedPartieDeChasse()).isNull();
        }

        @Test
        void echoue_si_les_chasseurs_sont_en_apéro() {
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
                    setStatus(PartieStatus.APÉRO);
                    setEvents(new ArrayList<>());
                }
            });
            var service = new PartieDeChasseService(repository, LocalDate::now);

            assertThatThrownBy(() -> service.tirerSurUneGalinette(id, "Bernard"))
                    .isInstanceOf(OnTirePasPendantLapéroCestSacré.class);
        }

        @Test
        void echoue_si_la_partie_est_terminée() {
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
                    setStatus(PartieStatus.TERMINÉE);
                    setEvents(new ArrayList<>());
                }
            });
            var service = new PartieDeChasseService(repository, LocalDate::now);

            assertThatThrownBy(() -> service.tirerSurUneGalinette(id, "Bernard"))
                    .isInstanceOf(OnTirePasQuandLaPartieEstTerminee.class);
        }
    }

    static class Tirer {
        @Test
        void avec_un_chasseur_ayant_des_balles_et_assez_de_galinettes_sur_le_terrain() throws ChasseurInconnu, TasTropPicoledMonVieuxTasRienTouche, OnTirePasQuandLaPartieEstTerminee, TasPlusDeBallesMonVieuxChasseALaMain, LaPartieDeChasseNexistePas, OnTirePasPendantLapéroCestSacré {
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
                    setEvents(new ArrayList<>());
                }
            });
            var service = new PartieDeChasseService(repository, LocalDate::now);

            service.tirer(id, "Bernard");

            var savedPartieDeChasse = repository.getSavedPartieDeChasse();
            assertThat(savedPartieDeChasse.getId()).isEqualTo(id);
            assertThat(savedPartieDeChasse.getStatus()).isEqualTo(PartieStatus.EN_COURS);
            assertThat(savedPartieDeChasse.getTerrain().getNom()).isEqualTo("Pitibon sur Sauldre");
            assertThat(savedPartieDeChasse.getTerrain().getNbGalinettes()).isEqualTo(3);
            assertThat(savedPartieDeChasse.getChasseurs()).hasSize(3);
            assertThat(savedPartieDeChasse.getChasseurs().get(0).getNom()).isEqualTo("Dédé");
            assertThat(savedPartieDeChasse.getChasseurs().get(0).getBallesRestantes()).isEqualTo(20);
            assertThat(savedPartieDeChasse.getChasseurs().get(0).getNbGalinettes()).isZero();
            assertThat(savedPartieDeChasse.getChasseurs().get(1).getNom()).isEqualTo("Bernard");
            assertThat(savedPartieDeChasse.getChasseurs().get(1).getBallesRestantes()).isEqualTo(7);
            assertThat(savedPartieDeChasse.getChasseurs().get(1).getNbGalinettes()).isZero();
            assertThat(savedPartieDeChasse.getChasseurs().get(2).getNom()).isEqualTo("Robert");
            assertThat(savedPartieDeChasse.getChasseurs().get(2).getBallesRestantes()).isEqualTo(12);
            assertThat(savedPartieDeChasse.getChasseurs().get(2).getNbGalinettes()).isZero();
        }

        @Test
        void echoue_car_partie_nexiste_pas() {
            var id = UUID.randomUUID();
            var repository = new PartieDeChasseRepositoryForTests();
            var service = new PartieDeChasseService(repository, LocalDate::now);

            assertThatThrownBy(() -> service.tirer(id, "Bernard"))
                    .isInstanceOf(LaPartieDeChasseNexistePas.class);
            assertThat(repository.getSavedPartieDeChasse()).isNull();
        }

        @Test
        void echoue_avec_un_chasseur_nayant_plus_de_balles() {
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
                            setBallesRestantes(0);
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
                    setEvents(new ArrayList<>());
                }
            });
            var service = new PartieDeChasseService(repository, LocalDate::now);

            assertThatThrownBy(() -> service.tirer(id, "Bernard"))
                    .isInstanceOf(TasPlusDeBallesMonVieuxChasseALaMain.class);
        }

        @Test
        void echoue_car_le_chasseur_n_est_pas_dans_la_partie() {
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
                    setEvents(new ArrayList<>());
                }
            });
            var service = new PartieDeChasseService(repository, LocalDate::now);

            assertThatThrownBy(() -> service.tirer(id, "Chasseur inconnu"))
                    .isInstanceOf(ChasseurInconnu.class);
            assertThat(repository.getSavedPartieDeChasse()).isNull();
        }

        @Test
        void echoue_si_les_chasseurs_sont_en_apéro() {
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
                    setStatus(PartieStatus.APÉRO);
                    setEvents(new ArrayList<>());
                }
            });
            var service = new PartieDeChasseService(repository, LocalDate::now);

            assertThatThrownBy(() -> service.tirer(id, "Bernard"))
                    .isInstanceOf(OnTirePasPendantLapéroCestSacré.class);
        }

        @Test
        void echoue_si_la_partie_est_terminée() {
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
                    setStatus(PartieStatus.TERMINÉE);
                    setEvents(new ArrayList<>());
                }
            });
            var service = new PartieDeChasseService(repository, LocalDate::now);

            assertThatThrownBy(() -> service.tirer(id, "Bernard"))
                    .isInstanceOf(OnTirePasQuandLaPartieEstTerminee.class);
        }
    }
}
