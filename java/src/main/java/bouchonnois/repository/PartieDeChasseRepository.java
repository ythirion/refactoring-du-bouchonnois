package bouchonnois.repository;

import bouchonnois.domain.PartieDeChasse;

import java.util.UUID;

public interface PartieDeChasseRepository {
    void save(PartieDeChasse partieDeChasse);

    PartieDeChasse getById(UUID partieDeChasseId);
}