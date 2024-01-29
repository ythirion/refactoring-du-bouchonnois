package bouchonnois.service.exceptions;

public class ChasseurInconnu extends Exception {
    public ChasseurInconnu(String chasseur) {
        super("Chasseur inconnu " + chasseur);
    }
}
