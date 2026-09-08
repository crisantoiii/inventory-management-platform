using InventoryPlatform.Application.Features.Purchasing;
using InventoryPlatform.Application.Features.Purchasing.ReceivePurchaseOrder;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Domain.Exceptions;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;
using InventoryPlatform.UnitTests.TestSupport.Purchasing;
using Xunit;

namespace InventoryPlatform.UnitTests.Application.Purchasing;

public sealed class ReceivePurchaseOrderHandlerTests
{
    // =====================================================================
    // A. Purchase order not found
    // =====================================================================

    [Fact]
    public async Task HandleAsync_PurchaseOrderNotFound_ReturnsNotFoundFailureAndShortCircuits()
    {
        // Arrange
        var request = new ReceivePurchaseOrderRequest(PurchaseOrderId: 42, ProductId: 10, Quantity: 3m);

        var productRepository = new FakeProductRepository(product: null);
        var transactionRepository = new FakeInventoryTransactionRepository();
        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder: null);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new ReceivePurchaseOrderHandler(
            purchaseOrderRepository,
            productRepository,
            transactionRepository,
            unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(PurchaseOrderErrors.NotFound, result.Error);
        Assert.Equal("PurchaseOrder.NotFound", result.Error.Code);
        Assert.Equal("Purchase order not found.", result.Error.Message);

        // Short-circuit: the Product lookup never proceeds.
        Assert.Equal(1, purchaseOrderRepository.GetByIdAsyncCallCount);
        Assert.Equal(42, purchaseOrderRepository.LastGetByIdAsyncRequestId);
        Assert.Equal(0, productRepository.GetByIdAsyncCallCount);
        Assert.Equal(0, transactionRepository.AddAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // B. Product not found
    // =====================================================================

    [Fact]
    public async Task HandleAsync_ProductNotFound_ReturnsProductNotFoundFailureAndShortCircuits()
    {
        // Arrange
        var purchaseOrder = PurchasingTestData.CreateApprovedPurchaseOrder(
            id: 7,
            supplierId: 1,
            productId: 10);
        var request = new ReceivePurchaseOrderRequest(PurchaseOrderId: 7, ProductId: 999, Quantity: 3m);

        var productRepository = new FakeProductRepository(product: null);
        var transactionRepository = new FakeInventoryTransactionRepository();
        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new ReceivePurchaseOrderHandler(
            purchaseOrderRepository,
            productRepository,
            transactionRepository,
            unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(PurchaseOrderErrors.ProductNotFound(999), result.Error);
        Assert.Equal("PurchaseOrder.ProductNotFound", result.Error.Code);
        Assert.Equal("Product with ID '999' was not found.", result.Error.Message);

        // Short-circuit: no aggregate mutation, no transaction, no save.
        Assert.Equal(1, productRepository.GetByIdAsyncCallCount);
        Assert.Equal(999, productRepository.LastGetByIdAsyncRequestId);
        Assert.Equal(PurchaseOrderStatus.Approved, purchaseOrder.Status);
        Assert.Empty(transactionRepository.AddedTransactions);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // C. Invalid PurchaseOrder state (DomainException propagates)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_DraftPurchaseOrder_DomainExceptionPropagatesAndNothingMutates()
    {
        // Arrange — Draft WITH items: the violated precondition is the state
        // requirement (only approved orders can receive), not the item requirement.
        var product = PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10");
        var purchaseOrder = PurchasingTestData.CreateDraftPurchaseOrderWithItem(
            id: 7,
            supplierId: 1,
            productId: 10,
            quantity: 10m);
        var request = new ReceivePurchaseOrderRequest(PurchaseOrderId: 7, ProductId: 10, Quantity: 3m);

        var productRepository = new FakeProductRepository(product);
        var transactionRepository = new FakeInventoryTransactionRepository();
        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new ReceivePurchaseOrderHandler(
            purchaseOrderRepository,
            productRepository,
            transactionRepository,
            unitOfWork);

        // Act / Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => handler.HandleAsync(request));

        Assert.Contains("Only approved", exception.Message);

        // Nothing mutated or persisted.
        Assert.Equal(PurchaseOrderStatus.Draft, purchaseOrder.Status);
        Assert.Equal(0m, product.QuantityOnHand);
        Assert.Empty(transactionRepository.AddedTransactions);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_SubmittedPurchaseOrder_DomainExceptionPropagatesAndNothingMutates()
    {
        // Arrange — real Domain transition: Draft + item -> Submit().
        var product = PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10");
        var purchaseOrder = PurchasingTestData.CreateSubmittedPurchaseOrder(
            id: 7,
            supplierId: 1,
            productId: 10,
            quantity: 10m);
        var request = new ReceivePurchaseOrderRequest(PurchaseOrderId: 7, ProductId: 10, Quantity: 3m);

        var productRepository = new FakeProductRepository(product);
        var transactionRepository = new FakeInventoryTransactionRepository();
        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new ReceivePurchaseOrderHandler(
            purchaseOrderRepository,
            productRepository,
            transactionRepository,
            unitOfWork);

        // Act / Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => handler.HandleAsync(request));

        Assert.Contains("Only approved", exception.Message);

        Assert.Equal(PurchaseOrderStatus.Submitted, purchaseOrder.Status);
        Assert.Equal(0m, product.QuantityOnHand);
        Assert.Empty(transactionRepository.AddedTransactions);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // D. Missing PurchaseOrder item (Product/item not present in the order)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_ProductNotInPurchaseOrderItems_DomainExceptionPropagatesAndNothingMutates()
    {
        // Arrange — approved order containing only product 10; the request
        // targets product 20, which exists separately but is not an order item.
        var product = PurchasingTestData.CreateProduct(id: 20, sku: "SKU-20");
        var purchaseOrder = PurchasingTestData.CreateApprovedPurchaseOrder(
            id: 7,
            supplierId: 1,
            productId: 10,
            quantity: 10m);
        var request = new ReceivePurchaseOrderRequest(PurchaseOrderId: 7, ProductId: 20, Quantity: 3m);

        var productRepository = new FakeProductRepository(product);
        var transactionRepository = new FakeInventoryTransactionRepository();
        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new ReceivePurchaseOrderHandler(
            purchaseOrderRepository,
            productRepository,
            transactionRepository,
            unitOfWork);

        // Act / Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => handler.HandleAsync(request));

        Assert.Contains("not found", exception.Message);

        // The separately loaded product's stock is unchanged and nothing persisted.
        Assert.Equal(0m, product.QuantityOnHand);
        Assert.Empty(transactionRepository.AddedTransactions);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // E. Non-positive quantity (DomainException propagates)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_ZeroQuantity_DomainExceptionPropagatesAndNothingMutates()
    {
        // Arrange
        var product = PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10");
        var purchaseOrder = PurchasingTestData.CreateApprovedPurchaseOrder(
            id: 7,
            supplierId: 1,
            productId: 10,
            quantity: 10m);
        var request = new ReceivePurchaseOrderRequest(PurchaseOrderId: 7, ProductId: 10, Quantity: 0m);

        var productRepository = new FakeProductRepository(product);
        var transactionRepository = new FakeInventoryTransactionRepository();
        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new ReceivePurchaseOrderHandler(
            purchaseOrderRepository,
            productRepository,
            transactionRepository,
            unitOfWork);

        // Act / Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => handler.HandleAsync(request));

        Assert.Contains("greater than zero", exception.Message);

        Assert.Equal(PurchaseOrderStatus.Approved, purchaseOrder.Status);
        Assert.Equal(0m, product.QuantityOnHand);
        Assert.Empty(transactionRepository.AddedTransactions);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_NegativeQuantity_DomainExceptionPropagatesAndNothingMutates()
    {
        // Arrange
        var product = PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10");
        var purchaseOrder = PurchasingTestData.CreateApprovedPurchaseOrder(
            id: 7,
            supplierId: 1,
            productId: 10,
            quantity: 10m);
        var request = new ReceivePurchaseOrderRequest(PurchaseOrderId: 7, ProductId: 10, Quantity: -2m);

        var productRepository = new FakeProductRepository(product);
        var transactionRepository = new FakeInventoryTransactionRepository();
        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new ReceivePurchaseOrderHandler(
            purchaseOrderRepository,
            productRepository,
            transactionRepository,
            unitOfWork);

        // Act / Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => handler.HandleAsync(request));

        Assert.Contains("greater than zero", exception.Message);

        Assert.Equal(0m, product.QuantityOnHand);
        Assert.Empty(transactionRepository.AddedTransactions);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // F. Over-receiving (cumulative received quantity exceeds ordered quantity)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_OverReceiving_DomainExceptionPropagatesAndNothingMutates()
    {
        // Arrange — approved order for 10; request receives 11 (> remaining 10).
        var product = PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10");
        var purchaseOrder = PurchasingTestData.CreateApprovedPurchaseOrder(
            id: 7,
            supplierId: 1,
            productId: 10,
            quantity: 10m);
        var request = new ReceivePurchaseOrderRequest(PurchaseOrderId: 7, ProductId: 10, Quantity: 11m);

        var productRepository = new FakeProductRepository(product);
        var transactionRepository = new FakeInventoryTransactionRepository();
        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new ReceivePurchaseOrderHandler(
            purchaseOrderRepository,
            productRepository,
            transactionRepository,
            unitOfWork);

        // Act / Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => handler.HandleAsync(request));

        Assert.Contains("cannot exceed", exception.Message);

        // The rejected receive does not increase stock and nothing persisted.
        Assert.Equal(0m, product.QuantityOnHand);
        Assert.Empty(transactionRepository.AddedTransactions);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_CumulativeOverReceiving_DomainExceptionPropagatesAndNothingMutates()
    {
        // Arrange — approved order for 10; a prior partial receive of 6 leaves 4
        // remaining; the request receives 5 more (6 + 5 = 11 > 10).
        var product = PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10");
        var purchaseOrder = PurchasingTestData.CreateApprovedPurchaseOrder(
            id: 7,
            supplierId: 1,
            productId: 10,
            quantity: 10m);
        purchaseOrder.Receive(productId: 10, quantity: 6m);
        var request = new ReceivePurchaseOrderRequest(PurchaseOrderId: 7, ProductId: 10, Quantity: 5m);

        var productRepository = new FakeProductRepository(product);
        var transactionRepository = new FakeInventoryTransactionRepository();
        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new ReceivePurchaseOrderHandler(
            purchaseOrderRepository,
            productRepository,
            transactionRepository,
            unitOfWork);

        // Act / Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => handler.HandleAsync(request));

        Assert.Contains("cannot exceed", exception.Message);

        // Only the prior in-arrangement receive is reflected; the rejected one
        // adds nothing and nothing persisted.
        Assert.Equal(6m, purchaseOrder.Items.First().ReceivedQuantity);
        Assert.Equal(0m, product.QuantityOnHand);
        Assert.Empty(transactionRepository.AddedTransactions);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // G. Successful partial receive
    // =====================================================================

    [Fact]
    public async Task HandleAsync_PartialReceive_ReturnsReceivingAndMutatesStockAndAddsTransaction()
    {
        // Arrange — approved order for 10; receive 4 (< remaining 10).
        var product = PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10");
        var purchaseOrder = PurchasingTestData.CreateApprovedPurchaseOrder(
            id: 7,
            supplierId: 1,
            productId: 10,
            quantity: 10m);
        var request = new ReceivePurchaseOrderRequest(PurchaseOrderId: 7, ProductId: 10, Quantity: 4m);

        var productRepository = new FakeProductRepository(product);
        var transactionRepository = new FakeInventoryTransactionRepository();
        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new ReceivePurchaseOrderHandler(
            purchaseOrderRepository,
            productRepository,
            transactionRepository,
            unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);

        var response = result.Value!;
        Assert.Equal(7, response.PurchaseOrderId);
        Assert.Equal(PurchaseOrderStatus.Receiving, response.Status);
        Assert.Equal(PurchaseOrderStatus.Receiving, purchaseOrder.Status);

        // Aggregate/item mutation.
        var item = purchaseOrder.Items.Single(i => i.ProductId == 10);
        Assert.Equal(4m, item.ReceivedQuantity);
        Assert.Equal(6m, item.RemainingQuantity);
        Assert.False(item.IsFullyReceived);

        // The separately loaded Product's stock increased by exactly the received quantity.
        Assert.Equal(4m, product.QuantityOnHand);

        // Exactly one InventoryTransaction with the source-grounded fields.
        var transaction = Assert.Single(transactionRepository.AddedTransactions);
        Assert.Equal(10, transaction.ProductId);
        Assert.Equal(TransactionType.StockIn, transaction.TransactionType);
        Assert.True(transaction.IsStockIn);
        Assert.Equal(4m, transaction.Quantity);
        Assert.Equal("PO-7", transaction.ReferenceNumber);
        Assert.Equal("Purchase Order 7 receiving", transaction.Remarks);

        // Save occurred exactly once.
        Assert.Equal(1, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // H. Successful full receive
    // =====================================================================

    [Fact]
    public async Task HandleAsync_FullReceive_ReturnsCompletedAndMutatesStockAndAddsTransaction()
    {
        // Arrange — approved order for 10; receive the full remaining 10.
        var product = PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10");
        var purchaseOrder = PurchasingTestData.CreateApprovedPurchaseOrder(
            id: 7,
            supplierId: 1,
            productId: 10,
            quantity: 10m);
        var request = new ReceivePurchaseOrderRequest(PurchaseOrderId: 7, ProductId: 10, Quantity: 10m);

        var productRepository = new FakeProductRepository(product);
        var transactionRepository = new FakeInventoryTransactionRepository();
        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new ReceivePurchaseOrderHandler(
            purchaseOrderRepository,
            productRepository,
            transactionRepository,
            unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var response = result.Value!;
        Assert.Equal(7, response.PurchaseOrderId);
        Assert.Equal(PurchaseOrderStatus.Completed, response.Status);
        Assert.Equal(PurchaseOrderStatus.Completed, purchaseOrder.Status);

        var item = purchaseOrder.Items.Single(i => i.ProductId == 10);
        Assert.Equal(10m, item.ReceivedQuantity);
        Assert.Equal(0m, item.RemainingQuantity);
        Assert.True(item.IsFullyReceived);

        Assert.Equal(10m, product.QuantityOnHand);

        var transaction = Assert.Single(transactionRepository.AddedTransactions);
        Assert.Equal(10, transaction.ProductId);
        Assert.Equal(TransactionType.StockIn, transaction.TransactionType);
        Assert.Equal(10m, transaction.Quantity);
        Assert.Equal("PO-7", transaction.ReferenceNumber);
        Assert.Equal("Purchase Order 7 receiving", transaction.Remarks);

        Assert.Equal(1, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_FinalFullReceiveAfterPartial_ReturnsCompleted()
    {
        // Arrange — approved order for 10; a prior partial receive of 6 leaves 4;
        // receiving the remaining 4 completes the order (real Domain transitions).
        var product = PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10");
        var purchaseOrder = PurchasingTestData.CreateApprovedPurchaseOrder(
            id: 7,
            supplierId: 1,
            productId: 10,
            quantity: 10m);
        purchaseOrder.Receive(productId: 10, quantity: 6m);
        var request = new ReceivePurchaseOrderRequest(PurchaseOrderId: 7, ProductId: 10, Quantity: 4m);

        var productRepository = new FakeProductRepository(product);
        var transactionRepository = new FakeInventoryTransactionRepository();
        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new ReceivePurchaseOrderHandler(
            purchaseOrderRepository,
            productRepository,
            transactionRepository,
            unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var response = result.Value!;
        Assert.Equal(PurchaseOrderStatus.Completed, response.Status);

        var item = purchaseOrder.Items.Single(i => i.ProductId == 10);
        Assert.Equal(10m, item.ReceivedQuantity);
        Assert.True(item.IsFullyReceived);

        // Only the handler's receive (4) increases the separately loaded product's
        // stock; the in-arrangement receive never touched it.
        Assert.Equal(4m, product.QuantityOnHand);

        var transaction = Assert.Single(transactionRepository.AddedTransactions);
        Assert.Equal(4m, transaction.Quantity);

        Assert.Equal(1, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // Observable dependency ordering (test-scoped CallOrder)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_SuccessfulReceive_RecordsLoadLoadAddSaveOrdering()
    {
        // Arrange
        var callOrder = new CallOrder();
        var product = PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10");
        var purchaseOrder = PurchasingTestData.CreateApprovedPurchaseOrder(
            id: 7,
            supplierId: 1,
            productId: 10,
            quantity: 10m);
        var request = new ReceivePurchaseOrderRequest(PurchaseOrderId: 7, ProductId: 10, Quantity: 4m);

        var productRepository = new FakeProductRepository(product, callOrder: callOrder);
        var transactionRepository = new FakeInventoryTransactionRepository(callOrder: callOrder);
        var purchaseOrderRepository = new FakePurchaseOrderRepository(
            purchaseOrder: purchaseOrder,
            callOrder: callOrder);
        var unitOfWork = new FakeUnitOfWork(callOrder: callOrder);

        var handler = new ReceivePurchaseOrderHandler(
            purchaseOrderRepository,
            productRepository,
            transactionRepository,
            unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert — observable cross-dependency ordering only:
        // PO load -> Product load -> transaction add -> save.
        Assert.True(result.IsSuccess);
        Assert.Equal(
            new[]
            {
                "PurchaseOrderRepository.GetByIdAsync",
                "ProductRepository.GetByIdAsync",
                "InventoryTransactionRepository.AddAsync",
                "UnitOfWork.SaveChangesAsync"
            },
            callOrder.Events.ToArray());
    }

    [Fact]
    public async Task HandleAsync_ProductNotFound_RecordsNoAddOrSaveInteractions()
    {
        // Arrange
        var callOrder = new CallOrder();
        var request = new ReceivePurchaseOrderRequest(PurchaseOrderId: 7, ProductId: 999, Quantity: 3m);

        var productRepository = new FakeProductRepository(product: null, callOrder: callOrder);
        var transactionRepository = new FakeInventoryTransactionRepository(callOrder: callOrder);
        var purchaseOrderRepository = new FakePurchaseOrderRepository(
            purchaseOrder: PurchasingTestData.CreateApprovedPurchaseOrder(id: 7, supplierId: 1, productId: 10),
            callOrder: callOrder);
        var unitOfWork = new FakeUnitOfWork(callOrder: callOrder);

        var handler = new ReceivePurchaseOrderHandler(
            purchaseOrderRepository,
            productRepository,
            transactionRepository,
            unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            new[]
            {
                "PurchaseOrderRepository.GetByIdAsync",
                "ProductRepository.GetByIdAsync"
            },
            callOrder.Events.ToArray());
    }

    // =====================================================================
    // Local/private fakes (T04 only — not promoted to shared support)
    // =====================================================================

    private sealed class FakeProductRepository : IProductRepository
    {
        private readonly Product? _product;
        private readonly CallOrder? _callOrder;

        public int GetByIdAsyncCallCount { get; private set; }

        public int? LastGetByIdAsyncRequestId { get; private set; }

        public FakeProductRepository(Product? product, CallOrder? callOrder = null)
        {
            _product = product;
            _callOrder = callOrder;
        }

        public Task<Product?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            GetByIdAsyncCallCount++;
            LastGetByIdAsyncRequestId = id;
            _callOrder?.Record("ProductRepository.GetByIdAsync");

            return Task.FromResult(_product);
        }

        // IProductRepository / IRepository<Product> members not exercised by
        // ReceivePurchaseOrderHandler tests fail fast rather than returning
        // misleading defaults.
        public Task<Product?> GetWithRelationshipsAsync(
            int id,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeProductRepository does not support GetWithRelationshipsAsync: no T04 test exercises this member.");

        public Task<Product?> GetBySkuAsync(
            string sku,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeProductRepository does not support GetBySkuAsync: no T04 test exercises this member.");

        public Task<bool> ExistsBySkuAsync(
            string sku,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeProductRepository does not support ExistsBySkuAsync: no T04 test exercises this member.");

        public Task<PagedResult<Product>> GetPagedAsync(
            PagedQuery request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeProductRepository does not support GetPagedAsync: no T04 test exercises this member.");

        public Task<IReadOnlyList<Product>> GetAllAsync(
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeProductRepository does not support GetAllAsync: no T04 test exercises this member.");

        public Task<IReadOnlyList<Product>> FindAsync(
            System.Linq.Expressions.Expression<System.Func<Product, bool>> predicate,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeProductRepository does not support FindAsync: no T04 test exercises this member.");

        public Task AddAsync(
            Product entity,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeProductRepository does not support AddAsync: no T04 test exercises this member.");

        public void Update(
            Product entity)
            => throw new NotSupportedException(
                "FakeProductRepository does not support Update: no T04 test exercises this member.");

        public void Remove(
            Product entity)
            => throw new NotSupportedException(
                "FakeProductRepository does not support Remove: no T04 test exercises this member.");

        public Task<bool> ExistsAsync(
            System.Linq.Expressions.Expression<System.Func<Product, bool>> predicate,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeProductRepository does not support ExistsAsync: no T04 test exercises this member.");
    }

    private sealed class FakeInventoryTransactionRepository : IInventoryTransactionRepository
    {
        private readonly CallOrder? _callOrder;

        public int AddAsyncCallCount { get; private set; }

        public List<InventoryTransaction> AddedTransactions { get; } = new();

        public FakeInventoryTransactionRepository(CallOrder? callOrder = null)
        {
            _callOrder = callOrder;
        }

        public Task AddAsync(
            InventoryTransaction entity,
            CancellationToken cancellationToken = default)
        {
            AddAsyncCallCount++;
            AddedTransactions.Add(entity);
            _callOrder?.Record("InventoryTransactionRepository.AddAsync");

            return Task.CompletedTask;
        }

        // IInventoryTransactionRepository / IRepository<InventoryTransaction> members
        // not exercised by ReceivePurchaseOrderHandler tests fail fast rather than
        // returning misleading defaults.
        public Task<InventoryTransaction?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeInventoryTransactionRepository does not support GetByIdAsync: no T04 test exercises this member.");

        public Task<IReadOnlyList<InventoryTransaction>> GetAllAsync(
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeInventoryTransactionRepository does not support GetAllAsync: no T04 test exercises this member.");

        public Task<IReadOnlyList<InventoryTransaction>> FindAsync(
            System.Linq.Expressions.Expression<System.Func<InventoryTransaction, bool>> predicate,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeInventoryTransactionRepository does not support FindAsync: no T04 test exercises this member.");

        public void Update(
            InventoryTransaction entity)
            => throw new NotSupportedException(
                "FakeInventoryTransactionRepository does not support Update: no T04 test exercises this member.");

        public void Remove(
            InventoryTransaction entity)
            => throw new NotSupportedException(
                "FakeInventoryTransactionRepository does not support Remove: no T04 test exercises this member.");

        public Task<bool> ExistsAsync(
            System.Linq.Expressions.Expression<System.Func<InventoryTransaction, bool>> predicate,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeInventoryTransactionRepository does not support ExistsAsync: no T04 test exercises this member.");

        public Task<PagedResult<InventoryTransaction>> GetPagedAsync(
            PagedQuery request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeInventoryTransactionRepository does not support GetPagedAsync: no T04 test exercises this member.");

        public Task<IReadOnlyList<InventoryTransaction>> GetByProductAsync(
            int productId,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException(
                "FakeInventoryTransactionRepository does not support GetByProductAsync: no T04 test exercises this member.");
    }
}
