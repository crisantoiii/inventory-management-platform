using InventoryPlatform.Application.Features.Products.GetProducts;
using InventoryPlatform.Application.Features.Purchasing.CreatePurchaseOrder;
using InventoryPlatform.Application.Features.Suppliers.GetSuppliers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryPlatform.Web.Pages.Purchasing.PurchaseOrders;

public class CreateModel : PageModel
{
    private readonly CreatePurchaseOrderHandler _handler;
    private readonly GetSuppliersHandler _getSuppliersHandler;
    private readonly GetProductsHandler _getProductsHandler;

    public CreateModel(
        CreatePurchaseOrderHandler handler,
        GetSuppliersHandler getSuppliersHandler,
        GetProductsHandler getProductsHandler)
    {
        _handler = handler;
        _getSuppliersHandler = getSuppliersHandler;
        _getProductsHandler = getProductsHandler;
    }

    [BindProperty]
    public PurchaseOrderInputModel PurchaseOrder { get; set; } = new();

    public sealed class PurchaseOrderInputModel
    {
        public int SupplierId { get; set; }

        public DateOnly ExpectedDeliveryDate { get; set; }

        public string? Remarks { get; set; }

        public List<PurchaseOrderItemInputModel> Items { get; set; } =
        [
            new PurchaseOrderItemInputModel()
        ];
    }

    public sealed class PurchaseOrderItemInputModel
    {
        public int ProductId { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitCost { get; set; }
    }

    public SelectList SupplierOptions { get; private set; } =
        new SelectList(Array.Empty<object>());

    public SelectList ProductOptions { get; private set; } =
        new SelectList(Array.Empty<object>());

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        if (PurchaseOrder.Items.Count == 0)
        {
            PurchaseOrder.Items.Add(
                new PurchaseOrderItemInputModel());
        }

        PurchaseOrder.ExpectedDeliveryDate =
            DateOnly.FromDateTime(DateTime.Today);

        if (!await PopulateDropdownListsAsync(cancellationToken))
        {
            return Page();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownListsAsync(cancellationToken);
            return Page();
        }

        var request = new CreatePurchaseOrderRequest(
            PurchaseOrder.SupplierId,
            PurchaseOrder.ExpectedDeliveryDate,
            PurchaseOrder.Remarks,
            PurchaseOrder.Items
                .Select(item => new CreatePurchaseOrderItemRequest(
                    item.ProductId,
                    item.Quantity,
                    item.UnitCost))
                .ToList());

        var result = await _handler.HandleAsync(
            request,
            cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            await PopulateDropdownListsAsync(cancellationToken);
            return Page();
        }

        TempData["SuccessMessage"] =
            $"Purchase Order '{result.Value!.Id}' was created successfully.";

        return RedirectToPage("Index");
    }

    private async Task<bool> PopulateDropdownListsAsync(
        CancellationToken cancellationToken)
    {
        var suppliersResult =
            await _getSuppliersHandler.HandleAsync(
                new GetSuppliersRequest(),
                cancellationToken);

        var productsResult =
            await _getProductsHandler.HandleAsync(
                new GetProductsRequest(),
                cancellationToken);

        if (suppliersResult.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                $"Unable to load suppliers: {suppliersResult.Error.Message}");
        }
        else
        {
            SupplierOptions = new SelectList(
                suppliersResult.Value!.Items,
                "Id",
                "Name");
        }

        if (productsResult.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                $"Unable to load products: {productsResult.Error.Message}");
        }
        else
        {
            ProductOptions = new SelectList(
                productsResult.Value!.Items,
                "Id",
                "Name");
        }

        return suppliersResult.IsSuccess &&
               productsResult.IsSuccess;
    }
}