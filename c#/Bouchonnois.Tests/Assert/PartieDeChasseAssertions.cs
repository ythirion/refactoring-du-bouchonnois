using Bouchonnois.Domain;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using static Bouchonnois.Domain.PartieStatus;

namespace Bouchonnois.Tests.Assert;

public class PartieDeChasseAssertions : ReferenceTypeAssertions<PartieDeChasse?, PartieDeChasseAssertions>
{
    protected override string Identifier => "partie de chasse";

    public PartieDeChasseAssertions(PartieDeChasse? partieDeChasse)
        : base(partieDeChasse)
    {
    }

    private AndConstraint<PartieDeChasseAssertions> Call(Action act)
    {
        act();
        return new AndConstraint<PartieDeChasseAssertions>(this);
    }

    public AndConstraint<PartieDeChasseAssertions> HaveEmittedEvent(
        DateTime expectedTime,
        string expectedMessage)
    {
        var expectedEvent = new Event(expectedTime, expectedMessage);

        return Call(
            () => Execute.Assertion
                .ForCondition(!string.IsNullOrEmpty(expectedMessage))
                .FailWith("Impossible de faire une assertion sur un message vide")
                .Then
                .Given(() => Subject!.Events)
                .ForCondition(events => events.Count == 1 && events.Last() == new Event(expectedTime, expectedMessage))
                .FailWith($"Les events devraient contenir {expectedEvent}."));
    }


    public AndConstraint<PartieDeChasseAssertions> ChasseurATiréSurUneGalinette(
        string nom,
        int ballesRestantes,
        int galinettes)
        => Call(() =>
            Execute.Assertion
                .ForCondition(Subject!.Chasseurs.Any(c => c.Nom == nom))
                .FailWith("Chasseur non présent dans la partie de chasse")
                .Then
                .Given(() => Subject!.Chasseurs.First(c => c.Nom == nom))
                .ForCondition(
                    chasseur => chasseur.BallesRestantes == ballesRestantes && chasseur.NbGalinettes == galinettes)
                .FailWith(
                    $"Le nombre de balles restantes pour {nom} devrait être de {ballesRestantes} balle(s) et il devrait avoir capturé {galinettes} galinette(s), " +
                    $"il lui reste {Chasseur(nom).BallesRestantes} balle(s) et a capturé {Chasseur(nom).NbGalinettes} galinette(s)"));

    public AndConstraint<PartieDeChasseAssertions> ChasseurATiré(
        string nom,
        int ballesRestantes)
        => Call(() =>
            Execute.Assertion
                .ForCondition(Subject!.Chasseurs.Any(c => c.Nom == nom))
                .FailWith("Chasseur non présent dans la partie de chasse")
                .Then
                .Given(() => Subject!.Chasseurs.First(c => c.Nom == nom))
                .ForCondition(
                    chasseur => chasseur.BallesRestantes == ballesRestantes)
                .FailWith(
                    $"Le nombre de balles restantes pour {nom} devrait être de {ballesRestantes} balle(s)"));


    private Chasseur Chasseur(string nom) => Subject!.Chasseurs.First(c => c.Nom == nom);

    public AndConstraint<PartieDeChasseAssertions> GalinettesSurLeTerrain(int nbGalinettes)
        => Call(() =>
            Execute.Assertion
                .Given(() => Subject!.Terrain)
                .ForCondition(terrain => terrain.NbGalinettes == nbGalinettes)
                .FailWith(
                    $"Le terrain devrait contenir {nbGalinettes} mais en contient {Subject!.Terrain.NbGalinettes}"));

    public AndConstraint<PartieDeChasseAssertions> BeInApéro()
        => Call(() =>
            Execute.Assertion
                .Given(() => Subject!)
                .ForCondition(partieDeChasse => partieDeChasse.Status == Apéro)
                .FailWith("Les chasseurs devraient être à l'apéro"));

    public AndConstraint<PartieDeChasseAssertions> BeEnCours()
        => Call(() =>
            Execute.Assertion
                .Given(() => Subject!)
                .ForCondition(partieDeChasse => partieDeChasse.Status == EnCours)
                .FailWith("Les chasseurs devraient être en cours de chasse"));
}