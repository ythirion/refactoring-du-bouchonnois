package bouchonnois.service;

import bouchonnois.domain.Chasseur;
import bouchonnois.domain.Event;
import bouchonnois.domain.PartieDeChasse;
import bouchonnois.domain.PartieStatus;
import bouchonnois.repository.PartieDeChasseRepository;
import bouchonnois.service.exceptions.*;
import io.vavr.Tuple2;
import lombok.AllArgsConstructor;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.Comparator;
import java.util.List;
import java.util.UUID;
import java.util.function.Supplier;
import java.util.stream.Collectors;

@AllArgsConstructor
public class PartieDeChasseService {
    private final PartieDeChasseRepository repository;
    private final Supplier<LocalDateTime> timeProvider;

    public UUID démarrer(Tuple2<String, Integer> terrainDeChasse, List<Tuple2<String, Integer>> chasseurs) throws ImpossibleDeDémarrerUnePartieSansGalinettes, ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle, ImpossibleDeDémarrerUnePartieSansChasseur {
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
                throw new ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle();
            }
            Chasseur chasseurToAdd = new Chasseur();
            chasseurToAdd.setNom(chasseur._1);
            chasseurToAdd.setBallesRestantes(chasseur._2);
            partieDeChasse.getChasseurs().add(chasseurToAdd);
        }

        if (partieDeChasse.getChasseurs().isEmpty()) {
            throw new ImpossibleDeDémarrerUnePartieSansChasseur();
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

    public void tirerSurUneGalinette(UUID id, String chasseur) throws LaPartieDeChasseNexistePas, TasPlusDeBallesMonVieuxChasseALaMain, ChasseurInconnu, OnTirePasPendantLapéroCestSacré, TasTropPicoledMonVieuxTasRienTouche, OnTirePasQuandLaPartieEstTerminee {
        PartieDeChasse partieDeChasse = repository.getById(id);

        if (partieDeChasse == null) {
            throw new LaPartieDeChasseNexistePas();
        }

        if (partieDeChasse.getTerrain().getNbGalinettes() != 0) {

            if (partieDeChasse.getStatus() != PartieStatus.APÉRO) {
                if (partieDeChasse.getStatus() != PartieStatus.TERMINÉE) {
                    if (partieDeChasse.getChasseurs().stream().anyMatch(c -> c.getNom().equals(chasseur))) {
                        var chasseurQuiTire = partieDeChasse.getChasseurs().stream()
                                .filter(c -> c.getNom().equals(chasseur)).findFirst()
                                .get();

                        if (chasseurQuiTire.getBallesRestantes() == 0) {
                            partieDeChasse.getEvents().add(new Event(timeProvider.get(),
                                    chasseur + " veut tirer sur une galinette -> T'as plus de balles mon vieux, chasse à la main"));
                            repository.save(partieDeChasse);

                            throw new TasPlusDeBallesMonVieuxChasseALaMain();
                        }

                        chasseurQuiTire.setBallesRestantes(chasseurQuiTire.getBallesRestantes() - 1);
                        chasseurQuiTire.setNbGalinettes(chasseurQuiTire.getNbGalinettes() + 1);
                        partieDeChasse.getTerrain().setNbGalinettes(partieDeChasse.getTerrain().getNbGalinettes() - 1);
                        partieDeChasse.getEvents().add(new Event(timeProvider.get(), chasseur + " tire sur une galinette"));

                    } else {
                        throw new ChasseurInconnu(chasseur);
                    }
                } else {
                    partieDeChasse.getEvents().add(new Event(timeProvider.get(), chasseur + " veut tirer -> On tire pas quand la partie est terminée"));
                    repository.save(partieDeChasse);

                    throw new OnTirePasQuandLaPartieEstTerminee();
                }
            } else {
                partieDeChasse.getEvents()
                        .add(new Event(timeProvider.get(), chasseur + " veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!"));
                repository.save(partieDeChasse);
                throw new OnTirePasPendantLapéroCestSacré();
            }
        } else {
            throw new TasTropPicoledMonVieuxTasRienTouche();
        }

        repository.save(partieDeChasse);
    }

    public void tirer(UUID id, String chasseur) throws LaPartieDeChasseNexistePas, TasPlusDeBallesMonVieuxChasseALaMain, ChasseurInconnu, OnTirePasPendantLapéroCestSacré, TasTropPicoledMonVieuxTasRienTouche, OnTirePasQuandLaPartieEstTerminee {
        PartieDeChasse partieDeChasse = repository.getById(id);

        if (partieDeChasse == null) {
            throw new LaPartieDeChasseNexistePas();
        }
        if (partieDeChasse.getStatus() != PartieStatus.APÉRO) {
            if (partieDeChasse.getStatus() != PartieStatus.TERMINÉE) {
                if (partieDeChasse.getChasseurs().stream().anyMatch(c -> c.getNom().equals(chasseur))) {
                    var chasseurQuiTire = partieDeChasse.getChasseurs()
                            .stream()
                            .filter(c -> c.getNom().equals(chasseur))
                            .findFirst()
                            .get();

                    if (chasseurQuiTire.getBallesRestantes() == 0) {
                        partieDeChasse.getEvents().add(new Event(timeProvider.get(),
                                chasseur + " tire -> T'as plus de balles mon vieux, chasse à la main"));
                        repository.save(partieDeChasse);

                        throw new TasPlusDeBallesMonVieuxChasseALaMain();
                    }

                    chasseurQuiTire.setBallesRestantes(chasseurQuiTire.getBallesRestantes() - 1);
                    partieDeChasse.getEvents().add(new Event(timeProvider.get(), chasseur + " tire"));

                } else {
                    throw new ChasseurInconnu(chasseur);
                }
            } else {
                partieDeChasse.getEvents().add(new Event(timeProvider.get(), chasseur + " veut tirer -> On tire pas quand la partie est terminée"));
                repository.save(partieDeChasse);

                throw new OnTirePasQuandLaPartieEstTerminee();
            }
        } else {
            partieDeChasse.getEvents().add(new Event(timeProvider.get(), chasseur + " veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!"));
            repository.save(partieDeChasse);
            throw new OnTirePasPendantLapéroCestSacré();
        }
        repository.save(partieDeChasse);
    }

    public void prendreLapéro(UUID id) throws LaPartieDeChasseNexistePas, OnEstDéjaEnTrainDePrendreLapéro, OnPrendPasLapéroQuandLaPartieEstTerminée {
        PartieDeChasse partieDeChasse = repository.getById(id);

        if (partieDeChasse == null) {
            throw new LaPartieDeChasseNexistePas();
        }

        if (partieDeChasse.getStatus() == PartieStatus.APÉRO) {
            throw new OnEstDéjaEnTrainDePrendreLapéro();
        } else if (partieDeChasse.getStatus() == PartieStatus.TERMINÉE) {
            throw new OnPrendPasLapéroQuandLaPartieEstTerminée();
        } else {
            partieDeChasse.setStatus(PartieStatus.APÉRO);
            partieDeChasse.getEvents().add(new Event(timeProvider.get(), "Petit apéro"));
            repository.save(partieDeChasse);
        }
    }

    public void reprendreLaPartie(UUID id) throws LaPartieDeChasseNexistePas, LaChasseEstDéjaEnCours, QuandCestFiniCestFini {
        PartieDeChasse partieDeChasse = repository.getById(id);

        if (partieDeChasse == null) {
            throw new LaPartieDeChasseNexistePas();
        }

        if (partieDeChasse.getStatus() == PartieStatus.EN_COURS) {
            throw new LaChasseEstDéjaEnCours();
        }

        if (partieDeChasse.getStatus() == PartieStatus.TERMINÉE) {
            throw new QuandCestFiniCestFini();
        }

        partieDeChasse.setStatus(PartieStatus.EN_COURS);
        partieDeChasse.getEvents().add(new Event(timeProvider.get(), "Reprise de la chasse"));
        repository.save(partieDeChasse);
    }

    public String terminerLaPartie(UUID id) throws QuandCestFiniCestFini {
        PartieDeChasse partieDeChasse = repository.getById(id);

        var classement = partieDeChasse
                .getChasseurs()
                .stream()
                .collect(Collectors.groupingBy(Chasseur::getNbGalinettes));

        if (partieDeChasse.getStatus() == PartieStatus.TERMINÉE) {
            throw new QuandCestFiniCestFini();
        }

        partieDeChasse.setStatus(PartieStatus.TERMINÉE);

        String result = "";

        if (classement.entrySet().stream().allMatch(entry -> entry.getKey() == 0)) {
            result = "Brocouille";
            partieDeChasse.getEvents().add(new Event(timeProvider.get(), "La partie de chasse est terminée, vainqueur : Brocouille"));
        } else {
            var winners = classement.get(classement.keySet().stream()
                    .max(Integer::compare)
                    .orElse(0));

            result = winners.stream()
                    .map(Chasseur::getNom)
                    .collect(Collectors.joining(", "));

            partieDeChasse.getEvents().add(new Event(timeProvider.get(), "La partie de chasse est terminée, vainqueur : " +
                    winners.stream()
                            .map(chasseur -> chasseur.getNom() + " - " + chasseur.getNbGalinettes() + " galinettes")
                            .collect(Collectors.joining(", "))));
        }
        repository.save(partieDeChasse);

        return result;
    }

    public String consulterStatus(UUID id) throws LaPartieDeChasseNexistePas {
        PartieDeChasse partieDeChasse = repository.getById(id);

        if (partieDeChasse == null) {
            throw new LaPartieDeChasseNexistePas();
        }

        List<Event> events = partieDeChasse.getEvents();
        events.sort(Comparator.comparing(Event::date).reversed());

        return events.stream()
                .map(Event::toString)
                .collect(Collectors.joining(System.lineSeparator()));
    }
}