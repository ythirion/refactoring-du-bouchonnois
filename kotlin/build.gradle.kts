import info.solidsoft.gradle.pitest.PitestPluginExtension

plugins {
    kotlin("jvm") version "1.9.20"
    application
    jacoco
    id("info.solidsoft.pitest") version "1.9.11"
}

group = "org.bouchonnois"
version = "1.0-SNAPSHOT"

repositories {
    mavenCentral()
}

dependencies {
    testImplementation("io.kotest:kotest-runner-junit5:5.7.2")
    testImplementation("io.kotest.extensions:kotest-extensions-pitest:1.2.0")
}

tasks.test {
    useJUnitPlatform()
    finalizedBy(tasks.jacocoTestReport)
}

kotlin {
    jvmToolchain(8)
    compilerOptions {
        allWarningsAsErrors = true
    }
}


jacoco {
    toolVersion = "0.8.7"
}

tasks.jacocoTestReport {
    dependsOn(tasks.test)
    reports {
        xml.required.set(true)
    }
}

configure<PitestPluginExtension> {
    targetClasses.set(listOf("bouchonnois.*"))
}

application {
    mainClass.set("MainKt")
}