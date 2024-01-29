package bouchonnois.tests.unit;

import bouchonnois.domain.PartieDeChasse;
import bouchonnois.tests.doubles.PartieDeChasseRepositoryForTests;

import static org.assertj.core.api.Assertions.assertThat;

abstract class PartieDeChasseServiceTests {
    protected static void assertPartieDeChasseHasBeenSaved(PartieDeChasseRepositoryForTests repository, PartieDeChasse partieDeChasse) {
        assertThat(repository.getSavedPartieDeChasse())
                .isEqualTo(partieDeChasse);
    }
}