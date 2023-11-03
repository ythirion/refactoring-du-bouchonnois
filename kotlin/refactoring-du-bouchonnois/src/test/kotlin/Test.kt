import io.kotest.core.spec.style.StringSpec
import io.kotest.matchers.shouldBe

class Test : StringSpec({
    "a first string spec" {
        1 + 2 shouldBe 3
    }
})