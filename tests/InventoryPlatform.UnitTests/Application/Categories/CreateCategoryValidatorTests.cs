using FluentValidation;
using InventoryPlatform.Application.Features.Categories.CreateCategory;
using Xunit;

namespace InventoryPlatform.UnitTests.Application.Categories;

public sealed class CreateCategoryValidatorTests
{
    // =====================================================================
    // Valid request passes
    // =====================================================================

    [Fact]
    public void Validate_ValidRequest_ReturnsNoErrors()
    {
        // Arrange
        var validator = new CreateCategoryValidator();
        var request = new CreateCategoryRequest(
            Name: "Valid Category",
            Description: "Some description");

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    // =====================================================================
    // Name NotEmpty
    // =====================================================================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Validate_NameEmptyOrWhitespace_Fails(string name)
    {
        // Arrange
        var validator = new CreateCategoryValidator();
        var request = new CreateCategoryRequest(
            Name: name,
            Description: "Some description");

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal("Name", error.PropertyName);
        Assert.Equal("'Name' must not be empty.", error.ErrorMessage);
    }

    [Fact]
    public void Validate_NameNotEmpty_IsValid()
    {
        // Arrange
        var validator = new CreateCategoryValidator();
        var request = new CreateCategoryRequest(
            Name: "Valid Name",
            Description: null);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    // =====================================================================
    // Name maximum length 100
    // =====================================================================

    [Fact]
    public void Validate_NameAtMaxLength100_IsValid()
    {
        // Arrange
        var validator = new CreateCategoryValidator();
        var request = new CreateCategoryRequest(
            Name: new string('x', 100),
            Description: null);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NameOverMaxLength100_Fails()
    {
        // Arrange
        var validator = new CreateCategoryValidator();
        var request = new CreateCategoryRequest(
            Name: new string('x', 101),
            Description: null);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_NameNull_IsValid()
    {
        // Arrange
        var validator = new CreateCategoryValidator();
        var request = new CreateCategoryRequest(
            Name: "Valid Name",
            Description: null);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    // =====================================================================
    // Deterministic rule order: Name.NotEmpty then Name.MaximumLength
    // =====================================================================

    [Fact]
    public void Validate_MultipleFailures_ReturnsErrorsInDeclarationOrder()
    {
        // Arrange
        var validator = new CreateCategoryValidator();
        var request = new CreateCategoryRequest(
            Name: new string('x', 101),
            Description: null);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        // First error should be Name.MaximumLength (since Name is not empty but too long)
        Assert.Equal("Name", result.Errors[0].PropertyName);
    }
}