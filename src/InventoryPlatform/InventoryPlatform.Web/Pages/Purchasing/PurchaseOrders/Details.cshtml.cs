using InventoryPlatform.Application.Features.Purchasing.ApprovePurchaseOrder;
using InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrder;
using InventoryPlatform.Application.Features.Purchasing.ReceivePurchaseOrder;
using InventoryPlatform.Application.Features.Purchasing.SubmitPurchaseOrder;
using InventoryPlatform.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Purchasing.PurchaseOrders;

public class DetailsModel : PageModel
{
    private readonly GetPurchaseOrderHandler _handler;
    private readonly SubmitPurchaseOrderHandler _submitHandler;
    private readonly ApprovePurchaseOrderHandler _approveHandler;
    private readonly ReceivePurchaseOrderHandler _receiveHandler;

    public DetailsModel(
        GetPurchaseOrderHandler handler,
        SubmitPurchaseOrderHandler submitHandler,
        ApprovePurchaseOrderHandler approveHandler,
        ReceivePurchaseOrderHandler receiveHandler)
    {
        _handler = handler;
        _submitHandler = submitHandler;
        _approveHandler = approveHandler;
        _receiveHandler = receiveHandler;
    }

    public GetPurchaseOrderResponse? PurchaseOrder { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string Search { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public DateOnly? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? ToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public PurchaseOrderStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool Descending { get; set; }

    public async Task<IActionResult> OnGetAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(
            new GetPurchaseOrderRequest(id),
            cancellationToken);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        PurchaseOrder = result.Value;

        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync(
    int id,
    CancellationToken cancellationToken)
    {
        var result = await _submitHandler.HandleAsync(
            new SubmitPurchaseOrderRequest(id),
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            var purchaseOrderResult = await _handler.HandleAsync(
                new GetPurchaseOrderRequest(id),
                cancellationToken);

            if (purchaseOrderResult.IsFailure ||
                purchaseOrderResult.Value is null)
            {
                return NotFound();
            }

            PurchaseOrder = purchaseOrderResult.Value;

            return Page();
        }

        TempData["SuccessMessage"] =
            $"Purchase Order '{result.Value!.Id}' was submitted successfully.";

        return RedirectToPage(
            "./Details",
            new { id = result.Value.Id, search = Search, fromDate = FromDate, toDate = ToDate, status = Status, sortBy = SortBy, descending = Descending });
    }

    public async Task<IActionResult> OnPostApproveAsync(
    int id,
    CancellationToken cancellationToken)
    {
        var result = await _approveHandler.HandleAsync(
            new ApprovePurchaseOrderRequest(id),
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            var purchaseOrderResult = await _handler.HandleAsync(
                new GetPurchaseOrderRequest(id),
                cancellationToken);

            if (purchaseOrderResult.IsFailure ||
                purchaseOrderResult.Value is null)
            {
                return NotFound();
            }

            PurchaseOrder = purchaseOrderResult.Value;

            return Page();
        }

        TempData["SuccessMessage"] =
            $"Purchase Order '{result.Value!.Id}' was approved successfully.";

        return RedirectToPage(
            "./Details",
            new { id = result.Value.Id, search = Search, fromDate = FromDate, toDate = ToDate, status = Status, sortBy = SortBy, descending = Descending });
    }

    public async Task<IActionResult> OnPostReceiveAsync(
    int purchaseOrderId,
    int productId,
    decimal quantity,
    CancellationToken cancellationToken)
    {
        var result = await _receiveHandler.HandleAsync(
            new ReceivePurchaseOrderRequest(
                purchaseOrderId,
                productId,
                quantity),
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            var purchaseOrderResult = await _handler.HandleAsync(
                new GetPurchaseOrderRequest(purchaseOrderId),
                cancellationToken);

            if (purchaseOrderResult.IsFailure ||
                purchaseOrderResult.Value is null)
            {
                return NotFound();
            }

            PurchaseOrder = purchaseOrderResult.Value;

            return Page();
        }

        TempData["SuccessMessage"] =
            $"Purchase Order '{result.Value!.PurchaseOrderId}' was received successfully.";

        return RedirectToPage(
            "./Details",
            new { id = result.Value.PurchaseOrderId, search = Search, fromDate = FromDate, toDate = ToDate, status = Status, sortBy = SortBy, descending = Descending });
    }
}