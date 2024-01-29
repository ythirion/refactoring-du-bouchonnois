package bouchonnois.tests.unit;

import bouchonnois.domain.Chasseur;
import bouchonnois.domain.PartieDeChasse;
import bouchonnois.domain.PartieStatus;
import bouchonnois.service.PartieDeChasseService;
import bouchonnois.service.Terrain;
import bouchonnois.service.exceptions.QuandCestFiniCestFini;
import bouchonnois.tests.doubles.PartieDeChasseRepositoryForTests;
import org.junit.jupiter.api.Test;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;

public class TerminerLaPartieDeChasse extends PartieDeChasseServiceTests {
    @Test
    void quand_la_partie_est_en_cours_et_1_seul_chasseur_gagne() throws QuandCestFiniCestFini {
        var id = UUID.randomUUID();
        var repository = new PartieDeChasseRepositoryForTests();

        repository.add(new PartieDeChasse() {
            {
                setId(id);
                setChasseurs(new ArrayList<>() {{
                    add(new Chasseur() {{
                        setNom("Dédé");
                        setBallesRestantes(20);
                    }});
                    add(new Chasseur() {{
                        setNom("Bernard");
                        setBallesRestantes(8);
                    }});
                    add(new Chasseur() {{
                        setNom("Robert");
                        setBallesRestantes(12);
                        setNbGalinettes(2);
                    }});
                }});

                setTerrain(new Terrain("Pitibon sur Sauldre") {{
                    setNbGalinettes(3);
                }});
                setStatus(PartieStatus.EN_COURS);
                setEvents(new ArrayList<>());
            }
        });
        var service = new PartieDeChasseService(repository, LocalDateTime::now);

        var meilleurChasseur = service.terminerLaPartie(id);

        var savedPartieDeChasse = repository.getSavedPartieDeChasse();
        assertThat(savedPartieDeChasse.getId()).isEqualTo(id);
        assertThat(savedPartieDeChasse.getStatus()).isEqualTo(PartieStatus.TERMINÉE);
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
        assertThat(savedPartieDeChasse.getChasseurs().get(2).getNbGalinettes()).isEqualTo(2);

        assertThat(meilleurChasseur).isEqualTo("Robert");
    }

    @Test
    void quand_la_partie_est_en_cours_et_1_seul_chasseur_dans_la_partie() throws QuandCestFiniCestFini {
        var id = UUID.randomUUID();
        var repository = new PartieDeChasseRepositoryForTests();

        repository.add(new PartieDeChasse() {
            {
                setId(id);
                setChasseurs(new ArrayList<>() {{
                    add(new Chasseur() {{
                        setNom("Robert");
                        setBallesRestantes(12);
                        setNbGalinettes(2);
                    }});
                }});

                setTerrain(new Terrain("Pitibon sur Sauldre") {{
                    setNbGalinettes(3);
                }});
                setStatus(PartieStatus.EN_COURS);
                setEvents(new ArrayList<>());
            }
        });
        var service = new PartieDeChasseService(repository, LocalDateTime::now);

        var meilleurChasseur = service.terminerLaPartie(id);

        var savedPartieDeChasse = repository.getSavedPartieDeChasse();
        assertThat(savedPartieDeChasse.getId()).isEqualTo(id);
        assertThat(savedPartieDeChasse.getStatus()).isEqualTo(PartieStatus.TERMINÉE);
        assertThat(savedPartieDeChasse.getTerrain().getNom()).isEqualTo("Pitibon sur Sauldre");
        assertThat(savedPartieDeChasse.getTerrain().getNbGalinettes()).isEqualTo(3);
        assertThat(savedPartieDeChasse.getChasseurs()).hasSize(1);
        assertThat(savedPartieDeChasse.getChasseurs().get(0).getNom()).isEqualTo("Robert");
        assertThat(savedPartieDeChasse.getChasseurs().get(0).getBallesRestantes()).isEqualTo(12);
        assertThat(savedPartieDeChasse.getChasseurs().get(0).getNbGalinettes()).isEqualTo(2);

        assertThat(meilleurChasseur).isEqualTo("Robert");
    }

