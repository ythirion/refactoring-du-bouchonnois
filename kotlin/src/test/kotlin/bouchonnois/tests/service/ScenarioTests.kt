package bouchonnois.tests.service

import bouchonnois.service.PartieDeChasseService
import bouchonnois.service.exceptions.TasPlusDeBallesMonVieuxChasseALaMain
import bouchonnois.tests.doubles.PartieDeChasseRepositoryForTests
import io.kotest.core.spec.style.FeatureSpec
import io.kotest.matchers.shouldBe
import java.lang.System.lineSeparator
import java.time.LocalDateTime
import java.time.Month

class ScenarioTests : FeatureSpec({
    feature("test d'acceptation métier") {
        var time = LocalDateTime.of(2024, Month.APRIL, 25, 9, 0, 0)

        scenario("dérouler une partie") {
            val repository = PartieDeChasseRepositoryForTests()
            val service = PartieDeChasseService(repository) { time }
            val chasseurs = mapOf(
                ("Dédé" to 20),
                ("Bernard" to 8),
                ("Robert" to 12)
            )
            val terrainDeChasse = ("Pitibon sur Sauldre" to 4)

            val id = service.démarrer(
                terrainDeChasse,
                chasseurs
            )

            time = time.plusMinutes(10)
            service.tirer(id, "Dédé")

            time = time.plusMinutes(30)
            service.tirerSurUneGalinette(id, "Robert")

            time = time.plusMinutes(20)
            service.prendreLapéro(id)

            time = time.plusHours(1)
            service.reprendreLaPartie(id)

            time = time.plusMinutes(2)
            service.tirer(id, "Bernard")

            time = time.plusMinutes(1)
            service.tirer(id, "Bernard")

            time = time.plusMinutes(1)
            service.tirerSurUneGalinette(id, "Dédé")

            time = time.plusMinutes(26)
            service.tirerSurUneGalinette(id, "Robert")

            time = time.plusMinutes(10)
            service.prendreLapéro(id)

            time = time.plusMinutes(170)
            service.reprendreLaPartie(id)

            time = time.plusMinutes(11)
            service.tirer(id, "Bernard")

            time = time.plusSeconds(1)
            service.tirer(id, "Bernard")

            time = time.plusSeconds(1)
            service.tirer(id, "Bernard")

            time = time.plusSeconds(1)
            service.tirer(id, "Bernard")

            time = time.plusSeconds(1)
            service.tirer(id, "Bernard")

            time = time.plusSeconds(1)
            service.tirer(id, "Bernard")

            time = time.plusSeconds(1)

            try {
                service.tirer(id, "Bernard")
            } catch (e: TasPlusDeBallesMonVieuxChasseALaMain) {
            }

            time = time.plusMinutes(19)
            service.tirerSurUneGalinette(id, "Robert")

            time = time.plusMinutes(30)
            service.terminerLaPartie(id)

            service.consulterStatus(id) shouldBe "15:30 - La partie de chasse est terminée, vainqueur : Robert - 3 galinettes" + lineSeparator() +
                    "15:00 - Robert tire sur une galinette" + lineSeparator() +
                    "14:41 - Bernard tire -> T'as plus de balles mon vieux, chasse à la main" + lineSeparator() +
                    "14:41 - Bernard tire" + lineSeparator() +
                    "14:41 - Bernard tire" + lineSeparator() +
                    "14:41 - Bernard tire" + lineSeparator() +
                    "14:41 - Bernard tire" + lineSeparator() +
                    "14:41 - Bernard tire" + lineSeparator() +
                    "14:41 - Bernard tire" + lineSeparator() +
                    "14:30 - Reprise de la chasse" + lineSeparator() +
                    "11:40 - Petit apéro" + lineSeparator() +
                    "11:30 - Robert tire sur une galinette" + lineSeparator() +
                    "11:04 - Dédé tire sur une galinette" + lineSeparator() +
                    "11:03 - Bernard tire" + lineSeparator() +
                    "11:02 - Bernard tire" + lineSeparator() +
                    "11:00 - Reprise de la chasse" + lineSeparator() +
                    "10:00 - Petit apéro" + lineSeparator() +
                    "09:40 - Robert tire sur une galinette" + lineSeparator() +
                    "09:10 - Dédé tire" + lineSeparator() + "09:00 - La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"
        }
    }
})