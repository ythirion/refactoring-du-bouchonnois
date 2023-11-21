package bouchonnois.domain;

import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class Chasseur {
    private String nom;
    private int ballesRestantes;
    private int nbGalinettes;
}