package bouchonnois.tests.doubles

import bouchonnois.domain.PartieDeChasse
import bouchonnois.repository.PartieDeChasseRepository
import java.util.*

class PartieDeChasseRepositoryForTests : PartieDeChasseRepository {
    var savedPartieDeChasse: PartieDeChasse? = null
    private val partiesDeChasse: Map<UUID, PartieDeChasse> = mutableMapOf()

    override fun save(partieDeChasse: PartieDeChasse) {
        savedPartieDeChasse = partieDeChasse
        partiesDeChasse + (partieDeChasse.id to partieDeChasse)
    }

    override fun getById(partieDeChasseId: UUID?): PartieDeChasse? =
        partiesDeChasse.getOrDefault(partieDeChasseId, null)
}