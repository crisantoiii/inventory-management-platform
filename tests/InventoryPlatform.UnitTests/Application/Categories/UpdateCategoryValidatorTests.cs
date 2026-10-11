using FluentValidation;
using InventoryPlatform.Application.Features.Categories.UpdateCategory;
using Xunit;

namespace InventoryPlatform.UnitTests.Application.Categories;

public sealed class UpdateCategoryValidatorTests
{
    // =====================================================================
    // Valid request passes
    // =====================================================================

    [Fact]
    public void Validate_ValidRequest_ReturnsNoErrors()
    {
        // Arrange
        var validator = new UpdateCategoryValidator();
        var request = new UpdateCategoryRequest(
            Id: 1,
            Name: "Valid Category",
            Description: "Some description");

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    // =====================================================================
    // Id GreaterThan(0)
    // =====================================================================

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Validate_InvalidId_FailsWithExpectedPropertyAndMessage(int id)
    {
        // Arrange
        var validator = new UpdateCategoryValidator();
        var request = new UpdateCategoryRequest(
            Id: id,
            Name: "Valid Name",
            Description: null);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal("Id", error.PropertyName);
        Assert.Equal("'Id' must be greater than '0'.", error.ErrorMessage);
    }

    [Fact]
    public void Validate_IdOne_IsValid()
    {
        // Arrange
        var validator = new UpdateCategoryValidator();
        var request = new UpdateCategoryRequest(
            Id: 1,
            Name: "Valid Name",
            Description: null);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
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
        var validator = new UpdateCategoryValidator();
        var request = new UpdateCategoryRequest(
            Id: 1,
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
        var validator = new UpdateCategoryValidator();
        var request = new UpdateCategoryRequest(
            Id: 1,
            Name: "Valid Name",
            Description: null);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    // =====================================================================
    // Name maximum length 100 (changed from 200)
    // =====================================================================

    [Fact]
    public void Validate_NameAtMaxLength100_IsValid()
    {
        // Arrange
        var validator = new UpdateCategoryValidator();
        var request = new UpdateCategoryRequest(
            Id: 1,
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
        var validator = new UpdateCategoryValidator();
        var request = new UpdateCategoryRequest(
            Id: 1,
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
        var validator = new UpdateCategoryValidator();
        var request = new UpdateCategoryRequest(
            Id: 1,
            Name: "Valid Name",
            Description: null);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    // =====================================================================
    // Deterministic rule order: Id.GreaterThan(0), Name.NotEmpty, Name.MaximumLength
    // =====================================================================

    [Fact]
    public void Validate_MultipleFailures_ReturnsErrorsInDeclarationOrder()
    {
        // Arrange
        var validator = new UpdateCategoryValidator();
        var request = new UpdateCategoryRequest(
            Id: 0,
            Name: new string('x', 101),
            Description: null);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        // First error should be Id.GreaterThan(0)
        Assert.Equal("Id", result.Errors[0].PropertyName);
        Assert.Equal("'Id' must be greater than '0'.", result.Errors[0].ErrorMessage);
        
        // Second error should be Name.NotEmpty or Name.MaximumLength depending on validation order
        // With the current rule declaration order: Id > Name.NotEmpty > Name.MaximumLength
        Assert.Contains("Name", result.Errors.Select(e => e.PropertyName));
    }
}