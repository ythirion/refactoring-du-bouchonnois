package bouchonnois.tests.service;

import bouchonnois.domain.Chasseur;
import bouchonnois.domain.Event;
import bouchonnois.domain.PartieDeChasse;
import bouchonnois.domain.PartieStatus;
import bouchonnois.service.PartieDeChasseService;
import bouchonnois.service.Terrain;
import bouchonnois.service.exceptions.*;
import bouchonnois.tests.doubles.PartieDeChasseRepositoryForTests;
import io.vavr.Tuple2;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.UUID;

import static java.lang.System.lineSeparator;
import static java.time.Month.APRIL;
import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;

class PartieDeChasseServiceTests {
    @Nested
    class DemarrerUnePartieDeChasse {
        @Test
        void avec_plusieurs_chasseurs() throws ImpossibleDeDémarrerUnePartieSansGalinettes, ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle, ImpossibleDeDémarrerUnePartieSansChasseur {
            var repository = new PartieDeChasseRepositoryForTests();
            var service = new PartieDeChasseService(repository, LocalDateTime::now);
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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);
            var chasseurs = new ArrayList<Tuple2<String, Integer>>();
            var terrainDeChasse = new Tuple2<>("Pitibon sur Sauldre", 3);

            assertThatThrownBy(() -> service.démarrer(terrainDeChasse, chasseurs))
                    .isInstanceOf(ImpossibleDeDémarrerUnePartieSansChasseur.class);
            assertThat(repository.getSavedPartieDeChasse()).isNull();
        }

        @Test
        void echoue_avec_un_terrain_sans_galinettes() {
            var repository = new PartieDeChasseRepositoryForTests();
            var service = new PartieDeChasseService(repository, LocalDateTime::now);
            var chasseurs = new ArrayList<Tuple2<String, Integer>>();
            var terrainDeChasse = new Tuple2<>("Pitibon sur Sauldre", 0);

            assertThatThrownBy(() -> service.démarrer(terrainDeChasse, chasseurs))
                    .isInstanceOf(ImpossibleDeDémarrerUnePartieSansGalinettes.class);
            assertThat(repository.getSavedPartieDeChasse()).isNull();
        }

        @Test
        void echoue_si_un_chasseur_sans_balle() {
            var repository = new PartieDeChasseRepositoryForTests();
            var service = new PartieDeChasseService(repository, LocalDateTime::now);
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

    @Nested
    class TirerSurUneGalinette {
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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

            assertThatThrownBy(() -> service.tirerSurUneGalinette(id, "Bernard"))
                    .isInstanceOf(OnTirePasQuandLaPartieEstTerminee.class);
        }
    }

    @Nested
    class Tirer {
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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

