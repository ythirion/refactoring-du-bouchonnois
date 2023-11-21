package bouchonnois.service;

import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class Terrain {
    private final String nom;
    private int nbGalinettes;

    public Terrain(String nom) {
        this.nom = nom;
    }
}
