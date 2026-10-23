using BeneficialStrategies.Iso20022;
using BeneficialStrategies.Iso20022.Framework;
using BeneficialStrategies.Iso20022.SimpleTypes;

namespace MyCompany.Examples;

public interface IMyIdentification : IIsoComponent
{
    [IsoXmlTag("Id")] Max35Text Identification { get; }
    [IsoXmlTag("Issr")] Max35Text? Issuer { get; }
}
