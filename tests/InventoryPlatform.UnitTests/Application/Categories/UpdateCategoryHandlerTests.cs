using FluentValidation;
using FluentValidation.Results;
using InventoryPlatform.Application.Features.Categories;
using InventoryPlatform.Application.Features.Categories.UpdateCategory;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;
using Xunit;

namespace InventoryPlatform.UnitTests.Application.Categories;

public sealed class UpdateCategoryHandlerTests
{
    // =====================================================================
    // Not found behavior (repository-owned after validation)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_NotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var request = new UpdateCategoryRequest(
            Id: 999,
            Name: "Valid Name",
            Description: "Description");

        var categoryRepository = new FakeCategoryRepository(
            getByIdResult: null);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new UpdateCategoryHandler(
            categoryRepository,
            unitOfWork,
            new UpdateCategoryValidator());

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryErrors.NotFound, result.Error);

        Assert.Equal(1, categoryRepository.GetByIdAsyncCallCount);
        Assert.Equal(999, categoryRepository.LastGetByIdAsyncRequestId);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_NotFound_NoSave()
    {
        // Arrange
        var request = new UpdateCategoryRequest(
            Id: 999,
            Name: "Valid Name",
            Description: "Description");

        var categoryRepository = new FakeCategoryRepository(
            getByIdResult: null);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new UpdateCategoryHandler(
            categoryRepository,
            unitOfWork,
            new UpdateCategoryValidator());

        // Act
        _ = await handler.HandleAsync(request);

        // Assert
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // Successful update
    // =====================================================================

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsSuccessWithUpdatedCategory()
    {
        // Arrange
        var existingCategory = new Category("Original Name", "Original Description");
        existingCategory.GetType().GetProperty("Id")?.SetValue(existingCategory, 1);
        
        var request = new UpdateCategoryRequest(
            Id: 1,
            Name: "Updated Name",
            Description: "Updated Description");

        var categoryRepository = new FakeCategoryRepository(
            getByIdResult: existingCategory);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new UpdateCategoryHandler(
            categoryRepository,
            unitOfWork,
            new UpdateCategoryValidator());

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);

        var response = result.Value!;
        Assert.Equal(1, response.Id);
        Assert.Equal("Updated Name", response.Name);

        Assert.Equal("Updated Name", existingCategory.Name);
        Assert.Equal("Updated Description", existingCategory.Description);

        Assert.Equal(1, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // T02: Application validation invoked first (frozen A1 contract per DD-046)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_InvalidId_ReturnsValidationFailureWithNoRepositoryInteraction()
    {
        // Arrange
        var request = new UpdateCategoryRequest(
            Id: 0,
            Name: "Valid Name",
            Description: "Description");

        var categoryRepository = new FakeCategoryRepository(
            getByIdResult: new Category("Existing", "Desc"));
        var unitOfWork = new FakeUnitOfWork();

        var handler = new UpdateCategoryHandler(
            categoryRepository,
            unitOfWork,
            new UpdateCategoryValidator());

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Category.Validation", result.Error.Code);
        Assert.Equal("'Id' must be greater than '0'.", result.Error.Message);

        Assert.Equal(0, categoryRepository.GetByIdAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_InvalidNameEmpty_ReturnsValidationFailureWithNoRepositoryInteraction()
    {
        // Arrange
        var request = new UpdateCategoryRequest(
            Id: 1,
            Name: "",
            Description: "Description");

        var categoryRepository = new FakeCategoryRepository(
            getByIdResult: new Category("Existing", "Desc"));
        var unitOfWork = new FakeUnitOfWork();

        var handler = new UpdateCategoryHandler(
            categoryRepository,
            unitOfWork,
            new UpdateCategoryValidator());

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Category.Validation", result.Error.Code);
        Assert.Equal("'Name' must not be empty.", result.Error.Message);

        Assert.Equal(0, categoryRepository.GetByIdAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_InvalidNameTooLong_ReturnsValidationFailureWithNoRepositoryInteraction()
    {
        // Arrange
        var request = new UpdateCategoryRequest(
            Id: 1,
            Name: new string('x', 101),
            Description: "Description");

        var categoryRepository = new FakeCategoryRepository(
            getByIdResult: new Category("Existing", "Desc"));
        var unitOfWork = new FakeUnitOfWork();

        var handler = new UpdateCategoryHandler(
            categoryRepository,
            unitOfWork,
            new UpdateCategoryValidator());

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Category.Validation", result.Error.Code);
        Assert.Contains("100 characters or fewer", result.Error.Message);

        Assert.Equal(0, categoryRepository.GetByIdAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_ValidationFailure_MessageIsExactlyFirstFluentValidationFailure()
    {
        // Arrange
        var request = new UpdateCategoryRequest(
            Id: 0,
            Name: new string('x', 101),
            Description: "Description");

        var expectedMessage = new UpdateCategoryValidator()
            .Validate(request)
            .Errors[0]
            .ErrorMessage;

        var categoryRepository = new FakeCategoryRepository(
            getByIdResult: new Category("Existing", "Desc"));
        var unitOfWork = new FakeUnitOfWork();

        var handler = new UpdateCategoryHandler(
            categoryRepository,
            unitOfWork,
            new UpdateCategoryValidator());

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Category.Validation", result.Error.Code);
        Assert.Equal(expectedMessage, result.Error.Message);
        Assert.Equal(0, categoryRepository.GetByIdAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_IdAndNameFailures_IdMessageIsSelectedFirst()
    {
        // Arrange: Id and Name both fail - Id rule declared first
        var request = new UpdateCategoryRequest(
            Id: 0,
            Name: new string('x', 101),
            Description: "Description");

        var categoryRepository = new FakeCategoryRepository(
            getByIdResult: new Category("Existing", "Desc"));
        var unitOfWork = new FakeUnitOfWork();

        var handler = new UpdateCategoryHandler(
            categoryRepository,
            unitOfWork,
            new UpdateCategoryValidator());

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Category.Validation", result.Error.Code);
        Assert.Equal("'Id' must be greater than '0'.", result.Error.Message);
        Assert.Equal(0, categoryRepository.GetByIdAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_ForwardsSameCancellationTokenToValidatorAndDownstreamOperations()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var existingCategory = new Category("Original Name", "Original Description");
        var request = new UpdateCategoryRequest(
            Id: 1,
            Name: "Updated Name",
            Description: "Updated Description");

        var callOrder = new CallOrder();
        var recordingValidator = new RecordingValidator(new UpdateCategoryValidator(), callOrder);
        var categoryRepository = new FakeCategoryRepository(
            getByIdResult: existingCategory);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new UpdateCategoryHandler(
            categoryRepository,
            unitOfWork,
            recordingValidator);

        // Act
        var result = await handler.HandleAsync(request, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(1, recordingValidator.ValidateAsyncCallCount);
        Assert.Equal(cancellationToken, recordingValidator.LastCancellationToken);
        Assert.Same(request, recordingValidator.LastRequest);

        Assert.Equal(cancellationToken, categoryRepository.LastGetByIdAsyncCancellationToken);
        Assert.Equal(cancellationToken, unitOfWork.LastSaveChangesAsyncCancellationToken);
    }

    // =====================================================================
    // Call order: validation → GetById → SaveChangesAsync
    // =====================================================================

    [Fact]
    public async Task HandleAsync_SuccessfulUpdate_CallOrderIsCorrect()
    {
        // Arrange
        var callOrder = new CallOrder();
        var existingCategory = new Category("Original Name", "Original Description");
        var request = new UpdateCategoryRequest(
            Id: 1,
            Name: "Updated Name",
            Description: "Updated Description");

        var categoryRepository = new FakeCategoryRepository(
            getByIdResult: existingCategory,
            callOrder: callOrder);
        var unitOfWork = new FakeUnitOfWork(callOrder: callOrder);
        var recordingValidator = new RecordingValidator(new UpdateCategoryValidator(), callOrder);

        var handler = new UpdateCategoryHandler(
            categoryRepository,
            unitOfWork,
            recordingValidator);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var eventList = callOrder.Events.ToList();
        Assert.Contains("Validator.ValidateAsync", eventList);
        Assert.Contains("CategoryRepository.GetByIdAsync", eventList);
        Assert.Contains("UnitOfWork.SaveChangesAsync", eventList);

        var validatorIndex = eventList.IndexOf("Validator.ValidateAsync");
        var getIndex = eventList.IndexOf("CategoryRepository.GetByIdAsync");
        var saveIndex = eventList.IndexOf("UnitOfWork.SaveChangesAsync");

        Assert.True(validatorIndex >= 0);
        Assert.True(getIndex >= 0);
        Assert.True(saveIndex >= 0);
        Assert.True(validatorIndex < getIndex);
        Assert.True(getIndex < saveIndex);
    }

    [Fact]
    public async Task HandleAsync_ValidationFailure_NoRepositoryCalls()
    {
        // Arrange
        var callOrder = new CallOrder();
        var request = new UpdateCategoryRequest(
            Id: 0,
            Name: "Valid Name",
            Description: "Description");

        var categoryRepository = new FakeCategoryRepository(
            getByIdResult: new Category("Existing", "Desc"),
            callOrder: callOrder);
        var unitOfWork = new FakeUnitOfWork(callOrder: callOrder);
        var recordingValidator = new RecordingValidator(new UpdateCategoryValidator(), callOrder);

        var handler = new UpdateCategoryHandler(
            categoryRepository,
            unitOfWork,
            recordingValidator);

        // Act
        _ = await handler.HandleAsync(request);

        // Assert
        var eventList = callOrder.Events.ToList();
        Assert.Contains("Validator.ValidateAsync", eventList);
        Assert.DoesNotContain("CategoryRepository.GetByIdAsync", eventList);
        Assert.DoesNotContain("UnitOfWork.SaveChangesAsync", eventList);
    }

    [Fact]
    public async Task HandleAsync_NotFound_NoSave_CallOrder()
    {
        // Arrange
        var callOrder = new CallOrder();
        var request = new UpdateCategoryRequest(
            Id: 999,
            Name: "Valid Name",
            Description: "Description");

        var categoryRepository = new FakeCategoryRepository(
            getByIdResult: null,
            callOrder: callOrder);
        var unitOfWork = new FakeUnitOfWork(callOrder: callOrder);
        var recordingValidator = new RecordingValidator(new UpdateCategoryValidator(), callOrder);

        var handler = new UpdateCategoryHandler(
            categoryRepository,
            unitOfWork,
            recordingValidator);

        // Act
        _ = await handler.HandleAsync(request);

        // Assert
        var eventList = callOrder.Events.ToList();
        Assert.Contains("Validator.ValidateAsync", eventList);
        Assert.Contains("CategoryRepository.GetByIdAsync", eventList);
        Assert.DoesNotContain("UnitOfWork.SaveChangesAsync", eventList);
    }

    // =====================================================================
    // Local/private fakes (not promoted to shared support - Rule of Three)
    // =====================================================================

    public sealed class FakeCategoryRepository : ICategoryRepository
    {
        private readonly Category? _getByIdResult;
        private readonly CallOrder? _callOrder;

        public FakeCategoryRepository(
            Category? getByIdResult,
            CallOrder? callOrder = null)
        {
            _getByIdResult = getByIdResult;
            _callOrder = callOrder;
        }

        // --- Interaction recording ---

        public int GetByIdAsyncCallCount { get; private set; }
        public List<int> GetByIdAsyncRequests { get; } = new();
        public int? LastGetByIdAsyncRequestId =>
            GetByIdAsyncRequests.Count > 0 ? GetByIdAsyncRequests[^1] : null;
        public CancellationToken LastGetByIdAsyncCancellationToken { get; private set; }

        public int ExistsByNameAsyncCallCount { get; private set; }
        public string? LastExistsByNameAsyncRequestName { get; private set; }
        public CancellationToken LastExistsByNameAsyncCancellationToken { get; private set; }

        // --- ICategoryRepository / IRepository<Category> ---

        public Task<Category?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            GetByIdAsyncCallCount++;
            GetByIdAsyncRequests.Add(id);
            LastGetByIdAsyncCancellationToken = cancellationToken;
            _callOrder?.Record("CategoryRepository.GetByIdAsync");
            return Task.FromResult(_getByIdResult);
        }

        public Task<IReadOnlyList<Category>> GetAllAsync(
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeCategoryRepository does not support GetAllAsync");

        public Task<IReadOnlyList<Category>> FindAsync(
            System.Linq.Expressions.Expression<Func<Category, bool>> predicate,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeCategoryRepository does not support FindAsync");

        public Task AddAsync(
            Category entity,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeCategoryRepository does not support AddAsync");

        public void Update(Category entity)
        {
            _callOrder?.Record("CategoryRepository.Update");
            // No-op for test purposes - the handler updates the entity in place
        }

        public void Remove(Category entity)
            => throw new NotSupportedException(
                "FakeCategoryRepository does not support Remove");

        public Task<bool> ExistsAsync(
            System.Linq.Expressions.Expression<Func<Category, bool>> predicate,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeCategoryRepository does not support ExistsAsync");

        public Task<bool> ExistsByNameAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            ExistsByNameAsyncCallCount++;
            LastExistsByNameAsyncRequestName = name;
            LastExistsByNameAsyncCancellationToken = cancellationToken;
            _callOrder?.Record("CategoryRepository.ExistsByNameAsync");
            return Task.FromResult(false);
        }

        public Task<PagedResult<Category>> GetPagedAsync(
            PagedQuery request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeCategoryRepository does not support GetPagedAsync");
    }

    public sealed class FakeUnitOfWork : IUnitOfWork
    {
        private readonly int _saveChangesResult;
        private readonly CallOrder? _callOrder;

        public FakeUnitOfWork(
            CallOrder? callOrder = null,
            int saveChangesResult = 1)
        {
            _callOrder = callOrder;
            _saveChangesResult = saveChangesResult;
        }

        public int SaveChangesAsyncCallCount { get; private set; }
        public CancellationToken LastSaveChangesAsyncCancellationToken { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesAsyncCallCount++;
            LastSaveChangesAsyncCancellationToken = cancellationToken;
            _callOrder?.Record("UnitOfWork.SaveChangesAsync");
            return Task.FromResult(_saveChangesResult);
        }
    }

    public sealed class CallOrder
    {
        private readonly List<string> _events = new();
        public IReadOnlyList<string> Events => _events;
        public void Record(string interaction) => _events.Add(interaction);
    }

    public sealed class RecordingValidator : IValidator<UpdateCategoryRequest>
    {
        private readonly IValidator<UpdateCategoryRequest> _inner;
        private readonly CallOrder? _callOrder;

        public RecordingValidator(IValidator<UpdateCategoryRequest> inner, CallOrder? callOrder = null)
        {
            _inner = inner;
            _callOrder = callOrder;
        }

        public int ValidateAsyncCallCount { get; private set; }
        public CancellationToken LastCancellationToken { get; private set; }
        public UpdateCategoryRequest? LastRequest { get; private set; }

        public bool CanValidateInstancesOfType(Type instanceType)
            => _inner.CanValidateInstancesOfType(instanceType);

        public IValidatorDescriptor CreateDescriptor() => _inner.CreateDescriptor();

        public ValidationResult Validate(UpdateCategoryRequest instance)
            => _inner.Validate(instance);

        public Task<ValidationResult> ValidateAsync(
            UpdateCategoryRequest instance,
            CancellationToken cancellation = default)
        {
            ValidateAsyncCallCount++;
            LastRequest = instance;
            LastCancellationToken = cancellation;
            _callOrder?.Record("Validator.ValidateAsync");
            return _inner.ValidateAsync(instance, cancellation);
        }

        public ValidationResult Validate(IValidationContext context)
            => _inner.Validate(context);

        public Task<ValidationResult> ValidateAsync(
            IValidationContext context,
            CancellationToken cancellation = default)
            => _inner.ValidateAsync(context, cancellation);
    }
}