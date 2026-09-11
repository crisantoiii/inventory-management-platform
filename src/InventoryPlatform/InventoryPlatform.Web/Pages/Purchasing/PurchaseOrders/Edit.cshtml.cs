using InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrder;
using InventoryPlatform.Application.Features.Purchasing.RemovePurchaseOrderItem;
using InventoryPlatform.Application.Features.Purchasing.UpdatePurchaseOrderItem;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Domain.Exceptions;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Purchasing.PurchaseOrders;

[Authorize(Policy = AuthorizationPolicies.PurchaseOrder.EditPolicy)]
public class EditModel : PageModel
{
    private readonly GetPurchaseOrderHandler _getHandler;
    private readonly UpdatePurchaseOrderItemHandler _updateHandler;
    private readonly RemovePurchaseOrderItemHandler _removeHandler;
    private readonly IAuthorizationService _authorizationService;

    public EditModel(
        GetPurchaseOrderHandler getHandler,
        UpdatePurchaseOrderItemHandler updateHandler,
        RemovePurchaseOrderItemHandler removeHandler,
        IAuthorizationService authorizationService)
    {
        _getHandler = getHandler;
        _updateHandler = updateHandler;
        _removeHandler = removeHandler;
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
        var result = await _getHandler.HandleAsync(
            new GetPurchaseOrderRequest(id),
            cancellationToken);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        if (result.Value.Status != PurchaseOrderStatus.Draft)
        {
            return RedirectToPage("./Details", new { id });
        }

        PurchaseOrder = result.Value;

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateItemAsync(
        int id,
        int productId,
        decimal quantity,
        decimal unitCost,
        CancellationToken cancellationToken)
    {
        var authResult = await _authorizationService.AuthorizeAsync(
            User,
            resource: null,
            AuthorizationPolicies.ForCapability(
                AuthorizationPolicies.PurchaseOrder.Edit));

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        try
        {
            var result = await _updateHandler.HandleAsync(
                new UpdatePurchaseOrderItemRequest(
                    id,
                    productId,
                    quantity,
                    unitCost),
                cancellationToken);

            if (result.IsFailure)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                $"Item '{productId}' was updated successfully.";

            return RedirectToPage("./Edit", new
            {
                id,
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
        catch (DomainException exception)
        {
            return await RenderDomainFailureAsync(
                id,
                exception.Message,
                cancellationToken);
        }
    }

    public async Task<IActionResult> OnPostRemoveItemAsync(
        int id,
        int productId,
        CancellationToken cancellationToken)
    {
        var authResult = await _authorizationService.AuthorizeAsync(
            User,
            resource: null,
            AuthorizationPolicies.ForCapability(
                AuthorizationPolicies.PurchaseOrder.Edit));

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        try
        {
            var result = await _removeHandler.HandleAsync(
                new RemovePurchaseOrderItemRequest(id, productId),
                cancellationToken);

            if (result.IsFailure)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                $"Item '{productId}' was removed successfully.";

            return RedirectToPage("./Edit", new
            {
                id,
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
        catch (DomainException exception)
        {
            return await RenderDomainFailureAsync(
                id,
                exception.Message,
                cancellationToken);
        }
    }

    private async Task<IActionResult> RenderDomainFailureAsync(
        int id,
        string message,
        CancellationToken cancellationToken)
    {
        ModelState.AddModelError(string.Empty, message);

        var result = await _getHandler.HandleAsync(
            new GetPurchaseOrderRequest(id),
            cancellationToken);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        if (result.Value.Status != PurchaseOrderStatus.Draft)
        {
            return RedirectToPage("./Details", new { id });
        }

        PurchaseOrder = result.Value;

        return Page();
    }
}
