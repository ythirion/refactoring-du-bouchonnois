package bouchonnois.service.exceptions;

public class ChasseurInconnu extends RuntimeException {
    public ChasseurInconnu(String chasseur) {
        super("Chasseur inconnu " + chasseur);
    }
}
