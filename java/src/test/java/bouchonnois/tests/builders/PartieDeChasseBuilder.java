package bouchonnois.tests.builders;

import bouchonnois.domain.Event;
import bouchonnois.domain.PartieDeChasse;
import bouchonnois.domain.PartieStatus;
import bouchonnois.service.Terrain;

import java.util.Arrays;
import java.util.UUID;
import java.util.stream.Collectors;

public class PartieDeChasseBuilder {
    private int nbGalinettes;
    private ChasseurBuilder[] chasseurs = {ChasseurBuilder.dédé(), ChasseurBuilder.bernard(), ChasseurBuilder.robert()};
    private PartieStatus status = PartieStatus.EN_COURS;
    private Event[] events = new Event[0];

    private PartieDeChasseBuilder(int nbGalinettes) {
        this.nbGalinettes = nbGalinettes;
    }

    public static PartieDeChasseBuilder surUnTerrainRicheEnGalinettes() {
        return new PartieDeChasseBuilder(3);
    }

    public static PartieDeChasseBuilder surUnTerrainSansGalinettes() {
        return new PartieDeChasseBuilder(0);
    }

    public static UUID unePartieDeChasseInexistante() {
        return UUID.randomUUID();
    }

    public PartieDeChasseBuilder avec(ChasseurBuilder... chasseurs) {
        this.chasseurs = chasseurs;
        return this;
    }

    public PartieDeChasseBuilder aLapéro() {
        this.status = PartieStatus.APÉRO;
        return this;
    }

    public PartieDeChasseBuilder terminée() {
        this.status = PartieStatus.TERMINÉE;
        return this;
    }

    public PartieDeChasseBuilder events(Event... events) {
        this.events = events;
        return this;
    }

    public PartieDeChasse build() {
        var chasseurList = Arrays.stream(this.chasseurs)
                .map(ChasseurBuilder::build)
                .collect(Collectors.toList());

        var eventList = Arrays.asList(this.events);

        var terrain = new Terrain("Pitibon sur Sauldre");
        terrain.setNbGalinettes(this.nbGalinettes);

        return new PartieDeChasse(
                UUID.randomUUID(),
                terrain,
                chasseurList,
                eventList,
                this.status
        );
    }
}
