package bouchonnois.domain;

import bouchonnois.service.Terrain;
import lombok.Getter;
import lombok.Setter;

import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

@Getter
@Setter
public class PartieDeChasse {
    private UUID id;
    private List<Chasseur> chasseurs;
    private Terrain terrain;
    private PartieStatus status;
    private ArrayList<Event> events;

    public PartieDeChasse() {
    }

    public PartieDeChasse(UUID id, Terrain terrain) {
        this.id = id;
        this.chasseurs = new ArrayList<>();
        this.terrain = terrain;
        this.status = PartieStatus.EN_COURS;
        this.events = new ArrayList<>();
    }

    public PartieDeChasse(UUID id, Terrain terrain, List<Chasseur> chasseurs) {
        this(id, terrain);
        this.chasseurs = chasseurs;
    }

    public PartieDeChasse(UUID id,
                          Terrain terrain,
                          List<Chasseur> chasseurs,
                          List<Event> events,
                          PartieStatus status) {
        this(id, terrain, chasseurs, status);
        this.events = new ArrayList<>(events);
    }

    public PartieDeChasse(UUID id, Terrain terrain, List<Chasseur> chasseurs, PartieStatus status) {
        this(id, terrain, chasseurs);
        this.status = status;
    }
}
