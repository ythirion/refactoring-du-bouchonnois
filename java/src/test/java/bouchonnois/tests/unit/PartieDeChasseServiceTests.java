package bouchonnois.tests.unit;

import bouchonnois.domain.PartieDeChasse;
import bouchonnois.service.PartieDeChasseService;
import bouchonnois.tests.builders.PartieDeChasseBuilder;
import bouchonnois.tests.doubles.PartieDeChasseRepositoryForTests;
import io.vavr.Function0;
import org.junit.jupiter.api.DisplayNameGeneration;

import java.time.LocalDateTime;
import java.util.UUID;
import java.util.function.Consumer;

import static org.assertj.core.api.Assertions.assertThatThrownBy;
import static org.junit.jupiter.api.DisplayNameGenerator.ReplaceUnderscores;

@DisplayNameGeneration(ReplaceUnderscores.class)
abstract class PartieDeChasseServiceTests {
    protected static final String BERNARD = "Bernard";
    protected static final String ROBERT = "Robert";
    protected static final String DÉDÉ = "Dédé";
    protected static final String CHASSEUR_INCONNU = "Chasseur inconnu";
    protected static final LocalDateTime now = LocalDateTime.of(2024, 6, 6, 14, 50, 45);
    private static final Function0<LocalDateTime> timeProvider = () -> now;
    protected PartieDeChasseRepositoryForTests repository;
    protected PartieDeChasseService partieDeChasseService;
    private UUID partieDeChasseId;
    private Consumer<UUID> act;

    protected PartieDeChasseServiceTests() {
        repository = new PartieDeChasseRepositoryForTests();
        partieDeChasseService = new PartieDeChasseService(repository, timeProvider);
    }

    protected PartieDeChasse unePartieDeChasseExistante(PartieDeChasseBuilder partieDeChasseBuilder) {
        var partieDeChasse = partieDeChasseBuilder.build();
        repository.add(partieDeChasse);

        return partieDeChasse;
    }

    protected void given(UUID partieDeChasseId) {
        this.partieDeChasseId = partieDeChasseId;
    }

    protected void given(PartieDeChasse unePartieDeChasseExistante) {
        given(unePartieDeChasseExistante.getId());
    }

    protected void when(Consumer<UUID> act) {
        this.act = act;
    }

    protected void then(Consumer<PartieDeChasse> assertion, Runnable assertResult) {
        act.accept(partieDeChasseId);
        assertion.accept(repository.getSavedPartieDeChasse());

        if (assertResult != null) {
            assertResult.run();
        }
    }

    protected void then(Consumer<PartieDeChasse> assertResult) {
        act.accept(partieDeChasseId);
        if (assertResult != null) {
            assertResult.accept(repository.getSavedPartieDeChasse());
        }
    }

    protected <E extends Exception> void thenThrow(Class<E> exceptionClass, Consumer<PartieDeChasse> assertion) {
        thenThrow(exceptionClass, assertion, null);
    }

    protected <E extends Exception> void thenThrow(Class<E> exceptionClass, Consumer<PartieDeChasse> assertion, String expectedMessage) {
        var ex = assertThatThrownBy(() -> act.accept(partieDeChasseId))
                .isExactlyInstanceOf(exceptionClass);

        if (expectedMessage != null)
            ex.hasMessage(expectedMessage);

        assertion.accept(repository.getSavedPartieDeChasse());
    }

    protected <E extends Exception> void executeAndAssertThrow(
            Class<E> exceptionClass,
            Consumer<PartieDeChasseService> act,
            Consumer<PartieDeChasse> assertion) {
        assertThatThrownBy(() -> act.accept(partieDeChasseService)).isExactlyInstanceOf(exceptionClass);
        assertion.accept(repository.getSavedPartieDeChasse());
    }
}