// Copyright 2026 Jeff Ward, Beneficial Strategies. Usage subject to license of enclosing library.

namespace BeneficialStrategies.Iso20022.Factory.TestModel;

// A small model of the shapes the Factory builds, owned by these tests: a message, components, a list, an optional member,
// a choice with variants. It follows the pattern of the interfaces of the ISO 20022 messages, and needs none of them.

[IsoMessage("test.001.001.01", "Batch", "urn:test:batch")]
public interface IBatch : IOuterRecord
{
    [IsoXmlTag("Hdr")]
    IHeader Header { get; }

    [IsoXmlTag("Itm")]
    IEnumerable<IItem> Item { get; }

    [IsoXmlTag("Note")]
    Max35Text? Note { get; }
}

public interface IHeader : IIsoComponent
{
    [IsoXmlTag("Id")]
    Max35Text Identification { get; }

    [IsoXmlTag("CreDtTm")]
    ISODateTime Created { get; }

    [IsoXmlTag("Cnt")]
    Number? Count { get; }
}

public interface IItem : IIsoComponent
{
    [IsoXmlTag("Id")]
    Max35Text Identification { get; }

    [IsoXmlTag("Amt")]
    IMoney Amount { get; }

    [IsoXmlTag("Acct")]
    IAccountIdentification? Account { get; }
}

public interface IMoney : IIsoComponent
{
    [IsoXmlTag("Amt")]
    ActiveCurrencyAndAmount Amount { get; }
}

public interface IIdentification : IIsoComponent
{
    [IsoXmlTag("Id")]
    Max35Text Identification { get; }

    [IsoXmlTag("Issr")]
    Max35Text? Issuer { get; }
}

/// <summary>A choice: exactly one of its variants.</summary>
public interface IAccountIdentification : IIsoChoice { }

public interface IIban : IAccountIdentification, IIsoChoiceVariant
{
    [IsoXmlTag("IBAN")]
    IBAN2007Identifier Value { get; }
}

[IsoXmlTag("Othr")]
public interface IOther : IAccountIdentification, IIsoChoiceVariant
{
    [IsoXmlTag("Id")]
    Max35Text Identification { get; }

    [IsoXmlTag("Issr")]
    Max35Text? Issuer { get; }
}

// Shapes that refer to themselves and to each other, as the objects of a business model do.

public interface IPartyNode : IIsoComponent
{
    [IsoXmlTag("Nm")]
    Max35Text? Name { get; }

    [IsoXmlTag("Parent")]
    IPartyNode? Parent { get; }

    [IsoXmlTag("Acct")]
    IEnumerable<IAccountNode> Account { get; }
}

public interface IAccountNode : IIsoComponent
{
    [IsoXmlTag("Id")]
    Max35Text? Identification { get; }

    [IsoXmlTag("Ownr")]
    IPartyNode? Owner { get; }
}
