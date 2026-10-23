// Example 03: making an object by hand.
// Shows:  IsoDataFactory.Make and With, for a test or a sample where nothing is read.
// Needs:  only BeneficialStrategies.Iso20022.Factory.
// Prints: the identifications of an object and of a copy with one member changed.
using BeneficialStrategies.Iso20022.Factory;
using MyCompany.Examples;

// <snippet make>
var id = IsoDataFactory.Make<IMyIdentification>(b => b
    .Set(x => x.Identification).To("ID-1")
    .Set(x => x.Issuer).To("BANK"));

var other = IsoDataFactory.With(id, b => b.Set(x => x.Identification).To("ID-2"));   // a copy, one member changed
Console.WriteLine($"{id.Identification} {other.Identification} {id.Issuer}");         // ID-1 ID-2 BANK
// </snippet>
