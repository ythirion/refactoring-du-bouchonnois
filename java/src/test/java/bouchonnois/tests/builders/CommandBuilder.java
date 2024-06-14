package bouchonnois.tests.builders;

import io.vavr.Tuple;
import io.vavr.Tuple2;
import lombok.Getter;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public class CommandBuilder {
    @Getter
    private List<Tuple2<String, Integer>> chasseurs = new ArrayList<>();
    private int nbGalinettes;

    public static CommandBuilder demarrerUnePartieDeChasse() {
        return new CommandBuilder();
    }

    @SuppressWarnings("varargs")
    @SafeVarargs
    public final CommandBuilder avec(Tuple2<String, Integer>... chasseurs) {
        Collections.addAll(this.chasseurs, chasseurs);
        return this;
    }

    public CommandBuilder surUnTerrainRicheEnGalinettes() {
        this.nbGalinettes = 3;
        return this;
    }

    public Tuple2<String, Integer> getTerrain() {
        return Tuple.of("Pitibon sur Sauldre", nbGalinettes);
    }
}
