using BeneficialStrategies.Iso20022;
using BeneficialStrategies.Iso20022.Framework;
using BeneficialStrategies.Iso20022.SimpleTypes;

namespace MyCompany.Examples;

[IsoMessage("xmpl.001.001.01", "Party", "urn:example:xmpl.001.001.01")]
public interface IMyMessage : IOuterRecord
{
    [IsoXmlTag("Name")] Max35Text Name { get; }
}
