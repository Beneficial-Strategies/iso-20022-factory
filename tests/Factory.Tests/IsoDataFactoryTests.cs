// Copyright 2026 Jeff Ward, Beneficial Strategies. Usage subject to license of enclosing library.

namespace BeneficialStrategies.Iso20022.Factory;

public class IsoDataFactoryTests
{
    private static readonly IsoDataFactory Factory = new();

    /// <summary>The message of <see cref="Golden"/>, built by hand; the second item has the given identification.</summary>
    internal static IBatch Batch(string secondItem = "I-002") =>
        IsoDataFactory.Make<IBatch>(b => b
            .Set(x => x.Header).To(IsoDataFactory.Make<IHeader>(h => h
                .Set(x => x.Identification).To("B-001")
                .Set(x => x.Created).To("2026-01-01T00:00:00")
                .Set(x => x.Count).To("2")))
            .Set(x => x.Item).To([
                Item("I-001", 100m),
                IsoDataFactory.With(Item(secondItem, 200m), i => i
                    .Set(x => x.Account).To(IsoDataFactory.Make<IIban>(v => v.Set(x => x.Value).To("DE89370400440532013000")))),
            ]));

    private static IItem Item(string identification, decimal amount) =>
        IsoDataFactory.Make<IItem>(i => i
            .Set(x => x.Identification).To(identification)
            .Set(x => x.Amount).To(IsoDataFactory.Make<IMoney>(m => m
                .Set(x => x.Amount).To(new ActiveCurrencyAndAmount { Currency = "EUR", Amount = amount }))));

    private static IBatch Read(string xml)
    {
        var result = Iso20022XmlSerializer.TryRead<IBatch>(xml, Factory);
        Assert.True(result.Succeeded, string.Join("; ", result.Issues));
        return result.Message!;
    }

    [Fact]
    public void AMessage_IsReadIntoGeneratedClasses()
    {
        var message = Read(Golden.Xml);

        Assert.True(message.GetType().Assembly.IsDynamic);
        Assert.Equal("B-001", message.Header.Identification.Value);
        var items = message.Item.ToList();
        Assert.Equal(["I-001", "I-002"], items.Select(m => m.Identification.Value));
        Assert.Equal(200m, items[1].Amount.Amount.Amount);
    }

    [Fact]
    public void TheGeneratedClasses_HaveValueEquality()
    {
        var xml = Golden.Xml;

        var a = Read(xml);
        var b = Read(xml);
        var c = Read(Iso20022XmlSerializer.SerializeToString(Batch("I-999")));

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a, c);
        Assert.False(a.Equals(null));
        Assert.Contains("B-001", a.ToString());
    }

    [Fact]
    public void AnObjectMadeByHand_IsTheObjectReadFromTheXml()
    {
        Assert.Equal(Read(Golden.Xml), Batch());
    }

    [Fact]
    public void WhatWasRead_IsWrittenBackAsTheSameXml()
    {
        var xml = Golden.Xml;

        var written = Iso20022XmlSerializer.SerializeToString(Read(xml));

        Assert.True(
            XNode.DeepEquals(XDocument.Parse(xml).Root, XDocument.Parse(written).Root),
            written
        );
    }

    [Fact]
    public void AChoice_IsReadIntoTheVariantOfTheElementPresent()
    {
        var element = XElement.Parse(
            "<Id xmlns=\"urn:test\"><IBAN>DE89370400440532013000</IBAN></Id>"
        );

        var result = Iso20022XmlSerializer.Read<IAccountIdentification>(element, factory: Factory);

        Assert.True(result.Succeeded, string.Join("; ", result.Issues));
        Assert.Equal("DE89370400440532013000", Assert.IsAssignableFrom<IIban>(result.Message).Value.Value);
    }

    [Fact]
    public void AVariantWithSeveralMembers_IsReadByItsWrapperTag()
    {
        var element = XElement.Parse(
            "<Id xmlns=\"urn:test\"><Othr><Id>ABC-1</Id><Issr>BANK</Issr></Othr></Id>"
        );

        var result = Iso20022XmlSerializer.Read<IAccountIdentification>(element, factory: Factory);

        Assert.True(result.Succeeded, string.Join("; ", result.Issues));
        var other = Assert.IsAssignableFrom<IOther>(result.Message);
        Assert.Equal("ABC-1", other.Identification.Value);
        Assert.Equal("BANK", other.Issuer!.Value.Value);
    }

    [Fact]
    public void AListOfAnObjectMadeByTheFactory_StartsEmpty()
    {
        var batch = (IBatch)Factory.Create(typeof(IBatch));

        Assert.NotNull(batch.Item);
        Assert.Empty(batch.Item);
    }

    [Fact]
    public void AClassIsBuiltOncePerInterface_AndSharedByEveryFactory()
    {
        var one = new IsoDataFactory().ImplementationOf(typeof(IHeader));
        var two = new IsoDataFactory().ImplementationOf(typeof(IHeader));

        Assert.Same(one, two);
    }

    [Fact]
    public void ManyThreadsAskingForTheSameInterface_GetTheSameClass()
    {
        var types = Enumerable
            .Range(0, 64)
            .AsParallel()
            .Select(_ => new IsoDataFactory().Create(typeof(IMoney)).GetType())
            .Distinct()
            .ToList();

        Assert.Single(types);
    }

    [Fact]
    public void WarmingAMessage_BuildsEverythingItReaches()
    {
        var factory = new IsoDataFactory();

        factory.Warm(typeof(IBatch));

        Assert.NotNull(factory.Create(typeof(IBatch)));
        Assert.NotNull(factory.Create(typeof(IIban)));
        Assert.NotNull(factory.Create(typeof(IOther)));
    }

    [Fact]
    public void ShapesThatRefer_ToThemselvesAndToEachOther_CanBeWarmedAndBuilt()
    {
        var factory = new IsoDataFactory();

        factory.Warm(typeof(IPartyNode));
        var account = IsoDataFactory.Make<IAccountNode>(a => a.Set(x => x.Identification).To("A-1"));
        var party = IsoDataFactory.Make<IPartyNode>(p => p
            .Set(x => x.Name).To("Acme")
            .Set(x => x.Account).To([account]));
        var parent = IsoDataFactory.With(party, p => p.Set(x => x.Parent).To(party));

        Assert.Same(party, parent.Parent);
        Assert.Equal("A-1", parent.Account.Single().Identification!.Value.Value);
    }

    [Fact]
    public void AnythingButAnInterface_IsRefused()
    {
        Assert.Throws<ArgumentException>(() => Factory.Create(typeof(string)));
    }

    [Fact]
    public void ThePackage_NeedsNoMessageDefinitionsAndNoRecords()
    {
        var references = typeof(IsoDataFactory)
            .Assembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        // of the family, only Primitives (a Microsoft package such as Bcl.AsyncInterfaces may appear on .NET Framework)
        Assert.Equal(
            ["BeneficialStrategies.Iso20022.Primitives"],
            references.Where(name => name!.StartsWith("BeneficialStrategies.")).ToList());
    }

    [Fact]
    public void TheDependencyInjectionExtension_RegistersOneFactoryForBothServices()
    {
        var services = new ServiceCollection();
        services.AddIso20022Factory(typeof(IBatch));
        using var provider = services.BuildServiceProvider();

        Assert.Same(
            provider.GetRequiredService<IsoDataFactory>(),
            provider.GetRequiredService<IIsoDataFactory>()
        );
        var warm = provider.GetServices<Microsoft.Extensions.Hosting.IHostedService>().Single();
        warm.StartAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}
