![Beneficial Strategies](https://raw.githubusercontent.com/Beneficial-Strategies/iso-20022-factory/main/.beneficial/logo.png)

# BeneficialStrategies.Iso20022.Factory

Objects for the ISO 20022 interfaces, made at run time: ask for an interface and the Factory builds a class that implements it, fills it from XML or JSON, and gives it value equality.

**Objects for the ISO 20022 interfaces, made for you.** Ask for an interface (a message, a component, a choice variant) and the Factory builds, at run time, a small sealed class that implements it, fills it from XML or JSON, and gives it to you. You never write the implementation, and you do not install one: no message records, no [BeneficialStrategies.Iso20022](https://www.nuget.org/packages/BeneficialStrategies.Iso20022) with its 3,339 messages' worth of classes.

It rests on the idea behind the whole family: the **interfaces are the contract** ([BeneficialStrategies.Iso20022.Messages](https://www.nuget.org/packages/BeneficialStrategies.Iso20022.Messages)), and the tools (validators, readers, queue consumers) are written against them, not against any one implementation. The records are one implementation, a class of your own is another, and the Factory is the one that appears when you ask.

It is not specific to ISO's own messages: it makes an object for any interface that follows the pattern, an interface of your own with the primitive types for members. It depends only on [BeneficialStrategies.Iso20022.Primitives](https://www.nuget.org/packages/BeneficialStrategies.Iso20022.Primitives), and targets netstandard2.0, net8.0 and net10.0.

## Install

```
dotnet add package BeneficialStrategies.Iso20022.Factory --prerelease
```

The package is in alpha, so the command asks for a prerelease version. Three examples follow, each a step further, and each using nothing but this package.

## How to use

### Your own interface

The interface of a message of your own, with the primitive types for members:

<!-- snippet: examples/01-your-own-interface/IMyMessage.cs#interface -->
```csharp
[IsoMessage("xmpl.001.001.01", "Party", "urn:example:xmpl.001.001.01")]   // identifier, element, namespace of the document
public interface IMyMessage : IOuterRecord
{
    [IsoXmlTag("Name")] Max35Text Name { get; }                      // at most 35 characters
    [IsoXmlTag("ChargeBearer")] ChargeBearerType1Code ChargeBearer { get; }   // DEBT, CRED, SHAR or SLEV
}
```
<!-- /snippet -->

There is no class that implements `IMyMessage`, and you do not write one. Give the reader a factory:

<!-- snippet: examples/01-your-own-interface/Program.cs#read -->
```csharp
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
```
<!-- /snippet -->

The object is a normal value: its members are the interface's, `Equals` and `GetHashCode` compare the members (and lists by their items), `ToString` shows them, and the serializer writes it back to the same XML:

<!-- snippet: examples/01-your-own-interface/Program.cs#equality -->
```csharp
var again = Iso20022XmlSerializer.TryRead<IMyMessage>(xml, factory).Message!;
Console.WriteLine(result.Message!.Equals(again));          // True: the members are compared
Console.WriteLine(result.Message);                         // IMyMessage_1 { Name = Acme Corp, ChargeBearer = Shared }
```
<!-- /snippet -->

<!-- snippet: examples/01-your-own-interface/Program.cs#write -->
```csharp
Console.WriteLine(Iso20022XmlSerializer.SerializeToString(result.Message));
```
<!-- /snippet -->

```xml
<Document xmlns="urn:example:xmpl.001.001.01">
  <Party>
    <Name>Acme Corp</Name>
    <ChargeBearer>SHAR</ChargeBearer>
  </Party>
</Document>
```

Try the [project](https://github.com/Beneficial-Strategies/iso-20022-factory/tree/main/examples/01-your-own-interface) yourself.

### In an application: dependency injection

<!-- snippet: examples/02-dependency-injection/Program.cs#register -->
```csharp
var services = new ServiceCollection();
services.AddIso20022Factory(typeof(IMyMessage));     // the messages you will read

using var provider = services.BuildServiceProvider();
var factory = provider.GetRequiredService<IsoDataFactory>();
Console.WriteLine(ReferenceEquals(factory, provider.GetRequiredService<IIsoDataFactory>()));   // True: one factory, for both services

factory.Warm(typeof(IMyMessage));   // without a host, warm it yourself at startup
```
<!-- /snippet -->

`AddIso20022Factory` registers one `IsoDataFactory` (also as `IIsoDataFactory`) and, in a host, builds the classes for those messages and everything they reach when the host starts, so no read pays for building a class. Without a host, call `factory.Warm(typeof(IFoo), ...)` yourself at startup. The factory is thread-safe, and what it builds is shared by every factory in the process.

Try the [project](https://github.com/Beneficial-Strategies/iso-20022-factory/tree/main/examples/02-dependency-injection) yourself.

### Making an object by hand

For a test or a sample you want an object without reading anything. `IsoDataFactory.Make<T>` makes one for an interface and sets its members. The interface has no setters, so each member is named with a lambda and then given its value (two steps, so that the value converts to the member's type as it does for a record: `"ID-1"` to a `Max35Text`):

<!-- snippet: examples/03-making-an-object-by-hand/Program.cs#make -->
```csharp
var id = IsoDataFactory.Make<IMyIdentification>(b => b
    .Set(x => x.Identification).To("ID-1")
    .Set(x => x.Issuer).To("BANK"));

var other = IsoDataFactory.With(id, b => b.Set(x => x.Identification).To("ID-2"));   // a copy, one member changed
Console.WriteLine($"{id.Identification} {other.Identification} {id.Issuer}");         // ID-1 ID-2 BANK
```
<!-- /snippet -->

`IsoDataFactory.With` is the counterpart of a record's `with`: a copy of an object (of the factory, or a record) with the named members changed; the source is not touched. A member that is not named stays unset (a list is empty, and a list you give is compared by its items). `Make` and `With` build with `IsoDataFactory.Shared`. [BeneficialStrategies.Iso20022.Samples](https://www.nuget.org/packages/BeneficialStrategies.Iso20022.Samples) holds named valid objects made this way, for tests.

Try the [project](https://github.com/Beneficial-Strategies/iso-20022-factory/tree/main/examples/03-making-an-object-by-hand) yourself.

## More

* Documentation, release notes and issues: https://github.com/Beneficial-Strategies/iso-20022-factory
* License: MIT. Status: alpha.

### The interfaces of ISO's own messages

Add the package that has them, and ask for a message: `dotnet add package BeneficialStrategies.Iso20022.Messages --prerelease`, then `Iso20022XmlSerializer.TryRead<IFIToFIPaymentCancellationRequestV10>(xml, factory)`. Everything is as above, for a message with a deep tree of components: it reads, compares by value, and writes back to XML that reads into an equal object. If the document is wrong, the result lists every problem and no object is made. The validators of [BeneficialStrategies.Iso20022.Messages.Validation](https://www.nuget.org/packages/BeneficialStrategies.Iso20022.Messages.Validation) are written for the interfaces, so they accept the object. To read XML and validate it in one call, or queue payloads, see [Validation.Xml](https://www.nuget.org/packages/BeneficialStrategies.Iso20022.Validation.Xml) and [Validation.Xml.Payloads](https://www.nuget.org/packages/BeneficialStrategies.Iso20022.Validation.Xml.Payloads), which take the factory too.

### What it costs

* **Building a class** takes about 0.1 millisecond. A class is built once per interface for the life of the process and shared by every factory. Measured on the message library: the 131 interfaces that pacs.008 reaches build in about 11 ms, the 219 of camt.053 in about 27 ms, plus a one-time walk of the message's shape of under 0.2 s. `Warm` moves all of it to startup.
* **Reading** costs what reading into a record costs; an object made by the factory is an ordinary object, with no proxy, no interception and no per-instance overhead beyond its fields.
* **Memory:** the built classes cannot be unloaded. Warm only the messages you read.

### When not to use it

* **Native AOT and trimmed apps:** `System.Reflection.Emit` is not available. Read into the records of [BeneficialStrategies.Iso20022](https://www.nuget.org/packages/BeneficialStrategies.Iso20022) instead.
* **You want `with`, pattern matching on a concrete type or `new Message { ... }`:** use the records. The factory's classes exist to be read into and then used through the interface.
* The interface must be public (a nested public interface is fine).
* **Shapes that refer to themselves** (an entity that points to its owner, which points back) can be built and read, but the hash of such an object does not terminate: do not use one as a key or compare two of them.

### Your own factory

`IIsoDataFactory` is one method, `object Create(Type iface)`: return an object of a class of yours that implements the interface and has a public setter for each of its properties. The reader does the rest.

<!-- end of package readme -->

## Building and contributing

```
dotnet build -c Release
dotnet test -c Release --no-build
tools/run-examples.sh              # packs the package, then builds and runs every example against it
python3 tools/sync-readme.py       # fills the code blocks above from the examples
```

The .NET Framework 4.8 tests run on Windows only. A bug report or a change starts with an issue; a bug fix names the test that failed before it. See CONTRIBUTING.md.
