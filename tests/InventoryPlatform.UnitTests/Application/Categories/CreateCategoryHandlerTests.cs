using FluentValidation;
using FluentValidation.Results;
using InventoryPlatform.Application.Features.Categories;
using InventoryPlatform.Application.Features.Categories.CreateCategory;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;
using Xunit;

namespace InventoryPlatform.UnitTests.Application.Categories;

public sealed class CreateCategoryHandlerTests
{
    // =====================================================================
    // Duplicate name behavior (repository-owned after validation)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_DuplicateName_ReturnsDuplicateNameFailure()
    {
        // Arrange
        var request = new CreateCategoryRequest(
            Name: "Existing Category",
            Description: "Description");

        var categoryRepository = new FakeCategoryRepository(
            existsByName: true);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreateCategoryHandler(
            categoryRepository,
            unitOfWork,
            new CreateCategoryValidator());

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryErrors.DuplicateName, result.Error);

        Assert.Equal(1, categoryRepository.ExistsByNameAsyncCallCount);
        Assert.Equal("Existing Category", categoryRepository.LastExistsByNameAsyncRequestName);
        Assert.Equal(0, categoryRepository.AddAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_DuplicateName_NoAddOrSave()
    {
        // Arrange
        var request = new CreateCategoryRequest(
            Name: "Existing Category",
            Description: "Description");

        var categoryRepository = new FakeCategoryRepository(
            existsByName: true);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreateCategoryHandler(
            categoryRepository,
            unitOfWork,
            new CreateCategoryValidator());

        // Act
        _ = await handler.HandleAsync(request);

        // Assert
        Assert.Equal(0, categoryRepository.AddAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // Successful creation
    // =====================================================================

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsSuccessWithCreatedCategory()
    {
        // Arrange
        var request = new CreateCategoryRequest(
            Name: "New Category",
            Description: "Category description");

        var categoryRepository = new FakeCategoryRepository(
            existsByName: false);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreateCategoryHandler(
            categoryRepository,
            unitOfWork,
            new CreateCategoryValidator());

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);

        var response = result.Value!;
        Assert.True(response.Id > 0);
        Assert.Equal("New Category", response.Name);

        var added = categoryRepository.LastAddedCategory;
        Assert.NotNull(added);
        Assert.Equal("New Category", added!.Name);
        Assert.Equal("Category description", added.Description);
        Assert.True(added.IsActive);

        Assert.Equal(1, categoryRepository.AddAsyncCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // T02: Application validation invoked first (frozen A1 contract per DD-046)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_InvalidNameEmpty_ReturnsValidationFailureWithNoRepositoryInteraction()
    {
        // Arrange
        var request = new CreateCategoryRequest(
            Name: "",
            Description: "Description");

        var categoryRepository = new FakeCategoryRepository(
            existsByName: false);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreateCategoryHandler(
            categoryRepository,
            unitOfWork,
            new CreateCategoryValidator());

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Category.Validation", result.Error.Code);
        Assert.Equal("'Name' must not be empty.", result.Error.Message);

        Assert.Equal(0, categoryRepository.ExistsByNameAsyncCallCount);
        Assert.Equal(0, categoryRepository.AddAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_InvalidNameTooLong_ReturnsValidationFailureWithNoRepositoryInteraction()
    {
        // Arrange
        var request = new CreateCategoryRequest(
            Name: new string('x', 101),
            Description: "Description");

        var categoryRepository = new FakeCategoryRepository(
            existsByName: false);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreateCategoryHandler(
            categoryRepository,
            unitOfWork,
            new CreateCategoryValidator());

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Category.Validation", result.Error.Code);
        Assert.Contains("100 characters or fewer", result.Error.Message);

        Assert.Equal(0, categoryRepository.ExistsByNameAsyncCallCount);
        Assert.Equal(0, categoryRepository.AddAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_ValidationFailure_MessageIsExactlyFirstFluentValidationFailure()
    {
        // Arrange
        var request = new CreateCategoryRequest(
            Name: new string('x', 101),
            Description: "Description");

        var expectedMessage = new CreateCategoryValidator()
            .Validate(request)
            .Errors[0]
            .ErrorMessage;

        var categoryRepository = new FakeCategoryRepository(
            existsByName: false);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreateCategoryHandler(
            categoryRepository,
            unitOfWork,
            new CreateCategoryValidator());

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Category.Validation", result.Error.Code);
        Assert.Equal(expectedMessage, result.Error.Message);
        Assert.Equal(0, categoryRepository.ExistsByNameAsyncCallCount);
        Assert.Equal(0, categoryRepository.AddAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_ForwardsSameCancellationTokenToValidatorAndDownstreamOperations()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var request = new CreateCategoryRequest(
            Name: "Valid Category",
            Description: "Description");

        var callOrder = new CallOrder();
        var recordingValidator = new RecordingValidator(new CreateCategoryValidator(), callOrder);
        var categoryRepository = new FakeCategoryRepository(
            existsByName: false);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreateCategoryHandler(
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

        Assert.Equal(cancellationToken, categoryRepository.LastExistsByNameAsyncCancellationToken);
        Assert.Equal(cancellationToken, categoryRepository.LastAddAsyncCancellationToken);
        Assert.Equal(cancellationToken, unitOfWork.LastSaveChangesAsyncCancellationToken);
    }

    // =====================================================================
    // Call order: validation → ExistsByName → AddAsync → SaveChangesAsync
    // =====================================================================

    [Fact]
    public async Task HandleAsync_SuccessfulCreation_CallOrderIsCorrect()
    {
        // Arrange
        var callOrder = new CallOrder();
        var request = new CreateCategoryRequest(
            Name: "Valid Category",
            Description: "Description");

        var categoryRepository = new FakeCategoryRepository(
            existsByName: false,
            callOrder: callOrder);
        var unitOfWork = new FakeUnitOfWork(callOrder: callOrder);
        var recordingValidator = new RecordingValidator(new CreateCategoryValidator(), callOrder);

        var handler = new CreateCategoryHandler(
            categoryRepository,
            unitOfWork,
            recordingValidator);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var eventList = callOrder.Events.ToList();
        Assert.Contains("Validator.ValidateAsync", eventList);
        Assert.Contains("CategoryRepository.ExistsByNameAsync", eventList);
        Assert.Contains("CategoryRepository.AddAsync", eventList);
        Assert.Contains("UnitOfWork.SaveChangesAsync", eventList);

        var validatorIndex = eventList.IndexOf("Validator.ValidateAsync");
        var existsIndex = eventList.IndexOf("CategoryRepository.ExistsByNameAsync");
        var addIndex = eventList.IndexOf("CategoryRepository.AddAsync");
        var saveIndex = eventList.IndexOf("UnitOfWork.SaveChangesAsync");

        Assert.True(validatorIndex >= 0);
        Assert.True(existsIndex >= 0);
        Assert.True(addIndex >= 0);
        Assert.True(saveIndex >= 0);
        Assert.True(validatorIndex < existsIndex);
        Assert.True(existsIndex < addIndex);
        Assert.True(addIndex < saveIndex);
    }

    [Fact]
    public async Task HandleAsync_ValidationFailure_NoRepositoryCalls()
    {
        // Arrange
        var callOrder = new CallOrder();
        var request = new CreateCategoryRequest(
            Name: "",
            Description: "Description");

        var categoryRepository = new FakeCategoryRepository(
            existsByName: false,
            callOrder: callOrder);
        var unitOfWork = new FakeUnitOfWork(callOrder: callOrder);
        var recordingValidator = new RecordingValidator(new CreateCategoryValidator(), callOrder);

        var handler = new CreateCategoryHandler(
            categoryRepository,
            unitOfWork,
            recordingValidator);

        // Act
        _ = await handler.HandleAsync(request);

        // Assert
        var eventList = callOrder.Events.ToList();
        Assert.Contains("Validator.ValidateAsync", eventList);
        Assert.DoesNotContain("CategoryRepository.ExistsByNameAsync", eventList);
        Assert.DoesNotContain("CategoryRepository.AddAsync", eventList);
        Assert.DoesNotContain("UnitOfWork.SaveChangesAsync", eventList);
    }

    [Fact]
    public async Task HandleAsync_DuplicateName_NoAddOrSave_CallOrder()
    {
        // Arrange
        var callOrder = new CallOrder();
        var request = new CreateCategoryRequest(
            Name: "Existing Category",
            Description: "Description");

        var categoryRepository = new FakeCategoryRepository(
            existsByName: true,
            callOrder: callOrder);
        var unitOfWork = new FakeUnitOfWork(callOrder: callOrder);
        var recordingValidator = new RecordingValidator(new CreateCategoryValidator(), callOrder);

        var handler = new CreateCategoryHandler(
            categoryRepository,
            unitOfWork,
            recordingValidator);

        // Act
        _ = await handler.HandleAsync(request);

        // Assert
        var eventList = callOrder.Events.ToList();
        Assert.Contains("Validator.ValidateAsync", eventList);
        Assert.Contains("CategoryRepository.ExistsByNameAsync", eventList);
        Assert.DoesNotContain("CategoryRepository.AddAsync", eventList);
        Assert.DoesNotContain("UnitOfWork.SaveChangesAsync", eventList);
    }

    // =====================================================================
    // Local/private fakes (not promoted to shared support - Rule of Three)
    // =====================================================================

    public sealed class FakeCategoryRepository : ICategoryRepository
    {
        private readonly bool _existsByName;
        private readonly CallOrder? _callOrder;

        public FakeCategoryRepository(
            bool existsByName,
            CallOrder? callOrder = null)
        {
            _existsByName = existsByName;
            _callOrder = callOrder;
        }

        // --- Interaction recording ---

        public int ExistsByNameAsyncCallCount { get; private set; }
        public string? LastExistsByNameAsyncRequestName { get; private set; }
        public CancellationToken LastExistsByNameAsyncCancellationToken { get; private set; }

        public int AddAsyncCallCount { get; private set; }
        public List<Category> AddedCategories { get; } = new();
        public CancellationToken LastAddAsyncCancellationToken { get; private set; }
        public Category? LastAddedCategory =>
            AddedCategories.Count > 0 ? AddedCategories[^1] : null;

        public int GetByIdAsyncCallCount { get; private set; }
        public List<int> GetByIdAsyncRequests { get; } = new();
        public CancellationToken LastGetByIdAsyncCancellationToken { get; private set; }
        public Category? LastGottenByIdCategory { get; private set; }

        // --- ICategoryRepository / IRepository<Category> ---

        public Task<Category?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            GetByIdAsyncCallCount++;
            GetByIdAsyncRequests.Add(id);
            LastGetByIdAsyncCancellationToken = cancellationToken;
            _callOrder?.Record("CategoryRepository.GetByIdAsync");
            return Task.FromResult<Category?>(null);
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
        {
            AddAsyncCallCount++;
            AddedCategories.Add(entity);
            LastAddAsyncCancellationToken = cancellationToken;
            _callOrder?.Record("CategoryRepository.AddAsync");
            entity.GetType().GetProperty("Id")?.SetValue(entity, AddedCategories.Count);
            return Task.CompletedTask;
        }

        public void Update(Category entity)
            => throw new NotSupportedException(
                "FakeCategoryRepository does not support Update");

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
            return Task.FromResult(_existsByName);
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

    public sealed class RecordingValidator : IValidator<CreateCategoryRequest>
    {
        private readonly IValidator<CreateCategoryRequest> _inner;
        private readonly CallOrder? _callOrder;

        public RecordingValidator(IValidator<CreateCategoryRequest> inner, CallOrder? callOrder = null)
        {
            _inner = inner;
            _callOrder = callOrder;
        }

        public int ValidateAsyncCallCount { get; private set; }
        public CancellationToken LastCancellationToken { get; private set; }
        public CreateCategoryRequest? LastRequest { get; private set; }

        public bool CanValidateInstancesOfType(Type instanceType)
            => _inner.CanValidateInstancesOfType(instanceType);

        public IValidatorDescriptor CreateDescriptor() => _inner.CreateDescriptor();

        public ValidationResult Validate(CreateCategoryRequest instance)
            => _inner.Validate(instance);

        public Task<ValidationResult> ValidateAsync(
            CreateCategoryRequest instance,
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