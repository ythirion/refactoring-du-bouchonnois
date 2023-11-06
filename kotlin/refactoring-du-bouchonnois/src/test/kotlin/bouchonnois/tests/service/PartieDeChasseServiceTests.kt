package bouchonnois.tests.service

import bouchonnois.domain.Chasseur
import bouchonnois.domain.Event
import bouchonnois.domain.PartieDeChasse
import bouchonnois.domain.PartieStatus
import bouchonnois.service.PartieDeChasseService
import bouchonnois.service.Terrain
import bouchonnois.service.exceptions.*
import bouchonnois.tests.doubles.PartieDeChasseRepositoryForTests
import io.kotest.assertions.throwables.shouldThrow
import io.kotest.core.spec.style.FeatureSpec
import io.kotest.matchers.collections.shouldHaveSize
import io.kotest.matchers.shouldBe
import java.lang.System.lineSeparator
import java.time.LocalDateTime
import java.time.Month.APRIL
import java.util.*

class PartieDeChasseServiceTests : FeatureSpec({
    feature("démarrer une partie de chasse") {
        scenario("avec plusieurs chasseurs") {
            val repository = PartieDeChasseRepositoryForTests()
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }
            val chasseurs = mapOf(
                ("Dédé" to 20),
                ("Bernard" to 8),
                ("Robert" to 12)
            )
            val terrainDeChasse = ("Pitibon sur Sauldre" to 3)

            val id = service.démarrer(
                terrainDeChasse,
                chasseurs
            )

            val savedPartieDeChasse = repository.savedPartieDeChasse!!
            savedPartieDeChasse.id shouldBe id
            savedPartieDeChasse.status shouldBe PartieStatus.EN_COURS
            savedPartieDeChasse.terrain!!.nom shouldBe "Pitibon sur Sauldre"
            savedPartieDeChasse.terrain!!.nbGalinettes shouldBe 3
            savedPartieDeChasse.chasseurs!! shouldHaveSize 3
            savedPartieDeChasse.chasseurs!![0].nom shouldBe "Dédé"
            savedPartieDeChasse.chasseurs!![0].ballesRestantes shouldBe 20
            savedPartieDeChasse.chasseurs!![0].nbGalinettes shouldBe 0
            savedPartieDeChasse.chasseurs!![1].nom shouldBe "Bernard"
            savedPartieDeChasse.chasseurs!![1].ballesRestantes shouldBe 8
            savedPartieDeChasse.chasseurs!![1].nbGalinettes shouldBe 0
            savedPartieDeChasse.chasseurs!![2].nom shouldBe "Robert"
            savedPartieDeChasse.chasseurs!![2].ballesRestantes shouldBe 12
            savedPartieDeChasse.chasseurs!![2].nbGalinettes shouldBe 0
        }

        scenario("échoue sans chasseurs") {
            val repository = PartieDeChasseRepositoryForTests()
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }
            val chasseurs: Map<String, Int> = mapOf()
            val terrainDeChasse = ("Pitibon sur Sauldre" to 3)

            shouldThrow<ImpossibleDeDémarrerUnePartieSansChasseur> {
                service.démarrer(
                    terrainDeChasse,
                    chasseurs
                )
            }
            repository.savedPartieDeChasse shouldBe null
        }

        scenario("échoue avec un terrain sans galinettes") {
            val repository = PartieDeChasseRepositoryForTests()
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }
            val terrainDeChasse = ("Pitibon sur Sauldre" to 0)

            shouldThrow<ImpossibleDeDémarrerUnePartieSansGalinettes> {
                service.démarrer(
                    terrainDeChasse,
                    mapOf()
                )
            }
            repository.savedPartieDeChasse shouldBe null
        }

        scenario("échoue si chasseur sans balle") {
            val repository = PartieDeChasseRepositoryForTests()
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }
            val chasseurs = mapOf(
                ("Dédé" to 20),
                ("Bernard" to 0)
            )
            val terrainDeChasse = ("Pitibon sur Sauldre" to 3)

            shouldThrow<ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle> {
                service.démarrer(
                    terrainDeChasse,
                    chasseurs
                )
            }
            repository.savedPartieDeChasse shouldBe null
        }
    }

    feature("tirer sur une galinette") {
        scenario("avec un chasseur ayant des balles et assez de galinettes sur le terrain") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.EN_COURS
                        events = mutableListOf()
                    }

            )

            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            service.tirerSurUneGalinette(id, "Bernard")

            val savedPartieDeChasse = repository.savedPartieDeChasse!!
            savedPartieDeChasse.id shouldBe id
            savedPartieDeChasse.status shouldBe PartieStatus.EN_COURS
            savedPartieDeChasse.terrain!!.nom shouldBe "Pitibon sur Sauldre"
            savedPartieDeChasse.terrain!!.nbGalinettes shouldBe 2
            savedPartieDeChasse.chasseurs!! shouldHaveSize 3
            savedPartieDeChasse.chasseurs!![0].nom shouldBe "Dédé"
            savedPartieDeChasse.chasseurs!![0].ballesRestantes shouldBe 20
            savedPartieDeChasse.chasseurs!![0].nbGalinettes shouldBe 0
            savedPartieDeChasse.chasseurs!![1].nom shouldBe "Bernard"
            savedPartieDeChasse.chasseurs!![1].ballesRestantes shouldBe 7
            savedPartieDeChasse.chasseurs!![1].nbGalinettes shouldBe 1
            savedPartieDeChasse.chasseurs!![2].nom shouldBe "Robert"
            savedPartieDeChasse.chasseurs!![2].ballesRestantes shouldBe 12
            savedPartieDeChasse.chasseurs!![2].nbGalinettes shouldBe 0
        }

        scenario("échoue car partie n'existe pas") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            shouldThrow<LaPartieDeChasseNexistePas> {
                service.tirerSurUneGalinette(
                    id,
                    "Bernard"
                )
            }
            repository.savedPartieDeChasse shouldBe null
        }

        scenario("échoue avec un chasseur n'ayant plus de balles") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 0
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.EN_COURS
                        events = mutableListOf()
                    }

            )

            shouldThrow<TasPlusDeBallesMonVieuxChasseALaMain> {
                service.tirerSurUneGalinette(
                    id,
                    "Bernard"
                )
            }
        }

        scenario("échoue car pas de galinettes sur le terrain") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 0 }
                        status = PartieStatus.EN_COURS
                        events = mutableListOf()
                    }

            )
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            shouldThrow<TasTropPicoledMonVieuxTasRienTouche> {
                service.tirerSurUneGalinette(
                    id,
                    "Bernard"
                )
            }
            repository.savedPartieDeChasse shouldBe null
        }

        scenario("échoue car le chasseur n'est pas dans la partie") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.EN_COURS
                        events = mutableListOf()
                    }

            )
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            shouldThrow<ChasseurInconnu> {
                service.tirerSurUneGalinette(
                    id,
                    "Chasseur inconnu"
                )
            }
            repository.savedPartieDeChasse shouldBe null
        }

        scenario("échoue si les chasseurs sont en apéro") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.APÉRO
                        events = mutableListOf()
                    }

            )
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            shouldThrow<OnTirePasPendantLapéroCestSacré> {
                service.tirerSurUneGalinette(
                    id,
                    "Bernard"
                )
            }
        }

        scenario("échoue si la partie est terminée") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.TERMINÉE
                        events = mutableListOf()
                    }

            )
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            shouldThrow<OnTirePasQuandLaPartieEstTerminee> {
                service.tirerSurUneGalinette(
                    id,
                    "Bernard"
                )
            }
        }
    }

    feature("tirer") {
        scenario("avec un chasseur ayant des balles et assez de galinettes sur le terrain") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.EN_COURS
                        events = mutableListOf()
                    }

            )

            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            service.tirer(id, "Bernard")

            val savedPartieDeChasse = repository.savedPartieDeChasse!!
            savedPartieDeChasse.id shouldBe id
            savedPartieDeChasse.status shouldBe PartieStatus.EN_COURS
            savedPartieDeChasse.terrain!!.nom shouldBe "Pitibon sur Sauldre"
            savedPartieDeChasse.terrain!!.nbGalinettes shouldBe 3
            savedPartieDeChasse.chasseurs!! shouldHaveSize 3
            savedPartieDeChasse.chasseurs!![0].nom shouldBe "Dédé"
            savedPartieDeChasse.chasseurs!![0].ballesRestantes shouldBe 20
            savedPartieDeChasse.chasseurs!![0].nbGalinettes shouldBe 0
            savedPartieDeChasse.chasseurs!![1].nom shouldBe "Bernard"
            savedPartieDeChasse.chasseurs!![1].ballesRestantes shouldBe 7
            savedPartieDeChasse.chasseurs!![1].nbGalinettes shouldBe 0
            savedPartieDeChasse.chasseurs!![2].nom shouldBe "Robert"
            savedPartieDeChasse.chasseurs!![2].ballesRestantes shouldBe 12
            savedPartieDeChasse.chasseurs!![2].nbGalinettes shouldBe 0
        }

        scenario("échoue car partie n'existe pas") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            shouldThrow<LaPartieDeChasseNexistePas> {
                service.tirer(
                    id,
                    "Bernard"
                )
            }
            repository.savedPartieDeChasse shouldBe null
        }

        scenario("échoue avec un chasseur n'ayant plus de balles") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 0
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.EN_COURS
                        events = mutableListOf()
                    }

            )

            shouldThrow<TasPlusDeBallesMonVieuxChasseALaMain> {
                service.tirer(
                    id,
                    "Bernard"
                )
            }
        }

        scenario("échoue car le chasseur n'est pas dans la partie") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.EN_COURS
                        events = mutableListOf()
                    }

            )
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            shouldThrow<ChasseurInconnu> {
                service.tirer(
                    id,
                    "Chasseur inconnu"
                )
            }
            repository.savedPartieDeChasse shouldBe null
        }

        scenario("échoue si les chasseurs sont en apéro") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.APÉRO
                        events = mutableListOf()
                    }

            )
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            shouldThrow<OnTirePasPendantLapéroCestSacré> {
                service.tirer(
                    id,
                    "Bernard"
                )
            }
        }

        scenario("échoue si la partie est terminée") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.TERMINÉE
                        events = mutableListOf()
                    }

            )
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            shouldThrow<OnTirePasQuandLaPartieEstTerminee> {
                service.tirer(
                    id,
                    "Bernard"
                )
            }
        }
    }

    feature("prendre l'apéro") {
        scenario("quand la partie est en cours") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.EN_COURS
                        events = mutableListOf()
                    }

            )

            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            service.prendreLapéro(id)

            val savedPartieDeChasse = repository.savedPartieDeChasse!!
            savedPartieDeChasse.id shouldBe id
            savedPartieDeChasse.status shouldBe PartieStatus.APÉRO
            savedPartieDeChasse.terrain!!.nom shouldBe "Pitibon sur Sauldre"
            savedPartieDeChasse.terrain!!.nbGalinettes shouldBe 3
            savedPartieDeChasse.chasseurs!! shouldHaveSize 3
            savedPartieDeChasse.chasseurs!![0].nom shouldBe "Dédé"
            savedPartieDeChasse.chasseurs!![0].ballesRestantes shouldBe 20
            savedPartieDeChasse.chasseurs!![0].nbGalinettes shouldBe 0
            savedPartieDeChasse.chasseurs!![1].nom shouldBe "Bernard"
            savedPartieDeChasse.chasseurs!![1].ballesRestantes shouldBe 8
            savedPartieDeChasse.chasseurs!![1].nbGalinettes shouldBe 0
            savedPartieDeChasse.chasseurs!![2].nom shouldBe "Robert"
            savedPartieDeChasse.chasseurs!![2].ballesRestantes shouldBe 12
            savedPartieDeChasse.chasseurs!![2].nbGalinettes shouldBe 0
        }

        scenario("échoue car partie n'existe pas") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            shouldThrow<LaPartieDeChasseNexistePas> {
                service.prendreLapéro(id)
            }
            repository.savedPartieDeChasse shouldBe null
        }

        scenario("échoue si les chasseurs sont déja en apéro") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.APÉRO
                        events = mutableListOf()
                    }

            )

            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            shouldThrow<OnEstDéjaEnTrainDePrendreLapéro> {
                service.prendreLapéro(id)
            }
            repository.savedPartieDeChasse shouldBe null
        }

        scenario("échoue si la partie de chasse est terminée") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.TERMINÉE
                        events = mutableListOf()
                    }

            )

            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            shouldThrow<OnPrendPasLapéroQuandLaPartieEstTerminée> {
                service.prendreLapéro(id)
            }
            repository.savedPartieDeChasse shouldBe null
        }
    }

    feature("reprendre la partie de chasse") {
        scenario("quand l'apéro est en cours") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.APÉRO
                        events = mutableListOf()
                    }

            )
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            service.reprendreLaPartie(id)

            val savedPartieDeChasse = repository.savedPartieDeChasse!!
            savedPartieDeChasse.id shouldBe id
            savedPartieDeChasse.status shouldBe PartieStatus.EN_COURS
            savedPartieDeChasse.terrain!!.nom shouldBe "Pitibon sur Sauldre"
            savedPartieDeChasse.terrain!!.nbGalinettes shouldBe 3
            savedPartieDeChasse.chasseurs!! shouldHaveSize 3
            savedPartieDeChasse.chasseurs!![0].nom shouldBe "Dédé"
            savedPartieDeChasse.chasseurs!![0].ballesRestantes shouldBe 20
            savedPartieDeChasse.chasseurs!![0].nbGalinettes shouldBe 0
            savedPartieDeChasse.chasseurs!![1].nom shouldBe "Bernard"
            savedPartieDeChasse.chasseurs!![1].ballesRestantes shouldBe 8
            savedPartieDeChasse.chasseurs!![1].nbGalinettes shouldBe 0
            savedPartieDeChasse.chasseurs!![2].nom shouldBe "Robert"
            savedPartieDeChasse.chasseurs!![2].ballesRestantes shouldBe 12
            savedPartieDeChasse.chasseurs!![2].nbGalinettes shouldBe 0
        }

        scenario("échoue car partie n'existe pas") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            shouldThrow<LaPartieDeChasseNexistePas> {
                service.reprendreLaPartie(id)
            }
            repository.savedPartieDeChasse shouldBe null
        }

        scenario("échoue si la partie de chasse est en cours") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.EN_COURS
                        events = mutableListOf()
                    }

            )

            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            shouldThrow<LaChasseEstDéjaEnCours> {
                service.reprendreLaPartie(id)
            }
            repository.savedPartieDeChasse shouldBe null
        }

        scenario("échoue si la partie de chasse est terminée") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.TERMINÉE
                        events = mutableListOf()
                    }

            )

            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            shouldThrow<QuandCestFiniCestFini> {
                service.reprendreLaPartie(id)
            }
            repository.savedPartieDeChasse shouldBe null
        }
    }

    feature("terminer la partie de chasse") {
        scenario("quand la partie est en cours et 1 seul chasseur gagne") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                    this.nbGalinettes = 2
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.EN_COURS
                        events = mutableListOf()
                    }

            )
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            val meilleurChasseur = service.terminerLaPartie(id)

            val savedPartieDeChasse = repository.savedPartieDeChasse!!
            savedPartieDeChasse.id shouldBe id
            savedPartieDeChasse.status shouldBe PartieStatus.TERMINÉE
            savedPartieDeChasse.terrain!!.nom shouldBe "Pitibon sur Sauldre"
            savedPartieDeChasse.terrain!!.nbGalinettes shouldBe 3
            savedPartieDeChasse.chasseurs!! shouldHaveSize 3
            savedPartieDeChasse.chasseurs!![0].nom shouldBe "Dédé"
            savedPartieDeChasse.chasseurs!![0].ballesRestantes shouldBe 20
            savedPartieDeChasse.chasseurs!![0].nbGalinettes shouldBe 0
            savedPartieDeChasse.chasseurs!![1].nom shouldBe "Bernard"
            savedPartieDeChasse.chasseurs!![1].ballesRestantes shouldBe 8
            savedPartieDeChasse.chasseurs!![1].nbGalinettes shouldBe 0
            savedPartieDeChasse.chasseurs!![2].nom shouldBe "Robert"
            savedPartieDeChasse.chasseurs!![2].ballesRestantes shouldBe 12
            savedPartieDeChasse.chasseurs!![2].nbGalinettes shouldBe 2

            meilleurChasseur shouldBe "Robert"
        }

        scenario("quand la partie est en cours et 1 seul chasseur dans la partie") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                    this.nbGalinettes = 2
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.EN_COURS
                        events = mutableListOf()
                    }

            )
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            val meilleurChasseur = service.terminerLaPartie(id)

            val savedPartieDeChasse = repository.savedPartieDeChasse!!
            savedPartieDeChasse.id shouldBe id
            savedPartieDeChasse.status shouldBe PartieStatus.TERMINÉE
            savedPartieDeChasse.terrain!!.nom shouldBe "Pitibon sur Sauldre"
            savedPartieDeChasse.terrain!!.nbGalinettes shouldBe 3
            savedPartieDeChasse.chasseurs!! shouldHaveSize 1
            savedPartieDeChasse.chasseurs!![0].nom shouldBe "Robert"
            savedPartieDeChasse.chasseurs!![0].ballesRestantes shouldBe 12
            savedPartieDeChasse.chasseurs!![0].nbGalinettes shouldBe 2

            meilleurChasseur shouldBe "Robert"
        }

        scenario("quand la partie est en cours et 2 chasseurs ex aequo") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                    this.nbGalinettes = 2
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                    this.nbGalinettes = 2
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.EN_COURS
                        events = mutableListOf()
                    }

            )
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            val meilleurChasseur = service.terminerLaPartie(id)

            val savedPartieDeChasse = repository.savedPartieDeChasse!!
            savedPartieDeChasse.id shouldBe id
            savedPartieDeChasse.status shouldBe PartieStatus.TERMINÉE
            savedPartieDeChasse.terrain!!.nom shouldBe "Pitibon sur Sauldre"
            savedPartieDeChasse.terrain!!.nbGalinettes shouldBe 3
            savedPartieDeChasse.chasseurs!! shouldHaveSize 3
            savedPartieDeChasse.chasseurs!![0].nom shouldBe "Dédé"
            savedPartieDeChasse.chasseurs!![0].ballesRestantes shouldBe 20
            savedPartieDeChasse.chasseurs!![0].nbGalinettes shouldBe 2
            savedPartieDeChasse.chasseurs!![1].nom shouldBe "Bernard"
            savedPartieDeChasse.chasseurs!![1].ballesRestantes shouldBe 8
            savedPartieDeChasse.chasseurs!![1].nbGalinettes shouldBe 2
            savedPartieDeChasse.chasseurs!![2].nom shouldBe "Robert"
            savedPartieDeChasse.chasseurs!![2].ballesRestantes shouldBe 12
            savedPartieDeChasse.chasseurs!![2].nbGalinettes shouldBe 0

            meilleurChasseur shouldBe "Dédé, Bernard"
        }

        scenario("quand la partie est en cours et tout le monde brocouille") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.EN_COURS
                        events = mutableListOf()
                    }

            )
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            val meilleurChasseur = service.terminerLaPartie(id)

            val savedPartieDeChasse = repository.savedPartieDeChasse!!
            savedPartieDeChasse.id shouldBe id
            savedPartieDeChasse.status shouldBe PartieStatus.TERMINÉE
            savedPartieDeChasse.terrain!!.nom shouldBe "Pitibon sur Sauldre"
            savedPartieDeChasse.terrain!!.nbGalinettes shouldBe 3
            savedPartieDeChasse.chasseurs!! shouldHaveSize 3
            savedPartieDeChasse.chasseurs!![0].nom shouldBe "Dédé"
            savedPartieDeChasse.chasseurs!![0].ballesRestantes shouldBe 20
            savedPartieDeChasse.chasseurs!![0].nbGalinettes shouldBe 0
            savedPartieDeChasse.chasseurs!![1].nom shouldBe "Bernard"
            savedPartieDeChasse.chasseurs!![1].ballesRestantes shouldBe 8
            savedPartieDeChasse.chasseurs!![1].nbGalinettes shouldBe 0
            savedPartieDeChasse.chasseurs!![2].nom shouldBe "Robert"
            savedPartieDeChasse.chasseurs!![2].ballesRestantes shouldBe 12
            savedPartieDeChasse.chasseurs!![2].nbGalinettes shouldBe 0

            meilleurChasseur shouldBe "Brocouille"
        }

        scenario("quand les chasseurs sont à l'apéro et tous ex aequo") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                    this.nbGalinettes = 3
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                    this.nbGalinettes = 3
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                    this.nbGalinettes = 3
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.APÉRO
                        events = mutableListOf()
                    }

            )
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            val meilleurChasseur = service.terminerLaPartie(id)

            val savedPartieDeChasse = repository.savedPartieDeChasse!!
            savedPartieDeChasse.id shouldBe id
            savedPartieDeChasse.status shouldBe PartieStatus.TERMINÉE
            savedPartieDeChasse.terrain!!.nom shouldBe "Pitibon sur Sauldre"
            savedPartieDeChasse.terrain!!.nbGalinettes shouldBe 3
            savedPartieDeChasse.chasseurs!! shouldHaveSize 3
            savedPartieDeChasse.chasseurs!![0].nom shouldBe "Dédé"
            savedPartieDeChasse.chasseurs!![0].ballesRestantes shouldBe 20
            savedPartieDeChasse.chasseurs!![0].nbGalinettes shouldBe 3
            savedPartieDeChasse.chasseurs!![1].nom shouldBe "Bernard"
            savedPartieDeChasse.chasseurs!![1].ballesRestantes shouldBe 8
            savedPartieDeChasse.chasseurs!![1].nbGalinettes shouldBe 3
            savedPartieDeChasse.chasseurs!![2].nom shouldBe "Robert"
            savedPartieDeChasse.chasseurs!![2].ballesRestantes shouldBe 12
            savedPartieDeChasse.chasseurs!![2].nbGalinettes shouldBe 3

            meilleurChasseur shouldBe "Dédé, Bernard, Robert"
        }

        scenario("échoue si la partie est déjà terminée") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.TERMINÉE
                        events = mutableListOf()
                    }
            )
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            shouldThrow<QuandCestFiniCestFini> {
                service.reprendreLaPartie(id)
            }
            repository.savedPartieDeChasse shouldBe null
        }
    }

    feature("consulter status") {
        scenario("quand la partie vient de démarrer") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                    this.nbGalinettes = 3
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                    this.nbGalinettes = 3
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                    this.nbGalinettes = 3
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.EN_COURS
                        events = mutableListOf(
                            Event(
                                LocalDateTime.of(2024, APRIL, 25, 9, 0, 12),
                                "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"
                            )
                        )
                    }
            )
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            val status = service.consulterStatus(id)

            status shouldBe "09:00 - La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"
        }

        scenario("quand la partie est terminée") {
            val id = UUID.randomUUID()
            val repository = PartieDeChasseRepositoryForTests()

            repository.add(
                PartieDeChasse()
                    .apply {
                        this.id = id
                        chasseurs = mutableListOf(
                            Chasseur()
                                .apply {
                                    this.nom = "Dédé"
                                    this.ballesRestantes = 20
                                    this.nbGalinettes = 3
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Bernard"
                                    this.ballesRestantes = 8
                                    this.nbGalinettes = 3
                                },
                            Chasseur()
                                .apply {
                                    this.nom = "Robert"
                                    this.ballesRestantes = 12
                                    this.nbGalinettes = 3
                                }
                        )
                        terrain = Terrain("Pitibon sur Sauldre").apply { this.nbGalinettes = 3 }
                        status = PartieStatus.EN_COURS
                        events = mutableListOf(
                            Event(
                                LocalDateTime.of(2024, APRIL, 25, 9, 0, 12),
                                "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"
                            ),
                            Event(LocalDateTime.of(2024, APRIL, 25, 9, 10, 0), "Dédé tire"),
                            Event(LocalDateTime.of(2024, APRIL, 25, 9, 40, 0), "Robert tire sur une galinette"),
                            Event(LocalDateTime.of(2024, APRIL, 25, 10, 0, 0), "Petit apéro"),
                            Event(LocalDateTime.of(2024, APRIL, 25, 11, 0, 0), "Reprise de la chasse"),
                            Event(LocalDateTime.of(2024, APRIL, 25, 11, 2, 0), "Bernard tire"),
                            Event(LocalDateTime.of(2024, APRIL, 25, 11, 3, 0), "Bernard tire"),
                            Event(LocalDateTime.of(2024, APRIL, 25, 11, 4, 0), "Dédé tire sur une galinette"),
                            Event(LocalDateTime.of(2024, APRIL, 25, 11, 30, 0), "Robert tire sur une galinette"),
                            Event(LocalDateTime.of(2024, APRIL, 25, 11, 40, 0), "Petit apéro"),
                            Event(LocalDateTime.of(2024, APRIL, 25, 14, 30, 0), "Reprise de la chasse"),
                            Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 0), "Bernard tire"),
                            Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 1), "Bernard tire"),
                            Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 2), "Bernard tire"),
                            Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 3), "Bernard tire"),
                            Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 4), "Bernard tire"),
                            Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 5), "Bernard tire"),
                            Event(LocalDateTime.of(2024, APRIL, 25, 14, 41, 6), "Bernard tire"),
                            Event(
                                LocalDateTime.of(2024, APRIL, 25, 14, 41, 7),
                                "Bernard tire -> T'as plus de balles mon vieux, chasse à la main"
                            ),
                            Event(LocalDateTime.of(2024, APRIL, 25, 15, 0, 0), "Robert tire sur une galinette"),
                            Event(
                                LocalDateTime.of(2024, APRIL, 25, 15, 30, 0),
                                "La partie de chasse est terminée, vainqueur :  Robert - 3 galinettes"
                            )
                        )
                    }
            )
            val service = PartieDeChasseService(repository) { LocalDateTime.now() }

            val status = service.consulterStatus(id)

            status shouldBe "15:30 - La partie de chasse est terminée, vainqueur :  Robert - 3 galinettes" + lineSeparator() +
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
        }
    }
})