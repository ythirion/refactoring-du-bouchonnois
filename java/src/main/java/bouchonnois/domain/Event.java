package bouchonnois.domain;

import java.time.LocalDate;

public record Event(LocalDate date, String message) {
    @Override
    public String toString() {
        return "Event{" +
                "date=" + date +
                ", message='" + message + '\'' +
                '}';
    }
}