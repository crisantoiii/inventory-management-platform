using InventoryPlatform.Application.Features.Purchasing.ApprovePurchaseOrder;
using InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrder;
using InventoryPlatform.Application.Features.Purchasing.ReceivePurchaseOrder;
using InventoryPlatform.Application.Features.Purchasing.SubmitPurchaseOrder;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Purchasing.PurchaseOrders;

[Authorize(Policy = AuthorizationPolicies.PurchaseOrder.ViewPolicy)]
public class DetailsModel : PageModel
{
    private readonly GetPurchaseOrderHandler _handler;
    private readonly SubmitPurchaseOrderHandler _submitHandler;
    private readonly ApprovePurchaseOrderHandler _approveHandler;
    private readonly ReceivePurchaseOrderHandler _receiveHandler;
    private readonly IAuthorizationService _authorizationService;

    public DetailsModel(
        GetPurchaseOrderHandler handler,
        SubmitPurchaseOrderHandler submitHandler,
        ApprovePurchaseOrderHandler approveHandler,
        ReceivePurchaseOrderHandler receiveHandler,
        IAuthorizationService authorizationService)
    {
        _handler = handler;
        _submitHandler = submitHandler;
        _approveHandler = approveHandler;
        _receiveHandler = receiveHandler;
        _authorizationService = authorizationService;
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

    [BindProperty(SupportsGet = true)]
    public int PageNum { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

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
        var authResult = await _authorizationService.AuthorizeAsync(
            User,
            resource: null,
            AuthorizationPolicies.ForCapability(
                AuthorizationPolicies.PurchaseOrder.Submit));

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

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
            new
            {
                id = result.Value.Id,
                Search,
                FromDate,
                ToDate,
                Status,
                SortBy,
                Descending,
                PageNum,
                PageSize
            });
    }

    public async Task<IActionResult> OnPostApproveAsync(
    int id,
    CancellationToken cancellationToken)
    {
        var authResult = await _authorizationService.AuthorizeAsync(
            User,
            resource: null,
            AuthorizationPolicies.ForCapability(
                AuthorizationPolicies.PurchaseOrder.Approve));

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

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
            new
            {
                id = result.Value.Id,
                Search,
                FromDate,
                ToDate,
                Status,
                SortBy,
                Descending,
                PageNum,
                PageSize
            });
    }

    public async Task<IActionResult> OnPostReceiveAsync(
    int purchaseOrderId,
    int productId,
    decimal quantity,
    CancellationToken cancellationToken)
    {
        var authResult = await _authorizationService.AuthorizeAsync(
            User,
            resource: null,
            AuthorizationPolicies.ForCapability(
                AuthorizationPolicies.PurchaseOrder.Receive));

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

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
            new
            {
                id = result.Value.PurchaseOrderId,
                Search,
                FromDate,
                ToDate,
                Status,
                SortBy,
                Descending,
                PageNum,
                PageSize
            });
    }
}