package bouchonnois.domain;

import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;

public record Event(LocalDateTime date, String message) {
    @Override
    public String toString() {
        return date.format(DateTimeFormatter.ofPattern("HH:mm - ")) + message;
    }
}