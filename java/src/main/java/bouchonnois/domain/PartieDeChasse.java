package bouchonnois.domain;

import bouchonnois.service.Terrain;
import lombok.Data;

import java.util.List;
import java.util.UUID;

@Data
public class PartieDeChasse {
    private UUID id;
    private List<Chasseur> chasseurs;
    private Terrain terrain;
    private PartieStatus status;
    private List<Event> events;
}
