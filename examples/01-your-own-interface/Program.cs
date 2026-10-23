// Example 01: an interface of your own, read from XML into an object the Factory builds.
// Shows:  that you write only the interface; the class that implements it is built at run time, and it has value equality.
// Needs:  only BeneficialStrategies.Iso20022.Factory.
// Prints: the message read, whether two reads are equal, and the XML written back.
using BeneficialStrategies.Iso20022.Factory;
using BeneficialStrategies.Iso20022.Serialization;
using MyCompany.Examples;

// <snippet read>
var factory = new IsoDataFactory();

var xml = "<Document xmlns=\"urn:example:xmpl.001.001.01\"><Party><Name>Acme Corp</Name><ChargeBearer>SHAR</ChargeBearer></Party></Document>";

var result = Iso20022XmlSerializer.TryRead<IMyMessage>(xml, factory);

if (result.Succeeded)
{
    IMyMessage party = result.Message!;                    // an object of a class built for the interface
    Console.WriteLine($"{party.Name} pays {party.ChargeBearer}");   // Acme Corp pays Shared
}
else
    foreach (var issue in result.Issues)
        Console.WriteLine(issue);                          // every problem, with its path
// </snippet>

// <snippet equality>
var again = Iso20022XmlSerializer.TryRead<IMyMessage>(xml, factory).Message!;
Console.WriteLine(result.Message!.Equals(again));          // True: the members are compared
Console.WriteLine(result.Message);                         // IMyMessage_1 { Name = Acme Corp, ChargeBearer = Shared }
// </snippet>

// <snippet write>
Console.WriteLine(Iso20022XmlSerializer.SerializeToString(result.Message));
// </snippet>
