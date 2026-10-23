using BeneficialStrategies.Iso20022;
using BeneficialStrategies.Iso20022.Codesets;
using BeneficialStrategies.Iso20022.Framework;
using BeneficialStrategies.Iso20022.SimpleTypes;

namespace MyCompany.Examples;

// <snippet interface>
[IsoMessage("xmpl.001.001.01", "Party", "urn:example:xmpl.001.001.01")]   // identifier, element, namespace of the document
public interface IMyMessage : IOuterRecord
{
    [IsoXmlTag("Name")] Max35Text Name { get; }                      // at most 35 characters
    [IsoXmlTag("ChargeBearer")] ChargeBearerType1Code ChargeBearer { get; }   // DEBT, CRED, SHAR or SLEV
}
// </snippet>