            assertThatThrownBy(() -> service.tirer(id, "Bernard"))
                    .isInstanceOf(OnTirePasQuandLaPartieEstTerminee.class);
        }
    }

    @Nested
    class PrendreLApéro {
        @Test
        void quand_la_partie_est_en_cours() throws LaPartieDeChasseNexistePas, OnPrendPasLapéroQuandLaPartieEstTerminée, OnEstDéjaEnTrainDePrendreLapéro {
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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

            service.prendreLapéro(id);

            var savedPartieDeChasse = repository.getSavedPartieDeChasse();
            assertThat(savedPartieDeChasse.getId()).isEqualTo(id);
            assertThat(savedPartieDeChasse.getStatus()).isEqualTo(PartieStatus.APÉRO);
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
        void echoue_car_partie_nexiste_pas() {
            var id = UUID.randomUUID();
            var repository = new PartieDeChasseRepositoryForTests();
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

            assertThatThrownBy(() -> service.prendreLapéro(id))
                    .isInstanceOf(LaPartieDeChasseNexistePas.class);
            assertThat(repository.getSavedPartieDeChasse()).isNull();
        }

        @Test
        void echoue_si_les_chasseurs_sont_déjà_en_apéro() {
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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

            assertThatThrownBy(() -> service.prendreLapéro(id))
                    .isInstanceOf(OnEstDéjaEnTrainDePrendreLapéro.class);
            assertThat(repository.getSavedPartieDeChasse()).isNull();
        }

        @Test
        void echoue_si_la_partie_de_chasse_est_terminée() {
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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

            assertThatThrownBy(() -> service.prendreLapéro(id))
                    .isInstanceOf(OnPrendPasLapéroQuandLaPartieEstTerminée.class);
            assertThat(repository.getSavedPartieDeChasse()).isNull();
        }
    }

    @Nested
    class ReprendreLaPartieDeChasse {
        @Test
        void quand_lapéro_est_en_cours() throws LaPartieDeChasseNexistePas, LaChasseEstDéjaEnCours, QuandCestFiniCestFini {
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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

            service.reprendreLaPartie(id);

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
        void echoue_car_partie_nexiste_pas() {
            var id = UUID.randomUUID();
            var repository = new PartieDeChasseRepositoryForTests();
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

            assertThatThrownBy(() -> service.reprendreLaPartie(id))
                    .isInstanceOf(LaPartieDeChasseNexistePas.class);
            assertThat(repository.getSavedPartieDeChasse()).isNull();
        }

        @Test
        void echoue_si_la_chasse_est_en_cours() {
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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

            assertThatThrownBy(() -> service.reprendreLaPartie(id))
                    .isInstanceOf(LaChasseEstDéjaEnCours.class);
            assertThat(repository.getSavedPartieDeChasse()).isNull();
        }

        @Test
        void echoue_si_la_partie_de_chasse_est_terminée() {
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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

            assertThatThrownBy(() -> service.reprendreLaPartie(id))
                    .isInstanceOf(QuandCestFiniCestFini.class);
            assertThat(repository.getSavedPartieDeChasse()).isNull();
        }
    }

    @Nested
    class TerminerLaPartieDeChasse {
        @Test
        void quand_la_partie_est_en_cours_et_1_seul_chasseur_gagne() throws QuandCestFiniCestFini {
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
                    setEvents(new ArrayList<>());
                }
            });
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

            var meilleurChasseur = service.terminerLaPartie(id);

            var savedPartieDeChasse = repository.getSavedPartieDeChasse();
            assertThat(savedPartieDeChasse.getId()).isEqualTo(id);
            assertThat(savedPartieDeChasse.getStatus()).isEqualTo(PartieStatus.TERMINÉE);
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
            assertThat(savedPartieDeChasse.getChasseurs().get(2).getNbGalinettes()).isEqualTo(2);

            assertThat(meilleurChasseur).isEqualTo("Robert");
        }

        @Test
        void quand_la_partie_est_en_cours_et_1_seul_chasseur_dans_la_partie() throws QuandCestFiniCestFini {
            var id = UUID.randomUUID();
            var repository = new PartieDeChasseRepositoryForTests();

            repository.add(new PartieDeChasse() {
                {
                    setId(id);
                    setChasseurs(new ArrayList<>() {{
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
                    setEvents(new ArrayList<>());
                }
            });
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

            var meilleurChasseur = service.terminerLaPartie(id);

            var savedPartieDeChasse = repository.getSavedPartieDeChasse();
            assertThat(savedPartieDeChasse.getId()).isEqualTo(id);
            assertThat(savedPartieDeChasse.getStatus()).isEqualTo(PartieStatus.TERMINÉE);
            assertThat(savedPartieDeChasse.getTerrain().getNom()).isEqualTo("Pitibon sur Sauldre");
            assertThat(savedPartieDeChasse.getTerrain().getNbGalinettes()).isEqualTo(3);
            assertThat(savedPartieDeChasse.getChasseurs()).hasSize(1);
            assertThat(savedPartieDeChasse.getChasseurs().get(0).getNom()).isEqualTo("Robert");
            assertThat(savedPartieDeChasse.getChasseurs().get(0).getBallesRestantes()).isEqualTo(12);
            assertThat(savedPartieDeChasse.getChasseurs().get(0).getNbGalinettes()).isEqualTo(2);

            assertThat(meilleurChasseur).isEqualTo("Robert");
        }

        @Test
        void quand_la_partie_est_en_cours_et_2_chasseurs_ex_aequo() throws QuandCestFiniCestFini {
            var id = UUID.randomUUID();
            var repository = new PartieDeChasseRepositoryForTests();

            repository.add(new PartieDeChasse() {
                {
                    setId(id);
                    setChasseurs(new ArrayList<>() {{
                        add(new Chasseur() {{
                            setNom("Dédé");
                            setBallesRestantes(20);
                            setNbGalinettes(2);
                        }});
                        add(new Chasseur() {{
                            setNom("Bernard");
                            setBallesRestantes(8);
                            setNbGalinettes(2);
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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

            var meilleurChasseur = service.terminerLaPartie(id);

            var savedPartieDeChasse = repository.getSavedPartieDeChasse();
            assertThat(savedPartieDeChasse.getId()).isEqualTo(id);
            assertThat(savedPartieDeChasse.getStatus()).isEqualTo(PartieStatus.TERMINÉE);
            assertThat(savedPartieDeChasse.getTerrain().getNom()).isEqualTo("Pitibon sur Sauldre");
            assertThat(savedPartieDeChasse.getTerrain().getNbGalinettes()).isEqualTo(3);
            assertThat(savedPartieDeChasse.getChasseurs()).hasSize(3);
            assertThat(savedPartieDeChasse.getChasseurs().get(0).getNom()).isEqualTo("Dédé");
            assertThat(savedPartieDeChasse.getChasseurs().get(0).getBallesRestantes()).isEqualTo(20);
            assertThat(savedPartieDeChasse.getChasseurs().get(0).getNbGalinettes()).isEqualTo(2);
            assertThat(savedPartieDeChasse.getChasseurs().get(1).getNom()).isEqualTo("Bernard");
            assertThat(savedPartieDeChasse.getChasseurs().get(1).getBallesRestantes()).isEqualTo(8);
            assertThat(savedPartieDeChasse.getChasseurs().get(1).getNbGalinettes()).isEqualTo(2);
            assertThat(savedPartieDeChasse.getChasseurs().get(2).getNom()).isEqualTo("Robert");
            assertThat(savedPartieDeChasse.getChasseurs().get(2).getBallesRestantes()).isEqualTo(12);
            assertThat(savedPartieDeChasse.getChasseurs().get(2).getNbGalinettes()).isZero();

            assertThat(meilleurChasseur).isEqualTo("Dédé, Bernard");
        }

        @Test
        void quand_la_partie_est_en_cours_et_tout_le_monde_brocouille() throws QuandCestFiniCestFini {
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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

            var meilleurChasseur = service.terminerLaPartie(id);

            var savedPartieDeChasse = repository.getSavedPartieDeChasse();
            assertThat(savedPartieDeChasse.getId()).isEqualTo(id);
            assertThat(savedPartieDeChasse.getStatus()).isEqualTo(PartieStatus.TERMINÉE);
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

            assertThat(meilleurChasseur).isEqualTo("Brocouille");
        }

        @Test
        void quand_les_chasseurs_sont_a_lapéro_et_tous_ex_aequo() throws QuandCestFiniCestFini {
            var id = UUID.randomUUID();
            var repository = new PartieDeChasseRepositoryForTests();

            repository.add(new PartieDeChasse() {
                {
                    setId(id);
                    setChasseurs(new ArrayList<>() {{
                        add(new Chasseur() {{
                            setNom("Dédé");
                            setBallesRestantes(20);
                            setNbGalinettes(3);
                        }});
                        add(new Chasseur() {{
                            setNom("Bernard");
                            setBallesRestantes(8);
                            setNbGalinettes(3);
                        }});
                        add(new Chasseur() {{
                            setNom("Robert");
                            setBallesRestantes(12);
                            setNbGalinettes(3);
                        }});
                    }});

                    setTerrain(new Terrain("Pitibon sur Sauldre") {{
                        setNbGalinettes(3);
                    }});
                    setStatus(PartieStatus.APÉRO);
                    setEvents(new ArrayList<>());
                }
            });
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

            var meilleurChasseur = service.terminerLaPartie(id);

            var savedPartieDeChasse = repository.getSavedPartieDeChasse();
            assertThat(savedPartieDeChasse.getId()).isEqualTo(id);
            assertThat(savedPartieDeChasse.getStatus()).isEqualTo(PartieStatus.TERMINÉE);
            assertThat(savedPartieDeChasse.getTerrain().getNom()).isEqualTo("Pitibon sur Sauldre");
            assertThat(savedPartieDeChasse.getTerrain().getNbGalinettes()).isEqualTo(3);
            assertThat(savedPartieDeChasse.getChasseurs()).hasSize(3);
            assertThat(savedPartieDeChasse.getChasseurs().get(0).getNom()).isEqualTo("Dédé");
            assertThat(savedPartieDeChasse.getChasseurs().get(0).getBallesRestantes()).isEqualTo(20);
            assertThat(savedPartieDeChasse.getChasseurs().get(0).getNbGalinettes()).isEqualTo(3);
            assertThat(savedPartieDeChasse.getChasseurs().get(1).getNom()).isEqualTo("Bernard");
            assertThat(savedPartieDeChasse.getChasseurs().get(1).getBallesRestantes()).isEqualTo(8);
            assertThat(savedPartieDeChasse.getChasseurs().get(1).getNbGalinettes()).isEqualTo(3);
            assertThat(savedPartieDeChasse.getChasseurs().get(2).getNom()).isEqualTo("Robert");
            assertThat(savedPartieDeChasse.getChasseurs().get(2).getBallesRestantes()).isEqualTo(12);
            assertThat(savedPartieDeChasse.getChasseurs().get(2).getNbGalinettes()).isEqualTo(3);

            assertThat(meilleurChasseur).isEqualTo("Dédé, Bernard, Robert");
        }

        @Test
        void echoue_si_la_partie_est_déja_terminée() {
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
            var service = new PartieDeChasseService(repository, LocalDateTime::now);

            assertThatThrownBy(() -> service.terminerLaPartie(id))
                    .isInstanceOf(QuandCestFiniCestFini.class);
            assertThat(repository.getSavedPartieDeChasse()).isNull();
        }
    }

    @Nested
    class ConsulterStatus {
        @Test
        void quand_la_partie_vient_de_démarrer() throws LaPartieDeChasseNexistePas, LaChasseEstDéjaEnCours, QuandCestFiniCestFini {
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
        void quand_la_partie_est_terminée() throws LaPartieDeChasseNexistePas, LaChasseEstDéjaEnCours, QuandCestFiniCestFini {
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
    }
}
