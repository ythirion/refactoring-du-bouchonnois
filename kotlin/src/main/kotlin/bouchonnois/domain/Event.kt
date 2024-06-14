package bouchonnois.domain

import java.time.LocalDateTime
import java.time.format.DateTimeFormatter

data class Event(val date: LocalDateTime, val message: String) {
    override fun toString(): String {
        return date.format(DateTimeFormatter.ofPattern("HH:mm - ")) + message
    }
}