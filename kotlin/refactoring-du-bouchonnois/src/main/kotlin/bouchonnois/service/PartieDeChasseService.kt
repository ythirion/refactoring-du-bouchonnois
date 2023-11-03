package bouchonnois.service

import bouchonnois.domain.Chasseur
import bouchonnois.domain.Event
import bouchonnois.domain.PartieDeChasse
import bouchonnois.domain.PartieStatus
import bouchonnois.repository.PartieDeChasseRepository
import bouchonnois.service.exceptions.ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle
import bouchonnois.service.exceptions.ImpossibleDeDémarrerUnePartieSansChasseur
import bouchonnois.service.exceptions.ImpossibleDeDémarrerUnePartieSansGalinettes
import java.time.LocalDateTime
import java.util.*

class PartieDeChasseService(
    private val repository: PartieDeChasseRepository,
    private val timeProvider: () -> LocalDateTime
) {
    fun démarrer(terrainDeChasse: Pair<String, Int>, chasseurs: List<Pair<String, Int>>): UUID {
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
            if (chasseur.second == 0) {
                throw ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle()
            }
            val chasseurToAdd = Chasseur()
            chasseurToAdd.nom = chasseur.first
            chasseurToAdd.ballesRestantes = chasseur.second

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
                ("La partie de chasse commence à " + partieDeChasse.terrain!!.nom).toString() + " avec " + chasseursToString
            )
        )
        repository.save(partieDeChasse)

        return partieDeChasse.id!!
    }
}