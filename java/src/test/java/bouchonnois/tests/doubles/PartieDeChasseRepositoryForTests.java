package bouchonnois.tests.doubles;

import bouchonnois.domain.PartieDeChasse;
import bouchonnois.repository.PartieDeChasseRepository;
import lombok.Getter;

import java.util.HashMap;
import java.util.Map;
import java.util.UUID;

public class PartieDeChasseRepositoryForTests implements PartieDeChasseRepository {
    private final Map<UUID, PartieDeChasse> partiesDeChasse = new HashMap<>();
    @Getter
    private PartieDeChasse savedPartieDeChasse;

    @Override
    public void save(PartieDeChasse partieDeChasse) {
        savedPartieDeChasse = partieDeChasse;
        partiesDeChasse.put(partieDeChasse.getId(), partieDeChasse);
    }

    @Override
    public PartieDeChasse getById(UUID partieDeChasseId) {
        return null;
    }
}
