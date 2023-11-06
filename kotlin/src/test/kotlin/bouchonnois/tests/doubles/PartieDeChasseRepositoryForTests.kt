package bouchonnois.tests.doubles

import bouchonnois.domain.PartieDeChasse
import bouchonnois.repository.PartieDeChasseRepository
import java.util.*

class PartieDeChasseRepositoryForTests : PartieDeChasseRepository {
    var savedPartieDeChasse: PartieDeChasse? = null
    private val partiesDeChasse: MutableMap<UUID, PartieDeChasse> = mutableMapOf()

    override fun save(partieDeChasse: PartieDeChasse) {
        savedPartieDeChasse = partieDeChasse
        add(partieDeChasse)
    }

    override fun getById(partieDeChasseId: UUID?): PartieDeChasse? =
        partiesDeChasse[partieDeChasseId]

    fun add(partieDeChasse: PartieDeChasse) {
        partiesDeChasse.put(partieDeChasse.id!!, partieDeChasse)
    }
}