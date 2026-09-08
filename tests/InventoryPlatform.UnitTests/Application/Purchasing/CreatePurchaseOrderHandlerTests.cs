using InventoryPlatform.Application.Features.Purchasing;
using InventoryPlatform.Application.Features.Purchasing.CreatePurchaseOrder;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Domain.Exceptions;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;
using InventoryPlatform.UnitTests.TestSupport.Purchasing;
using Xunit;

namespace InventoryPlatform.UnitTests.Application.Purchasing;

public sealed class CreatePurchaseOrderHandlerTests
{
    // =====================================================================
    // Supplier behavior (local/private FakeSupplierRepository)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_SupplierNotFound_ReturnsSupplierNotFoundFailure()
    {
        // Arrange
        var supplierId = 1;
        var request = CreateValidRequest(supplierId: supplierId, itemProductId: 101);

        var supplierRepository = new FakeSupplierRepository(supplierId, found: false);
        var productRepository = new FakeProductRepository(0, null, false);
        var purchaseOrderRepository = new FakePurchaseOrderRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreatePurchaseOrderHandler(
            purchaseOrderRepository,
            supplierRepository,
            productRepository,
            unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(SupplierNotFound, result.Error);

        Assert.Equal(1, supplierRepository.GetByIdAsyncCallCount);
        Assert.Equal(supplierId, supplierRepository.LastGetByIdAsyncRequestId);
        Assert.Equal(0, productRepository.GetByIdAsyncCallCount);
        Assert.Equal(0, purchaseOrderRepository.AddAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_SupplierNotFound_NoYieldPurchaseOrderAddOrSave()
    {
        // Arrange
        var request = CreateValidRequest(supplierId: 1, itemProductId: 101);

        var supplierRepository = new FakeSupplierRepository(1, found: false);
        var purchaseOrderRepository = new FakePurchaseOrderRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreatePurchaseOrderHandler(
            purchaseOrderRepository,
            supplierRepository,
            new FakeProductRepository(0, null, false),
            unitOfWork);

        // Act
        _ = await handler.HandleAsync(request);

        // Assert
        Assert.Equal(0, purchaseOrderRepository.AddAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_SupplierInactive_ReturnsSupplierInactiveFailure()
    {
        // Arrange
        var supplier = PurchasingTestData.CreateSupplier(id: 1, name: "Inactive Supplier");
        supplier.Deactivate();

        var request = CreateValidRequest(supplierId: 1, itemProductId: 101);

        var supplierRepository = new FakeSupplierRepository(1, supplier, found: true);
        var purchaseOrderRepository = new FakePurchaseOrderRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreatePurchaseOrderHandler(
            purchaseOrderRepository,
            supplierRepository,
            new FakeProductRepository(0, null, false),
            unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(SupplierInactive, result.Error);

        Assert.Equal(0, purchaseOrderRepository.AddAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_SupplierInactive_NoYieldPurchaseOrderAddOrSave()
    {
        // Arrange
        var supplier = PurchasingTestData.CreateSupplier(id: 1, name: "Inactive Supplier");
        supplier.Deactivate();

        var supplierRepository = new FakeSupplierRepository(1, supplier, found: true);
        var purchaseOrderRepository = new FakePurchaseOrderRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = CreateHandler(
            purchaseOrderRepository,
            supplierRepository,
            unitOfWork: unitOfWork);

        var request = CreateValidRequest(supplierId: 1, itemProductId: 101);

        // Act
        _ = await handler.HandleAsync(request);

        // Assert
        Assert.Equal(0, purchaseOrderRepository.AddAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // Product behavior (local/private FakeProductRepository)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_ProductNotFound_ReturnsProductNotFoundFailure()
    {
        // Arrange
        var supplier = PurchasingTestData.CreateSupplier(id: 1, name: "Active Supplier");
        var request = CreateValidRequest(supplierId: 1, itemProductId: 101);

        var supplierRepository = new FakeSupplierRepository(1, supplier, found: true);
        var productRepository = new FakeProductRepository(101, null, false);
        var purchaseOrderRepository = new FakePurchaseOrderRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreatePurchaseOrderHandler(
            purchaseOrderRepository,
            supplierRepository,
            productRepository,
            unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(ProductNotFound(101), result.Error);

        Assert.Equal(1, productRepository.GetByIdAsyncCallCount);
        Assert.Equal(101, productRepository.LastGetByIdAsyncRequestId);
        Assert.Equal(0, purchaseOrderRepository.AddAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_ProductNotFound_NoYieldPurchaseOrderAddOrSave()
    {
        // Arrange
        var supplier = PurchasingTestData.CreateSupplier(id: 1, name: "Active Supplier");
        var supplierRepository = new FakeSupplierRepository(1, supplier, found: true);
        var productRepository = new FakeProductRepository(101, null, false);
        var purchaseOrderRepository = new FakePurchaseOrderRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreatePurchaseOrderHandler(
            purchaseOrderRepository,
            supplierRepository,
            productRepository,
            unitOfWork);

        var request = CreateValidRequest(supplierId: 1, itemProductId: 101);

        // Act
        _ = await handler.HandleAsync(request);

        // Assert
        Assert.Equal(0, purchaseOrderRepository.AddAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_ProductInactive_ReturnsProductInactiveFailure()
    {
        // Arrange
        var supplier = PurchasingTestData.CreateSupplier(id: 1, name: "Active Supplier");
        var product = PurchasingTestData.CreateProduct(id: 101, sku: "SKU-101", name: "Inactive Product");
        product.Deactivate();

        var request = CreateValidRequest(supplierId: 1, itemProductId: 101);

        var supplierRepository = new FakeSupplierRepository(1, supplier, found: true);
        var productRepository = new FakeProductRepository(101, product, true);
        var purchaseOrderRepository = new FakePurchaseOrderRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreatePurchaseOrderHandler(
            purchaseOrderRepository,
            supplierRepository,
            productRepository,
            unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(ProductInactive(101), result.Error);

        Assert.Equal(1, productRepository.GetByIdAsyncCallCount);
        Assert.Equal(101, productRepository.LastGetByIdAsyncRequestId);
        Assert.Equal(0, purchaseOrderRepository.AddAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_ProductInactive_NoYieldPurchaseOrderAddOrSave()
    {
        // Arrange
        var supplier = PurchasingTestData.CreateSupplier(id: 1, name: "Active Supplier");
        var product = PurchasingTestData.CreateProduct(id: 101, sku: "SKU-101", name: "Inactive Product");
        product.Deactivate();

        var supplierRepository = new FakeSupplierRepository(1, supplier, found: true);
        var productRepository = new FakeProductRepository(101, product, true);
        var purchaseOrderRepository = new FakePurchaseOrderRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreatePurchaseOrderHandler(
            purchaseOrderRepository,
            supplierRepository,
            productRepository,
            unitOfWork);

        var request = CreateValidRequest(supplierId: 1, itemProductId: 101);

        // Act
        _ = await handler.HandleAsync(request);

        // Assert
        Assert.Equal(0, purchaseOrderRepository.AddAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // Product lookup proceeds per item; first failing item short-circuits
    // =====================================================================

    [Fact]
    public async Task HandleAsync_MultipleItems_OnlyRequestedProductIdsQueriedWhenAllValid()
    {
        // Arrange
        var supplier = PurchasingTestData.CreateSupplier(id: 1, name: "Active Supplier");
        var request = new CreatePurchaseOrderRequest(
            SupplierId: 1,
            ExpectedDeliveryDate: new DateOnly(2026, 6, 1),
            Remarks: null,
            Items: new CreatePurchaseOrderItemRequest[]
            {
                new(10, 2m, 5m),
                new(20, 1m, 10m)
            }.AsReadOnly());

        var supplierRepository = new FakeSupplierRepository(1, supplier, found: true);
        var productRepository = new FakeProductRepository(
            productId1: 10,
            product1: PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10"),
            found1: true,
            productId2: 20,
            product2: PurchasingTestData.CreateProduct(id: 20, sku: "SKU-20"),
            found2: true);
        var purchaseOrderRepository = new FakePurchaseOrderRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreatePurchaseOrderHandler(
            purchaseOrderRepository,
            supplierRepository,
            productRepository,
            unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, productRepository.GetByIdAsyncCallCount);
        Assert.Contains(10, productRepository.GetRequests);
        Assert.Contains(20, productRepository.GetRequests);
        Assert.Equal(1, purchaseOrderRepository.AddAsyncCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_MultipleItems_SecondItemProductNotFoundFailsBeforeAdding()
    {
        // Arrange
        var supplier = PurchasingTestData.CreateSupplier(id: 1, name: "Active Supplier");
        var request = new CreatePurchaseOrderRequest(
            SupplierId: 1,
            ExpectedDeliveryDate: new DateOnly(2026, 6, 1),
            Remarks: null,
            Items: new CreatePurchaseOrderItemRequest[]
            {
                new(10, 2m, 5m),
                new(999, 1m, 10m)
            }.AsReadOnly());

        var supplierRepository = new FakeSupplierRepository(1, supplier, found: true);
        var productRepository = new FakeProductRepository(
            productId1: 10,
            product1: PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10"),
            found1: true,
            productId2: 999,
            product2: null,
            found2: false);
        var purchaseOrderRepository = new FakePurchaseOrderRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreatePurchaseOrderHandler(
            purchaseOrderRepository,
            supplierRepository,
            productRepository,
            unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ProductNotFound(999), result.Error);
        Assert.Equal(0, purchaseOrderRepository.AddAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // Duplicate Product / DomainException propagation
    // =====================================================================

    [Fact]
    public async Task HandleAsync_DuplicateProductInRequest_ThrowsDomainException()
    {
        // Arrange
        var supplier = PurchasingTestData.CreateSupplier(id: 1, name: "Active Supplier");
        var request = new CreatePurchaseOrderRequest(
            SupplierId: 1,
            ExpectedDeliveryDate: new DateOnly(2026, 6, 1),
            Remarks: null,
            Items: new CreatePurchaseOrderItemRequest[]
            {
                new(10, 2m, 5m),
                new(10, 1m, 10m)
            }.AsReadOnly());

        var supplierRepository = new FakeSupplierRepository(1, supplier, found: true);
        var productRepository = new FakeProductRepository(
            productId1: 10,
            product1: PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10"),
            found1: true);
        var purchaseOrderRepository = new FakePurchaseOrderRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreatePurchaseOrderHandler(
            purchaseOrderRepository,
            supplierRepository,
            productRepository,
            unitOfWork);

        // Act / Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => handler.HandleAsync(request));

        Assert.Contains("already exists", exception.Message);
        Assert.Equal(2, productRepository.GetByIdAsyncCallCount);
        Assert.Equal(10, productRepository.LastGetByIdAsyncRequestId);
        Assert.Equal(0, purchaseOrderRepository.AddAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // Successful creation
    // =====================================================================

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsSuccessWithDraftStatusAndPreservedValues()
    {
        // Arrange
        var supplierId = 1;
        var supplier = PurchasingTestData.CreateSupplier(id: supplierId, name: "Active Supplier");
        var productId = 101;
        var product = PurchasingTestData.CreateProduct(id: productId, sku: "SKU-101", name: "Active Product");
        var expectedDeliveryDate = new DateOnly(2026, 6, 15);
        var remarks = "Urgent";
        var quantity = 5m;
        var unitCost = 12.50m;

        var request = new CreatePurchaseOrderRequest(
            SupplierId: supplierId,
            ExpectedDeliveryDate: expectedDeliveryDate,
            Remarks: remarks,
            Items: new CreatePurchaseOrderItemRequest[]
            {
                new(productId, quantity, unitCost)
            }.AsReadOnly());

        var supplierRepository = new FakeSupplierRepository(supplierId, supplier, found: true);
        var productRepository = new FakeProductRepository(productId1: productId, product1: product, found1: true);
        var purchaseOrderRepository = new FakePurchaseOrderRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreatePurchaseOrderHandler(
            purchaseOrderRepository,
            supplierRepository,
            productRepository,
            unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);

        var response = result.Value!;
        Assert.Equal(PurchaseOrderStatus.Draft, response.Status);

        var added = purchaseOrderRepository.LastAddedPurchaseOrder;
        Assert.NotNull(added);
        Assert.Equal(supplierId, added!.SupplierId);
        Assert.Equal(expectedDeliveryDate, added!.ExpectedDeliveryDate);
        Assert.Equal(remarks, added!.Remarks);
        Assert.Single(added!.Items);

        var item = added!.Items.First();
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(quantity, item.Quantity);
        Assert.Equal(unitCost, item.UnitCost);

        Assert.Equal(1, purchaseOrderRepository.AddAsyncCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_ValidRequestWithMultipleItems_ReturnsSuccessWithAllItems()
    {
        // Arrange
        var supplierId = 1;
        var supplier = PurchasingTestData.CreateSupplier(id: supplierId, name: "Active Supplier");
        var request = new CreatePurchaseOrderRequest(
            SupplierId: supplierId,
            ExpectedDeliveryDate: new DateOnly(2026, 6, 1),
            Remarks: "Multiple items",
            Items: new CreatePurchaseOrderItemRequest[]
            {
                new(10, 2m, 5m),
                new(20, 1m, 10m),
                new(30, 4m, 0m)
            }.AsReadOnly());

        var supplierRepository = new FakeSupplierRepository(supplierId, supplier, found: true);
        var productRepository = new FakeProductRepository(
            productId1: 10,
            product1: PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10"),
            found1: true,
            productId2: 20,
            product2: PurchasingTestData.CreateProduct(id: 20, sku: "SKU-20"),
            found2: true,
            productId3: 30,
            product3: PurchasingTestData.CreateProduct(id: 30, sku: "SKU-30"),
            found3: true);
        var purchaseOrderRepository = new FakePurchaseOrderRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreatePurchaseOrderHandler(
            purchaseOrderRepository,
            supplierRepository,
            productRepository,
            unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        var added = purchaseOrderRepository.LastAddedPurchaseOrder;
        Assert.NotNull(added);
        Assert.Equal(3, added!.Items.Count);

        var productIds = added!.Items.Select(i => i.ProductId).ToList();
        Assert.Contains(10, productIds);
        Assert.Contains(20, productIds);
        Assert.Contains(30, productIds);

        var item10 = Assert.Single(added!.Items, i => i.ProductId == 10);
        Assert.Equal(2m, item10.Quantity);
        Assert.Equal(5m, item10.UnitCost);

        var item20 = Assert.Single(added!.Items, i => i.ProductId == 20);
        Assert.Equal(1m, item20.Quantity);
        Assert.Equal(10m, item20.UnitCost);

        var item30 = Assert.Single(added!.Items, i => i.ProductId == 30);
        Assert.Equal(4m, item30.Quantity);
        Assert.Equal(0m, item30.UnitCost);

        Assert.Equal(1, purchaseOrderRepository.AddAsyncCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // Add-before-save ordering (test-scoped CallOrder)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_SuccessfulCreation_AddAsyncOccursBeforeSaveChangesAsync()
    {
        // Arrange
        var callOrder = new CallOrder();
        var supplierId = 1;
        var supplier = PurchasingTestData.CreateSupplier(id: supplierId, name: "Active Supplier");
        var productId = 101;
        var product = PurchasingTestData.CreateProduct(id: productId, sku: "SKU-101", name: "Active Product");

        var request = new CreatePurchaseOrderRequest(
            SupplierId: supplierId,
            ExpectedDeliveryDate: new DateOnly(2026, 6, 1),
            Remarks: null,
            Items: new CreatePurchaseOrderItemRequest[]
            {
                new(productId, 2m, 5m)
            }.AsReadOnly());

        var supplierRepository = new FakeSupplierRepository(supplierId, supplier, found: true, callOrder: callOrder);
        var productRepository = new FakeProductRepository(
            productId1: productId,
            product1: product,
            found1: true,
            callOrder: callOrder);
        var purchaseOrderRepository = new FakePurchaseOrderRepository(callOrder: callOrder);
        var unitOfWork = new FakeUnitOfWork(callOrder: callOrder);

        var handler = new CreatePurchaseOrderHandler(
            purchaseOrderRepository,
            supplierRepository,
            productRepository,
            unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var eventList = callOrder.Events.ToList();
        Assert.Contains("SupplierRepository.GetByIdAsync", eventList);
        Assert.Contains("ProductRepository.GetByIdAsync", eventList);
        Assert.Contains("PurchaseOrderRepository.AddAsync", eventList);
        Assert.Contains("UnitOfWork.SaveChangesAsync", eventList);

        var addIndex = eventList.IndexOf("PurchaseOrderRepository.AddAsync");
        var saveIndex = eventList.IndexOf("UnitOfWork.SaveChangesAsync");
        Assert.True(addIndex >= 0);
        Assert.True(saveIndex >= 0);
        Assert.True(addIndex < saveIndex);
    }

    [Fact]
    public async Task HandleAsync_SupplierNotFound_SaveNotCalledAndNoAddRecorded()
    {
        // Arrange
        var callOrder = new CallOrder();
        var request = CreateValidRequest(supplierId: 1, itemProductId: 101);

        var supplierRepository = new FakeSupplierRepository(1, found: false, callOrder: callOrder);
        var purchaseOrderRepository = new FakePurchaseOrderRepository(callOrder: callOrder);
        var unitOfWork = new FakeUnitOfWork(callOrder: callOrder);

        var handler = CreateHandler(
            purchaseOrderRepository,
            supplierRepository,
            unitOfWork: unitOfWork);

        // Act
        _ = await handler.HandleAsync(request);

        // Assert
        var eventList = callOrder.Events.ToList();
        Assert.Contains("SupplierRepository.GetByIdAsync", eventList);
        Assert.DoesNotContain("UnitOfWork.SaveChangesAsync", eventList);
        Assert.DoesNotContain("PurchaseOrderRepository.AddAsync", eventList);
    }

    // =====================================================================
    // Helpers
    // =====================================================================

    private static CreatePurchaseOrderRequest CreateValidRequest(
        int supplierId = 1,
        int itemProductId = 101,
        DateOnly? expectedDeliveryDate = null,
        string? remarks = null)
    {
        return new CreatePurchaseOrderRequest(
            SupplierId: supplierId,
            ExpectedDeliveryDate: expectedDeliveryDate ?? new DateOnly(2026, 6, 1),
            Remarks: remarks,
            Items: new CreatePurchaseOrderItemRequest[]
            {
                new(itemProductId, 2m, 5m)
            }.AsReadOnly());
    }

    private static CreatePurchaseOrderHandler CreateHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork,
        IProductRepository? productRepository = null)
    {
        return new CreatePurchaseOrderHandler(
            purchaseOrderRepository,
            supplierRepository,
            productRepository ?? new FakeProductRepository(0, null, false),
            unitOfWork);
    }

    private static Error SupplierNotFound => PurchaseOrderErrors.SupplierNotFound;
    private static Error SupplierInactive => PurchaseOrderErrors.SupplierInactive;
    private static Error ProductNotFound(int productId) => PurchaseOrderErrors.ProductNotFound(productId);
    private static Error ProductInactive(int productId) => PurchaseOrderErrors.ProductInactive(productId);

    // =====================================================================
    // Local/private fakes (T02 only — not promoted to shared support)
    // =====================================================================

    public sealed class FakeSupplierRepository : ISupplierRepository
    {
        private readonly int _requestedId;
        private readonly Supplier? _supplier;
        private readonly bool _found;
        private readonly CallOrder? _callOrder;

        public int GetByIdAsyncCallCount { get; private set; }

        public int? LastGetByIdAsyncRequestId { get; private set; }

        private readonly List<int> _getRequests = new List<int>();

        public IReadOnlyList<int> GetRequests => _getRequests;

        public FakeSupplierRepository(
            int requestedId,
            bool found,
            CallOrder? callOrder = null)
            : this(requestedId, null, found, callOrder)
        {
        }

        public FakeSupplierRepository(
            int requestedId,
            Supplier? supplier,
            bool found,
            CallOrder? callOrder = null)
        {
            _requestedId = requestedId;
            _supplier = supplier;
            _found = found;
            _callOrder = callOrder;
        }

        public Task<Supplier?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            GetByIdAsyncCallCount++;
            _getRequests.Add(id);
            LastGetByIdAsyncRequestId = id;
            _callOrder?.Record("SupplierRepository.GetByIdAsync");

            return Task.FromResult(_found ? _supplier : null);
        }

        // ISupplierRepository / IRepository<Supplier> members not exercised by
        // CreatePurchaseOrderHandler tests fail fast rather than returning misleading defaults.
        public Task<PagedResult<Supplier>> GetPagedAsync(
            PagedQuery request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeSupplierRepository does not support GetPagedAsync: no T02 test exercises this member.");

        public Task<IReadOnlyList<Supplier>> GetAllAsync(
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeSupplierRepository does not support GetAllAsync: no T02 test exercises this member.");

        public Task<IReadOnlyList<Supplier>> FindAsync(
            System.Linq.Expressions.Expression<System.Func<Supplier, bool>> predicate,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeSupplierRepository does not support FindAsync: no T02 test exercises this member.");

        public Task AddAsync(
            Supplier entity,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeSupplierRepository does not support AddAsync: no T02 test exercises this member.");

        public void Update(
            Supplier entity)
            => throw new NotSupportedException(
                "FakeSupplierRepository does not support Update: no T02 test exercises this member.");

        public void Remove(
            Supplier entity)
            => throw new NotSupportedException(
                "FakeSupplierRepository does not support Remove: no T02 test exercises this member.");

        public Task<bool> ExistsAsync(
            System.Linq.Expressions.Expression<System.Func<Supplier, bool>> predicate,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeSupplierRepository does not support ExistsAsync: no T02 test exercises this member.");

        public Task<bool> ExistsByNameAsync(
            int excludingSupplierId,
            string name,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeSupplierRepository does not support ExistsByNameAsync: no T02 test exercises this member.");

        public Task<bool> ExistsByNameAsync(
            string name,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeSupplierRepository does not support ExistsByNameAsync: no T02 test exercises this member.");
    }

    public sealed class FakeProductRepository : IProductRepository
    {
        private readonly int _productId1;
        private readonly Product? _product1;
        private readonly bool _found1;

        private readonly int _productId2;
        private readonly Product? _product2;
        private readonly bool _found2;

        private readonly int _productId3;
        private readonly Product? _product3;
        private readonly bool _found3;

        private readonly CallOrder? _callOrder;

        public int GetByIdAsyncCallCount { get; private set; }

        public int? LastGetByIdAsyncRequestId { get; private set; }

        private readonly List<int> _getRequests = new List<int>();

        public IReadOnlyList<int> GetRequests => _getRequests;

        public FakeProductRepository(
            int productId1,
            Product? product1,
            bool found1,
            CallOrder? callOrder = null)
            : this(productId1, product1, found1, 0, null, false, 0, null, false, callOrder)
        {
        }

        public FakeProductRepository(
            int productId1,
            Product? product1,
            bool found1,
            int productId2,
            Product? product2,
            bool found2,
            CallOrder? callOrder = null)
            : this(
                productId1, product1, found1,
                productId2, product2, found2,
                0, null, false,
                callOrder)
        {
        }

        public FakeProductRepository(
            int productId1,
            Product? product1,
            bool found1,
            int productId2,
            Product? product2,
            bool found2,
            int productId3,
            Product? product3,
            bool found3,
            CallOrder? callOrder = null)
        {
            _productId1 = productId1;
            _product1 = product1;
            _found1 = found1;
            _productId2 = productId2;
            _product2 = product2;
            _found2 = found2;
            _productId3 = productId3;
            _product3 = product3;
            _found3 = found3;
            _callOrder = callOrder;
        }

        public Task<Product?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            GetByIdAsyncCallCount++;
            _getRequests.Add(id);
            LastGetByIdAsyncRequestId = id;
            _callOrder?.Record("ProductRepository.GetByIdAsync");

            return id switch
            {
                var i when i == _productId1 => Task.FromResult(_found1 ? _product1 : null),
                var i when i == _productId2 => Task.FromResult(_found2 ? _product2 : null),
                var i when i == _productId3 => Task.FromResult(_found3 ? _product3 : null),
                _ => Task.FromResult<Product?>(null)
            };
        }

        // IProductRepository / IRepository<Product> members not exercised by
        // CreatePurchaseOrderHandler tests fail fast rather than returning misleading defaults.
        public Task<Product?> GetWithRelationshipsAsync(
            int id,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeProductRepository does not support GetWithRelationshipsAsync: no T02 test exercises this member.");

        public Task<Product?> GetBySkuAsync(
            string sku,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeProductRepository does not support GetBySkuAsync: no T02 test exercises this member.");

        public Task<bool> ExistsBySkuAsync(
            string sku,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeProductRepository does not support ExistsBySkuAsync: no T02 test exercises this member.");

        public Task<PagedResult<Product>> GetPagedAsync(
            PagedQuery request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeProductRepository does not support GetPagedAsync: no T02 test exercises this member.");

        public Task<IReadOnlyList<Product>> GetAllAsync(
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeProductRepository does not support GetAllAsync: no T02 test exercises this member.");

        public Task<IReadOnlyList<Product>> FindAsync(
            System.Linq.Expressions.Expression<System.Func<Product, bool>> predicate,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeProductRepository does not support FindAsync: no T02 test exercises this member.");

        public Task AddAsync(
            Product entity,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeProductRepository does not support AddAsync: no T02 test exercises this member.");

        public void Update(
            Product entity)
            => throw new NotSupportedException(
                "FakeProductRepository does not support Update: no T02 test exercises this member.");

        public void Remove(
            Product entity)
            => throw new NotSupportedException(
                "FakeProductRepository does not support Remove: no T02 test exercises this member.");

        public Task<bool> ExistsAsync(
            System.Linq.Expressions.Expression<System.Func<Product, bool>> predicate,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeProductRepository does not support ExistsAsync: no T02 test exercises this member.");
    }
}
