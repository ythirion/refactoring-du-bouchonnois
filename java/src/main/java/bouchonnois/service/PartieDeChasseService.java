package bouchonnois.service;

import bouchonnois.domain.Chasseur;
import bouchonnois.domain.Event;
import bouchonnois.domain.PartieDeChasse;
import bouchonnois.domain.PartieStatus;
import bouchonnois.repository.PartieDeChasseRepository;
import bouchonnois.service.exceptions.ImpossibleDeDemarrerUnePartieAvecUnChasseurSansBalle;
import bouchonnois.service.exceptions.ImpossibleDeDemarrerUnePartieSansChasseur;
import bouchonnois.service.exceptions.ImpossibleDeDémarrerUnePartieSansGalinettes;
import io.vavr.Tuple2;
import lombok.AllArgsConstructor;

import java.time.LocalDate;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;
import java.util.function.Supplier;

@AllArgsConstructor
public class PartieDeChasseService {
    private final PartieDeChasseRepository repository;
    private final Supplier<LocalDate> timeProvider;

    public UUID démarrer(Tuple2<String, Integer> terrainDeChasse, List<Tuple2<String, Integer>> chasseurs) throws ImpossibleDeDémarrerUnePartieSansGalinettes, ImpossibleDeDemarrerUnePartieAvecUnChasseurSansBalle, ImpossibleDeDemarrerUnePartieSansChasseur {
        if (terrainDeChasse._2 <= 0) {
            throw new ImpossibleDeDémarrerUnePartieSansGalinettes();
        }

        PartieDeChasse partieDeChasse = new PartieDeChasse();
        partieDeChasse.setId(UUID.randomUUID());
        partieDeChasse.setStatus(PartieStatus.EN_COURS);
        partieDeChasse.setChasseurs(new ArrayList<>());

        Terrain terrain = new Terrain(terrainDeChasse._1);
        terrain.setNbGalinettes(terrainDeChasse._2);
        partieDeChasse.setTerrain(terrain);
        partieDeChasse.setEvents(new ArrayList<>());

        for (var chasseur : chasseurs) {
            if (chasseur._2 == 0) {
                throw new ImpossibleDeDemarrerUnePartieAvecUnChasseurSansBalle();
            }
            Chasseur chasseurToAdd = new Chasseur();
            chasseurToAdd.setNom(chasseur._1);
            chasseurToAdd.setBallesRestantes(chasseur._2);
            partieDeChasse.getChasseurs().add(chasseurToAdd);
        }

        if (partieDeChasse.getChasseurs().isEmpty()) {
            throw new ImpossibleDeDemarrerUnePartieSansChasseur();
        }

        String chasseursToString = String.join(", ",
                partieDeChasse.getChasseurs().stream()
                        .map(c -> c.getNom() + " (" + c.getBallesRestantes() + " balles)")
                        .toArray(String[]::new));

        partieDeChasse.getEvents().add(new Event(timeProvider.get(),
                "La partie de chasse commence à " + partieDeChasse.getTerrain().getNom() + " avec " + chasseursToString));

        repository.save(partieDeChasse);

        return partieDeChasse.getId();
    }
}