// Copyright 2026 Jeff Ward, Beneficial Strategies. Usage subject to license of enclosing library.

namespace BeneficialStrategies.Iso20022.Factory;

public class IsoBuilderTests
{
    [Fact]
    public void Make_SetsTheNamedMembers_AndConvertsTheValuesAsARecordWould()
    {
        var id = IsoDataFactory.Make<IIdentification>(b => b
            .Set(x => x.Identification).To("ID-1")
            .Set(x => x.Issuer).To("BANK"));

        Assert.Equal("ID-1", id.Identification.Value);
        Assert.Equal("BANK", id.Issuer!.Value.Value);
    }

    [Fact]
    public void AMemberThatIsNotNamed_StaysUnset_AndAListIsEmpty()
    {
        var id = IsoDataFactory.Make<IIdentification>(b => b.Set(x => x.Identification).To("ID-1"));
        var batch = IsoDataFactory.Make<IBatch>();

        Assert.Null(id.Issuer);
        Assert.Empty(batch.Item);
    }

    [Fact]
    public void ObjectsNestAndAListTakesItsItems()
    {
        var batch = IsoDataFactory.Make<IBatch>(b => b
            .Set(x => x.Note).To("N-1")
            .Set(x => x.Item).To([
                IsoDataFactory.Make<IItem>(m => m.Set(x => x.Identification).To("I-1")),
                IsoDataFactory.Make<IItem>(m => m.Set(x => x.Identification).To("I-2")),
            ]));

        Assert.Equal(["I-1", "I-2"], batch.Item.Select(r => r.Identification.Value));
    }

    [Fact]
    public void TwoObjectsWithTheSameMembers_AreEqual()
    {
        IIdentification Make() => IsoDataFactory.Make<IIdentification>(b => b.Set(x => x.Identification).To("ID-1"));

        Assert.Equal(Make(), Make());
        Assert.NotSame(Make(), Make());
    }

    [Fact]
    public void AListGivenByHand_IsComparedByItsItems_LikeAListRead()
    {
        IBatch Make(IEnumerable<IItem> items) => IsoDataFactory.Make<IBatch>(b => b.Set(x => x.Item).To(items));
        IItem Item(string id) => IsoDataFactory.Make<IItem>(m => m.Set(x => x.Identification).To(id));

        Assert.Equal(Make([Item("I-1"), Item("I-2")]), Make(new List<IItem> { Item("I-1"), Item("I-2") }));
        Assert.NotEqual(Make([Item("I-1")]), Make([Item("I-2")]));
    }

    [Fact]
    public void AnExpressionThatNamesNoMember_IsRefused()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            IsoDataFactory.Make<IIdentification>(b => b.Set(x => x.Identification.Value.Length.ToString()).To("x")));

        Assert.Contains("member", ex.Message);
    }

    [Fact]
    public void ASetThatIsNeverCompleted_ChangesNothing()
    {
        var id = IsoDataFactory.Make<IIdentification>(b => { _ = b.Set(x => x.Issuer); });

        Assert.Null(id.Issuer);
    }

    [Fact]
    public void With_CopiesAnObjectAndChangesTheNamedMembers_LeavingTheSourceAlone()
    {
        var source = IsoDataFactory.Make<IIdentification>(b => b
            .Set(x => x.Identification).To("ID-1")
            .Set(x => x.Issuer).To("BANK"));

        var changed = IsoDataFactory.With(source, b => b.Set(x => x.Identification).To("ID-2"));

        Assert.Equal("ID-2", changed.Identification.Value);
        Assert.Equal("BANK", changed.Issuer!.Value.Value);
        Assert.Equal("ID-1", source.Identification.Value);
        Assert.NotSame(source, changed);
        Assert.Equal(source, IsoDataFactory.With(source, _ => { }));
    }

    [Fact]
    public void With_AlsoCopiesAnObjectOfAnotherClass()
    {
        IIdentification source = new MyIdentification();

        var changed = IsoDataFactory.With(source, b => b.Set(x => x.Issuer).To("BANK"));

        Assert.Equal("BANK", changed.Issuer!.Value.Value);
        Assert.Null(source.Issuer);
    }

    /// <summary>An implementation of the interface that is not made by the factory; like a record, its members can be set.</summary>
    private sealed class MyIdentification : IIdentification
    {
        public Max35Text Identification { get; init; } = "ID-1";
        public Max35Text? Issuer { get; init; }
    }
}
