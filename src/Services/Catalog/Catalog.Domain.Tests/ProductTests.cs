using FluentAssertions;
using Catalog.Domain.Entities;

namespace Catalog.Domain.Tests;

public class ProductTests
{
    [Fact]
    public void Create_WithValidNameAndPrice_SetsPropertiesAndDefaultsActiveToTrue()
    {
        var product = Product.Create("Wireless Mouse", 25.00m);

        product.Name.Should().Be("Wireless Mouse");
        product.Price.Should().Be(25.00m);
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithIsActiveFalse_RespectsOverride()
    {
        var product = Product.Create("Discontinued Widget", 10.00m, isActive: false);

        product.IsActive.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithMissingName_Throws(string? name)
    {
        var act = () => Product.Create(name!, 10.00m);

        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void Create_WithNegativePrice_Throws()
    {
        var act = () => Product.Create("Widget", -1.00m);

        act.Should().Throw<ArgumentException>().WithParameterName("price");
    }
}
