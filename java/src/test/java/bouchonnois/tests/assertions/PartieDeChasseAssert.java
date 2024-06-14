package bouchonnois.tests.assertions;

import bouchonnois.domain.Chasseur;
import bouchonnois.domain.Event;
import bouchonnois.domain.PartieDeChasse;
import bouchonnois.domain.PartieStatus;
import org.assertj.core.api.AbstractAssert;
import org.assertj.core.api.Assertions;

import java.time.LocalDateTime;
import java.util.function.Consumer;

import static java.lang.String.format;

public class PartieDeChasseAssert extends AbstractAssert<PartieDeChasseAssert, PartieDeChasse> {
    public PartieDeChasseAssert(PartieDeChasse actual) {
        super(actual, PartieDeChasseAssert.class);
    }

    public static PartieDeChasseAssert assertPartieDeChasse(PartieDeChasse actual) {
        return new PartieDeChasseAssert(actual);
    }

    public PartieDeChasseAssert hasEmittedEvent(LocalDateTime expectedTime, String expectedMessage) {
        return assertThat(p -> {
                    var expectedEvent = new Event(expectedTime, expectedMessage);
                    var errorMessage = format("Expected event to be <%s> but was not.", expectedEvent);

                    Assertions.assertThat(p.getEvents())
                            .overridingErrorMessage(errorMessage)
                            .contains(expectedEvent);
                }
        );
    }

    public PartieDeChasseAssert chasseurATireSurUneGalinette(String nom, int ballesRestantes, int galinettes) {
        return assertThat(p -> {
            var foundChasseur = findChasseurByName(p, nom);
            assertBallesRestantes(foundChasseur, ballesRestantes);
            Assertions.assertThat(foundChasseur.getNbGalinettes())
                    .overridingErrorMessage("Le nombre de galinettes capturées par %s devrait être de %d", foundChasseur.getNom(), galinettes)
                    .isEqualTo(galinettes);
        });
    }

    public PartieDeChasseAssert chasseurATiré(String nom, int ballesRestantes) {
        return assertThat(p -> assertBallesRestantes(findChasseurByName(p, nom), ballesRestantes));
    }

    public PartieDeChasseAssert galinettesSurLeTerrain(int nbGalinettes) {
        return assertThat(p -> Assertions.assertThat(p.getTerrain().getNbGalinettes())
                .overridingErrorMessage("Le terrain devrait contenir %d mais en contient %d", nbGalinettes, actual.getTerrain().getNbGalinettes())
                .isEqualTo(nbGalinettes));
    }

    public PartieDeChasseAssert beInApéro() {
        return assertThat(p -> Assertions.assertThat(p.getStatus())
                .overridingErrorMessage("Les chasseurs devraient être à l'apéro")
                .isEqualTo(PartieStatus.APÉRO)
        );
    }

    public PartieDeChasseAssert beEnCours() {
        return assertThat(p -> Assertions.assertThat(p.getStatus())
                .overridingErrorMessage("Les chasseurs devraient être en cours de chasse")
                .isEqualTo(PartieStatus.EN_COURS)
        );
    }

    private static Chasseur findChasseurByName(PartieDeChasse partieDeChasse, String nom) {
        return partieDeChasse.getChasseurs().stream()
                .filter(c -> c.getNom().equals(nom))
                .findFirst()
                .orElseThrow(() -> new AssertionError("Chasseur non présent dans la partie de chasse"));
    }

    private static void assertBallesRestantes(Chasseur foundChasseur, int ballesRestantes) {
        Assertions.assertThat(foundChasseur.getBallesRestantes())
                .overridingErrorMessage("Le nombre de balles restantes pour %s devrait être de %d balle(s)", foundChasseur.getNom(), ballesRestantes)
                .isEqualTo(ballesRestantes);
    }

    private PartieDeChasseAssert assertThat(Consumer<PartieDeChasse> assertion) {
        assertion.accept(actual);
        return this;
    }
}