package bouchonnois.domain

import bouchonnois.service.Terrain
import java.util.*

class PartieDeChasse {
    var id: UUID? = null
    var chasseurs: MutableList<Chasseur>? = null
    var terrain: Terrain? = null
    var status: PartieStatus? = null
    var events: MutableList<Event>? = null
}