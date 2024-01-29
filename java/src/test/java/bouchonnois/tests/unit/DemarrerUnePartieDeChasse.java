package bouchonnois.tests.unit;

import bouchonnois.domain.PartieStatus;
import bouchonnois.service.PartieDeChasseService;
import bouchonnois.service.exceptions.ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle;
import bouchonnois.service.exceptions.ImpossibleDeDémarrerUnePartieSansChasseur;
import bouchonnois.service.exceptions.ImpossibleDeDémarrerUnePartieSansGalinettes;
import bouchonnois.tests.doubles.PartieDeChasseRepositoryForTests;
import io.vavr.Tuple2;
import org.junit.jupiter.api.Test;

import java.time.LocalDateTime;
import java.util.ArrayList;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;

class DemarrerUnePartieDeChasse {
    @Test
    void avec_plusieurs_chasseurs() throws ImpossibleDeDémarrerUnePartieSansGalinettes, ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle, ImpossibleDeDémarrerUnePartieSansChasseur {
        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, LocalDateTime::now);
        var chasseurs = new ArrayList<Tuple2<String, Integer>>() {{
            add(new Tuple2<>("Dédé", 20));
            add(new Tuple2<>("Bernard", 8));
            add(new Tuple2<>("Robert", 12));
        }};
        var terrainDeChasse = new Tuple2<>("Pitibon sur Sauldre", 3);

        var id = service.démarrer(
                terrainDeChasse,
                chasseurs
        );

        var savedPartieDeChasse = repository.getSavedPartieDeChasse();
        assertThat(savedPartieDeChasse.getId()).isNotNull().isEqualTo(id);
        assertThat(savedPartieDeChasse.getStatus()).isEqualTo(PartieStatus.EN_COURS);
        assertThat(savedPartieDeChasse.getTerrain().getNom()).isEqualTo("Pitibon sur Sauldre");
        assertThat(savedPartieDeChasse.getTerrain().getNbGalinettes()).isEqualTo(3);
        assertThat(savedPartieDeChasse.getChasseurs()).hasSize(3);
        assertThat(savedPartieDeChasse.getChasseurs().get(0).getNom()).isEqualTo("Dédé");
        assertThat(savedPartieDeChasse.getChasseurs().get(0).getBallesRestantes()).isEqualTo(20);
        assertThat(savedPartieDeChasse.getChasseurs().get(0).getNbGalinettes()).isZero();
        assertThat(savedPartieDeChasse.getChasseurs().get(1).getNom()).isEqualTo("Bernard");
        assertThat(savedPartieDeChasse.getChasseurs().get(1).getBallesRestantes()).isEqualTo(8);
        assertThat(savedPartieDeChasse.getChasseurs().get(1).getNbGalinettes()).isZero();
        assertThat(savedPartieDeChasse.getChasseurs().get(2).getNom()).isEqualTo("Robert");
        assertThat(savedPartieDeChasse.getChasseurs().get(2).getBallesRestantes()).isEqualTo(12);
        assertThat(savedPartieDeChasse.getChasseurs().get(2).getNbGalinettes()).isZero();
    }

    @Test
    void echoue_sans_chasseurs() {
        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, LocalDateTime::now);
        var chasseurs = new ArrayList<Tuple2<String, Integer>>();
        var terrainDeChasse = new Tuple2<>("Pitibon sur Sauldre", 3);

        assertThatThrownBy(() -> service.démarrer(terrainDeChasse, chasseurs))
                .isInstanceOf(ImpossibleDeDémarrerUnePartieSansChasseur.class);
        assertThat(repository.getSavedPartieDeChasse()).isNull();
    }

    @Test
    void echoue_avec_un_terrain_sans_galinettes() {
        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, LocalDateTime::now);
        var chasseurs = new ArrayList<Tuple2<String, Integer>>();
        var terrainDeChasse = new Tuple2<>("Pitibon sur Sauldre", 0);

        assertThatThrownBy(() -> service.démarrer(terrainDeChasse, chasseurs))
                .isInstanceOf(ImpossibleDeDémarrerUnePartieSansGalinettes.class);
        assertThat(repository.getSavedPartieDeChasse()).isNull();
    }

    @Test
    void echoue_si_un_chasseur_sans_balle() {
        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, LocalDateTime::now);
        var chasseurs = new ArrayList<Tuple2<String, Integer>>() {{
            add(new Tuple2<>("Dédé", 20));
            add(new Tuple2<>("Bernard", 0));
        }};
        var terrainDeChasse = new Tuple2<>("Pitibon sur Sauldre", 3);

        assertThatThrownBy(() -> service.démarrer(terrainDeChasse, chasseurs))
                .isInstanceOf(ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle.class);
        assertThat(repository.getSavedPartieDeChasse()).isNull();
    }
}