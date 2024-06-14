package bouchonnois.tests.builders;

import bouchonnois.domain.Chasseur;

public class ChasseurBuilder {
    private final String nom;
    private int ballesRestantes;
    private int nbGalinettes;

    public ChasseurBuilder(String nom) {
        this.nom = nom;
    }

    private ChasseurBuilder(String nom, int ballesRestantes) {
        this.nom = nom;
        this.ballesRestantes = ballesRestantes;
    }

    public static ChasseurBuilder leChasseur(String nom) {
        return new ChasseurBuilder(nom);
    }

    public static ChasseurBuilder dédé() {
        return new ChasseurBuilder("Dédé", 20);
    }

    public static ChasseurBuilder bernard() {
        return new ChasseurBuilder("Bernard", 8);
    }

    public static ChasseurBuilder robert() {
        return new ChasseurBuilder("Robert", 12);
    }

    public ChasseurBuilder sansBalles() {
        this.ballesRestantes = 0;
        return this;
    }

    public ChasseurBuilder ayantTué(int nbGalinettes) {
        this.nbGalinettes = nbGalinettes;
        return this;
    }

    public Chasseur build() {
        return new Chasseur() {{
            setNom(nom);
            setBallesRestantes(ballesRestantes);
            setNbGalinettes(nbGalinettes);
        }};
    }
}
