# Changelog

The versions of this package have three parts, `design.breaking.compatible`: the first changes with a redesign, the second with a change that breaks existing users, the third with every other change. Newest first. Each entry is written from the issues closed since the previous release.

## 1.0.0-alpha (2026-10-23)

* Changed: The package lives in its own repository, with its history, and is versioned and released independently of the other packages; it uses the published `BeneficialStrategies.Iso20022.Primitives` package, from 1.0.0-alpha up to the next breaking change.
* Changed: Versions have three parts, `design.breaking.compatible`: the first changes with a redesign, the second with a change that breaks existing users, the third with every other change. This is not Semantic Versioning.
* Changed: The README is the package README, and its examples are projects under `examples/` that are built and run against the packed package. The tests no longer need the message interfaces or records: they use a small model of their own.
* Fixed: A list given to `IsoDataFactory.Make` or `With` as an array or a `List` is now compared by its items, as a list read from XML is; before, such an object was never equal to one read from the same XML.

## 0.7.0-alpha

Released from the monorepo this package lived in before (Iso20022Library), together with the other packages; its history is in this repository's commits.
