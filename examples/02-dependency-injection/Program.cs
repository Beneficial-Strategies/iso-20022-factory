// Example 02: the Factory in an application, through dependency injection.
// Shows:  registering one factory for the messages you read, and warming it so that no read pays for building a class.
// Needs:  BeneficialStrategies.Iso20022.Factory and Microsoft.Extensions.DependencyInjection
//         (dotnet add package Microsoft.Extensions.DependencyInjection).
// Prints: that the factory is registered as both services, and the message it reads.
using BeneficialStrategies.Iso20022.Factory;
using BeneficialStrategies.Iso20022.Framework;
using BeneficialStrategies.Iso20022.Serialization;
using Microsoft.Extensions.DependencyInjection;
using MyCompany.Examples;

// <snippet register>
var services = new ServiceCollection();
services.AddIso20022Factory(typeof(IMyMessage));     // the messages you will read

using var provider = services.BuildServiceProvider();
var factory = provider.GetRequiredService<IsoDataFactory>();
Console.WriteLine(ReferenceEquals(factory, provider.GetRequiredService<IIsoDataFactory>()));   // True: one factory, for both services

factory.Warm(typeof(IMyMessage));   // without a host, warm it yourself at startup
// </snippet>

var xml = "<Document xmlns=\"urn:example:xmpl.001.001.01\"><Party><Name>Acme Corp</Name></Party></Document>";
Console.WriteLine(Iso20022XmlSerializer.TryRead<IMyMessage>(xml, factory).Message!.Name);   // Acme Corp