    @Test
    void quand_la_partie_est_en_cours_et_2_chasseurs_ex_aequo() throws QuandCestFiniCestFini {
        var id = UUID.randomUUID();
        var repository = new PartieDeChasseRepositoryForTests();

        repository.add(new PartieDeChasse() {
            {
                setId(id);
                setChasseurs(new ArrayList<>() {{
                    add(new Chasseur() {{
                        setNom("Dédé");
                        setBallesRestantes(20);
                        setNbGalinettes(2);
                    }});
                    add(new Chasseur() {{
                        setNom("Bernard");
                        setBallesRestantes(8);
                        setNbGalinettes(2);
                    }});
                    add(new Chasseur() {{
                        setNom("Robert");
                        setBallesRestantes(12);
                    }});
                }});

                setTerrain(new Terrain("Pitibon sur Sauldre") {{
                    setNbGalinettes(3);
                }});
                setStatus(PartieStatus.EN_COURS);
                setEvents(new ArrayList<>());
            }
        });
        var service = new PartieDeChasseService(repository, LocalDateTime::now);

        var meilleurChasseur = service.terminerLaPartie(id);

        var savedPartieDeChasse = repository.getSavedPartieDeChasse();
        assertThat(savedPartieDeChasse.getId()).isEqualTo(id);
        assertThat(savedPartieDeChasse.getStatus()).isEqualTo(PartieStatus.TERMINÉE);
        assertThat(savedPartieDeChasse.getTerrain().getNom()).isEqualTo("Pitibon sur Sauldre");
        assertThat(savedPartieDeChasse.getTerrain().getNbGalinettes()).isEqualTo(3);
        assertThat(savedPartieDeChasse.getChasseurs()).hasSize(3);
        assertThat(savedPartieDeChasse.getChasseurs().get(0).getNom()).isEqualTo("Dédé");
        assertThat(savedPartieDeChasse.getChasseurs().get(0).getBallesRestantes()).isEqualTo(20);
        assertThat(savedPartieDeChasse.getChasseurs().get(0).getNbGalinettes()).isEqualTo(2);
        assertThat(savedPartieDeChasse.getChasseurs().get(1).getNom()).isEqualTo("Bernard");
        assertThat(savedPartieDeChasse.getChasseurs().get(1).getBallesRestantes()).isEqualTo(8);
        assertThat(savedPartieDeChasse.getChasseurs().get(1).getNbGalinettes()).isEqualTo(2);
        assertThat(savedPartieDeChasse.getChasseurs().get(2).getNom()).isEqualTo("Robert");
        assertThat(savedPartieDeChasse.getChasseurs().get(2).getBallesRestantes()).isEqualTo(12);
        assertThat(savedPartieDeChasse.getChasseurs().get(2).getNbGalinettes()).isZero();

        assertThat(meilleurChasseur).isEqualTo("Dédé, Bernard");
    }

    @Test
    void quand_la_partie_est_en_cours_et_tout_le_monde_brocouille() throws QuandCestFiniCestFini {
        var id = UUID.randomUUID();
        var repository = new PartieDeChasseRepositoryForTests();

        repository.add(new PartieDeChasse() {
            {
                setId(id);
                setChasseurs(new ArrayList<>() {{
                    add(new Chasseur() {{
                        setNom("Dédé");
                        setBallesRestantes(20);
                    }});
                    add(new Chasseur() {{
                        setNom("Bernard");
                        setBallesRestantes(8);
                    }});
                    add(new Chasseur() {{
                        setNom("Robert");
                        setBallesRestantes(12);
                    }});
                }});

                setTerrain(new Terrain("Pitibon sur Sauldre") {{
                    setNbGalinettes(3);
                }});
                setStatus(PartieStatus.EN_COURS);
                setEvents(new ArrayList<>());
            }
        });
        var service = new PartieDeChasseService(repository, LocalDateTime::now);

        var meilleurChasseur = service.terminerLaPartie(id);

        var savedPartieDeChasse = repository.getSavedPartieDeChasse();
        assertThat(savedPartieDeChasse.getId()).isEqualTo(id);
        assertThat(savedPartieDeChasse.getStatus()).isEqualTo(PartieStatus.TERMINÉE);
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

        assertThat(meilleurChasseur).isEqualTo("Brocouille");
    }

