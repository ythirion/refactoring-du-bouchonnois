package bouchonnois.service.exceptions


class ChasseurInconnu(chasseur: String) : Exception("Chasseur inconnu $chasseur")