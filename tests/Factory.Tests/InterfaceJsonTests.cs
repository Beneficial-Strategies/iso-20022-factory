// Copyright 2026 Jeff Ward, Beneficial Strategies. Usage subject to license of enclosing library.

using System.Text.Json;
using System.Text.Json.Nodes;

namespace BeneficialStrategies.Iso20022.Factory;

/// <summary>The interfaces read and write as JSON in the shape of the records, with the objects made by the factory.</summary>
public class InterfaceJsonTests
{
    private static readonly IsoDataFactory Factory = new();

    private static readonly JsonSerializerOptions Options = Build(Iso20022JsonSerializerOptions.Default);

    private static JsonSerializerOptions Build(JsonSerializerOptions basis)
    {
        var options = new JsonSerializerOptions(basis);
        options.Converters.Add(new Iso20022InterfaceJsonConverterFactory(Factory));
        return options;
    }

    private static IBatch ReadXml() => Iso20022XmlSerializer.TryRead<IBatch>(Golden.Xml, Factory).Message!;

    [Fact]
    public void AMessageOfTheFactory_IsWrittenAsTheJsonOfTheGolden()
    {
        var fromInterface = JsonSerializer.Serialize(ReadXml(), Options);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(Golden.Json), JsonNode.Parse(fromInterface)), fromInterface);
    }

    [Fact]
    public void AnInterfaceReadFromTheJson_IsTheObjectReadFromTheXml()
    {
        var viaJson = JsonSerializer.Deserialize<IBatch>(Golden.Json, Options)!;

        Assert.Equal(ReadXml(), viaJson);
        Assert.Equal(2, viaJson.Item.Count());
    }

    [Fact]
    public void AChoice_IsWrittenWithItsVariantAndReadBackIntoTheSameVariant()
    {
        var choice = Iso20022XmlSerializer.Read<IAccountIdentification>(
            XElement.Parse("<Id xmlns=\"urn:test\"><Othr><Id>ABC-1</Id><Issr>BANK</Issr></Othr></Id>"), factory: Factory).Message!;

        var json = JsonSerializer.Serialize(choice, Options);
        var back = JsonSerializer.Deserialize<IAccountIdentification>(json, Options)!;

        Assert.Equal("Other", JsonNode.Parse(json)!["$type"]!.GetValue<string>());
        Assert.Equal(choice, back);
        Assert.Equal("BANK", Assert.IsAssignableFrom<IOther>(back).Issuer!.Value.Value);
    }

    [Fact]
    public void AChoiceWithoutItsVariant_IsAJsonExceptionThatSaysWhatIsExpected()
    {
        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<IAccountIdentification>("{\"value\":\"x\"}", Options));
        Assert.Contains("$type", ex.Message);

        ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<IAccountIdentification>("{\"$type\":\"Nope\"}", Options));
        Assert.Contains("Other", ex.Message);
    }

    [Fact]
    public void ABadValue_IsAJsonExceptionThatNamesTheMember()
    {
        var json = JsonNode.Parse(Golden.Json)!;
        json["header"]!["created"] = "2026-13-45";

        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<IBatch>(json.ToJsonString(), Options));

        Assert.Contains("created", ex.Message);
    }

    [Fact]
    public void ARequiredMemberThatIsMissing_IsAJsonException()
    {
        var json = JsonNode.Parse(Golden.Json)!;
        ((JsonObject)json).Remove("header");

        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<IBatch>(json.ToJsonString(), Options));

        Assert.Contains("header", ex.Message);
    }

    [Fact]
    public void AnAbsentList_IsEmpty()
    {
        var json = JsonNode.Parse(Golden.Json)!;
        ((JsonObject)json).Remove("item");

        var message = JsonSerializer.Deserialize<IBatch>(json.ToJsonString(), Options)!;

        Assert.Empty(message.Item);
    }

    [Fact]
    public void TheNamesFollowTheOptionsOfTheCaller()
    {
        var camel = Build(new JsonSerializerOptions(Iso20022JsonSerializerOptions.Default) { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var message = ReadXml();

        var json = JsonSerializer.Serialize(message, camel);

        Assert.NotNull(JsonNode.Parse(json)!["header"]);
        Assert.Equal(message, JsonSerializer.Deserialize<IBatch>(json, camel));
    }
}
