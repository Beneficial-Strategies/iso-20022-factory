// Copyright 2026 Jeff Ward, Beneficial Strategies. Usage subject to license of enclosing library.

namespace BeneficialStrategies.Iso20022.Factory;

/// <summary>One message of <see cref="IBatch"/> as XML and as JSON, kept as text: what is read from the one is what is written as the other.</summary>
internal static class Golden
{
    internal const string Xml = """
<Document xmlns="urn:test:batch">
  <Batch>
    <Hdr>
      <Id>B-001</Id>
      <CreDtTm>2026-01-01T00:00:00</CreDtTm>
      <Cnt>2</Cnt>
    </Hdr>
    <Itm>
      <Id>I-001</Id>
      <Amt>
        <Amt Ccy="EUR">100</Amt>
      </Amt>
    </Itm>
    <Itm>
      <Id>I-002</Id>
      <Amt>
        <Amt Ccy="EUR">200</Amt>
      </Amt>
      <Acct>
        <IBAN>DE89370400440532013000</IBAN>
      </Acct>
    </Itm>
  </Batch>
</Document>
""";

    internal const string Json = """
{"header":{"identification":"B-001","created":"2026-01-01T00:00:00","count":"2"},"item":[{"identification":"I-001","amount":{"amount":{"currency":"EUR","amount":"100"}},"account":null},{"identification":"I-002","amount":{"amount":{"currency":"EUR","amount":"200"}},"account":{"$type":"Iban","value":"DE89370400440532013000"}}],"note":null}
""";
}