    @Test
    void quand_les_chasseurs_sont_a_lapéro_et_tous_ex_aequo() throws QuandCestFiniCestFini {
        var id = UUID.randomUUID();
        var repository = new PartieDeChasseRepositoryForTests();

        repository.add(new PartieDeChasse() {
            {
                setId(id);
                setChasseurs(new ArrayList<>() {{
                    add(new Chasseur() {{
                        setNom("Dédé");
                        setBallesRestantes(20);
                        setNbGalinettes(3);
                    }});
                    add(new Chasseur() {{
                        setNom("Bernard");
                        setBallesRestantes(8);
                        setNbGalinettes(3);
                    }});
                    add(new Chasseur() {{
                        setNom("Robert");
                        setBallesRestantes(12);
                        setNbGalinettes(3);
                    }});
                }});

                setTerrain(new Terrain("Pitibon sur Sauldre") {{
                    setNbGalinettes(3);
                }});
                setStatus(PartieStatus.APÉRO);
                setEvents(new ArrayList<>());
            }
        });
        var service = new PartieDeChasseService(repository, LocalDateTime::now);

        var meilleurChasseur = service.terminerLaPartie(id);

        var savedPartieDeChasse = repository.getSavedPartieDeChasse();
        assertThat(savedPartieDeChasse.getId()).isEqualTo(id);
        assertThat(savedPartieDeChasse.getStatus()).isEqualTo(PartieStatus.TERMINÉE);
        assertThat(savedPartieDeChasse.getTerrain().getNom()).isEqualTo("Pitibon sur Sauldre");
        assertThat(savedPartieDeChasse.getTerrain().getNbGalinettes()).isEqualTo(3);
        assertThat(savedPartieDeChasse.getChasseurs()).hasSize(3);
        assertThat(savedPartieDeChasse.getChasseurs().get(0).getNom()).isEqualTo("Dédé");
        assertThat(savedPartieDeChasse.getChasseurs().get(0).getBallesRestantes()).isEqualTo(20);
        assertThat(savedPartieDeChasse.getChasseurs().get(0).getNbGalinettes()).isEqualTo(3);
        assertThat(savedPartieDeChasse.getChasseurs().get(1).getNom()).isEqualTo("Bernard");
        assertThat(savedPartieDeChasse.getChasseurs().get(1).getBallesRestantes()).isEqualTo(8);
        assertThat(savedPartieDeChasse.getChasseurs().get(1).getNbGalinettes()).isEqualTo(3);
        assertThat(savedPartieDeChasse.getChasseurs().get(2).getNom()).isEqualTo("Robert");
        assertThat(savedPartieDeChasse.getChasseurs().get(2).getBallesRestantes()).isEqualTo(12);
        assertThat(savedPartieDeChasse.getChasseurs().get(2).getNbGalinettes()).isEqualTo(3);

        assertThat(meilleurChasseur).isEqualTo("Dédé, Bernard, Robert");
    }

    @Test
    void echoue_si_la_partie_est_déja_terminée() {
        var id = UUID.randomUUID();
        var repository = new PartieDeChasseRepositoryForTests();

        repository.add(new PartieDeChasse() {
            {
                setId(id);
                setChasseurs(new ArrayList<>() {{
                    add(new Chasseur() {{
                        setNom("Dédé");
                        setBallesRestantes(20);
                    }});
                    add(new Chasseur() {{
                        setNom("Bernard");
                        setBallesRestantes(8);
                    }});
                    add(new Chasseur() {{
                        setNom("Robert");
                        setBallesRestantes(12);
                    }});
                }});

                setTerrain(new Terrain("Pitibon sur Sauldre") {{
                    setNbGalinettes(3);
                }});
                setStatus(PartieStatus.TERMINÉE);
                setEvents(new ArrayList<>());
            }
        });
        var service = new PartieDeChasseService(repository, LocalDateTime::now);

        assertThatThrownBy(() -> service.terminerLaPartie(id))
                .isInstanceOf(QuandCestFiniCestFini.class);
        assertThat(repository.getSavedPartieDeChasse()).isNull();
    }
}