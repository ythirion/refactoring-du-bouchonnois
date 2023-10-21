package bouchonnois.domain;

import lombok.Data;

@Data
public class Chasseur {
    private String nom;
    private int ballesRestantes;
    private int nbGalinettes;
}