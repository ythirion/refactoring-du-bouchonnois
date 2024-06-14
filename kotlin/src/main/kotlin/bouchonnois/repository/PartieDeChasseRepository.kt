package bouchonnois.repository

import bouchonnois.domain.PartieDeChasse
import java.util.*

interface PartieDeChasseRepository {
    fun save(partieDeChasse: PartieDeChasse)
    fun getById(partieDeChasseId: UUID?): PartieDeChasse?
}