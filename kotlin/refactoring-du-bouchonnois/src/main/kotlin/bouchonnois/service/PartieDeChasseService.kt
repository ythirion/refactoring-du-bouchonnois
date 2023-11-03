package bouchonnois.service

import bouchonnois.domain.Chasseur
import bouchonnois.domain.Event
import bouchonnois.domain.PartieDeChasse
import bouchonnois.domain.PartieStatus
import bouchonnois.repository.PartieDeChasseRepository
import bouchonnois.service.exceptions.*
import java.time.LocalDateTime
import java.util.*

class PartieDeChasseService(
    private val repository: PartieDeChasseRepository,
    private val timeProvider: () -> LocalDateTime
) {
    fun démarrer(terrainDeChasse: Pair<String, Int>, chasseurs: Map<String, Int>): UUID {
        if (terrainDeChasse.second <= 0) {
            throw ImpossibleDeDémarrerUnePartieSansGalinettes()
        }

        val partieDeChasse = PartieDeChasse()
        partieDeChasse.id = UUID.randomUUID()
        partieDeChasse.status = PartieStatus.EN_COURS
        partieDeChasse.chasseurs = mutableListOf()

        val terrain = Terrain(terrainDeChasse.first)
        terrain.nbGalinettes = terrainDeChasse.second

        partieDeChasse.terrain = terrain
        partieDeChasse.events = mutableListOf()

        for (chasseur in chasseurs) {
            if (chasseur.value == 0) {
                throw ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle()
            }
            val chasseurToAdd = Chasseur()
            chasseurToAdd.nom = chasseur.key
            chasseurToAdd.ballesRestantes = chasseur.value

            partieDeChasse.chasseurs!!.add(chasseurToAdd)
        }

        if (partieDeChasse.chasseurs!!.isEmpty()) {
            throw ImpossibleDeDémarrerUnePartieSansChasseur()
        }

        val chasseursToString = partieDeChasse
            .chasseurs!!
            .joinToString(", ") { "${it.nom} (${it.ballesRestantes} balles)" }

        partieDeChasse.events!!.add(
            Event(
                timeProvider(),
                "La partie de chasse commence à ${partieDeChasse.terrain!!.nom} avec $chasseursToString"
            )
        )
        repository.save(partieDeChasse)

        return partieDeChasse.id!!
    }

    fun tirerSurUneGalinette(id: UUID, chasseur: String) {
        val partieDeChasse = repository.getById(id) ?: throw LaPartieDeChasseNexistePas()

        if (partieDeChasse.terrain!!.nbGalinettes != 0) {
            if (partieDeChasse.status != PartieStatus.APÉRO) {
                if (partieDeChasse.status != PartieStatus.TERMINÉE) {
                    if (partieDeChasse.chasseurs!!.any { c -> c.nom == chasseur }) {
                        val chasseurQuiTire = partieDeChasse.chasseurs!!.first { c -> c.nom == chasseur }

                        if (chasseurQuiTire.ballesRestantes == 0) {
                            partieDeChasse.events!!.add(
                                Event(
                                    timeProvider(),
                                    "$chasseur veut tirer sur une galinette -> T'as plus de balles mon vieux, chasse à la main"
                                )
                            )
                            repository.save(partieDeChasse)
                            throw TasPlusDeBallesMonVieuxChasseALaMain()
                        }
                        chasseurQuiTire.ballesRestantes -= 1
                        chasseurQuiTire.nbGalinettes += 1
                        partieDeChasse.terrain!!.nbGalinettes -= 1
                        partieDeChasse.events!!.add(
                            Event(
                                timeProvider(),
                                "$chasseur tire sur une galinette"
                            )
                        )
                    } else {
                        throw ChasseurInconnu(chasseur)
                    }
                } else {
                    partieDeChasse.events!!.add(
                        Event(
                            timeProvider(),
                            "$chasseur veut tirer -> On tire pas quand la partie est terminée"
                        )
                    )
                    repository.save(partieDeChasse)
                    throw OnTirePasQuandLaPartieEstTerminee()
                }
            } else {
                partieDeChasse
                    .events!!.add(
                        Event(
                            timeProvider(),
                            "$chasseur veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!"
                        )
                    )
                repository.save(partieDeChasse)
                throw OnTirePasPendantLapéroCestSacré()
            }
        } else {
            throw TasTropPicoledMonVieuxTasRienTouche()
        }

        repository.save(partieDeChasse)
    }
}