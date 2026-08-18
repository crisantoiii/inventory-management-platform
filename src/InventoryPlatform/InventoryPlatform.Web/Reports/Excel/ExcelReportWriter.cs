using ClosedXML.Excel;
using InventoryPlatform.Application.DTOs.Reporting;

namespace InventoryPlatform.Web.Reports.Excel;

public sealed class ExcelReportWriter
{
    public const string ContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public byte[] CreateInventoryValuation(
        IReadOnlyList<InventoryValuationDto> items)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Inventory Valuation");

        WriteHeader(
            worksheet,
            "Product",
            "Category",
            "Quantity On Hand",
            "Cost Price",
            "Inventory Value");

        var row = 2;
        foreach (var item in items)
        {
            worksheet.Cell(row, 1).Value = item.ProductName;
            worksheet.Cell(row, 2).Value = item.CategoryName ?? "";
            worksheet.Cell(row, 3).Value = item.QuantityOnHand;
            worksheet.Cell(row, 4).Value = item.CostPrice;
            worksheet.Cell(row, 5).Value = item.InventoryValue;
            row++;
        }

        worksheet.Cell(row, 4).Value = "Total Inventory Value";
        worksheet.Cell(row, 5).Value = items.Sum(x => x.InventoryValue);
        worksheet.Range(row, 4, row, 5).Style.Font.Bold = true;

        FormatDecimalColumns(worksheet, 3, 5);

        return Save(workbook);
    }

    public byte[] CreatePurchaseHistory(
        IReadOnlyList<PurchaseHistoryDto> items)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Purchase History");

        WriteHeader(
            worksheet,
            "PO ID",
            "Supplier",
            "Order Date",
            "Status",
            "Total Amount",
            "Total Quantity",
            "Received Quantity",
            "Remaining Quantity");

        var row = 2;
        foreach (var item in items)
        {
            worksheet.Cell(row, 1).Value = item.PurchaseOrderId;
            worksheet.Cell(row, 2).Value = item.SupplierName;
            worksheet.Cell(row, 3).Value = item.OrderDate.ToDateTime(TimeOnly.MinValue);
            worksheet.Cell(row, 4).Value = item.Status.ToString();
            worksheet.Cell(row, 5).Value = item.TotalAmount;
            worksheet.Cell(row, 6).Value = item.TotalQuantity;
            worksheet.Cell(row, 7).Value = item.ReceivedQuantity;
            worksheet.Cell(row, 8).Value = item.RemainingQuantity;
            row++;
        }

        worksheet.Column(3).Style.NumberFormat.Format = "yyyy-mm-dd";
        FormatDecimalColumns(worksheet, 5, 8);

        return Save(workbook);
    }

    public byte[] CreateSupplierPurchaseAnalysis(
        IReadOnlyList<SupplierPurchaseAnalysisDto> items)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Supplier Purchase Analysis");

        WriteHeader(
            worksheet,
            "Supplier",
            "First Order Date",
            "Last Order Date",
            "Purchase Order Count",
            "Total Quantity",
            "Received Quantity",
            "Remaining Quantity",
            "Total Amount");

        var row = 2;
        foreach (var item in items)
        {
            worksheet.Cell(row, 1).Value = item.SupplierName;
            worksheet.Cell(row, 2).Value = item.FirstOrderDate.ToDateTime(TimeOnly.MinValue);
            worksheet.Cell(row, 3).Value = item.LastOrderDate.ToDateTime(TimeOnly.MinValue);
            worksheet.Cell(row, 4).Value = item.PurchaseOrderCount;
            worksheet.Cell(row, 5).Value = item.TotalQuantity;
            worksheet.Cell(row, 6).Value = item.ReceivedQuantity;
            worksheet.Cell(row, 7).Value = item.RemainingQuantity;
            worksheet.Cell(row, 8).Value = item.TotalAmount;
            row++;
        }

        worksheet.Column(2).Style.NumberFormat.Format = "yyyy-mm-dd";
        worksheet.Column(3).Style.NumberFormat.Format = "yyyy-mm-dd";
        FormatDecimalColumns(worksheet, 5, 8);

        return Save(workbook);
    }

    public byte[] CreateStockMovement(
        IReadOnlyList<StockMovementDto> items)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Stock Movement");

        WriteHeader(
            worksheet,
            "Date (UTC)",
            "Product",
            "SKU",
            "Movement Type",
            "Quantity",
            "Reference Number",
            "Remarks");

        var row = 2;
        foreach (var item in items)
        {
            worksheet.Cell(row, 1).Value = item.TransactionDateUtc;
            worksheet.Cell(row, 2).Value = item.ProductName;
            worksheet.Cell(row, 3).Value = item.ProductSku;
            worksheet.Cell(row, 4).Value = item.TransactionType.ToString();
            worksheet.Cell(row, 5).Value = item.Quantity;
            worksheet.Cell(row, 6).Value = item.ReferenceNumber;
            worksheet.Cell(row, 7).Value = item.Remarks ?? "";
            row++;
        }

        worksheet.Column(1).Style.NumberFormat.Format = "yyyy-mm-dd hh:mm:ss";
        FormatDecimalColumns(worksheet, 5, 5);

        return Save(workbook);
    }

    public byte[] CreateLowStock(
        IReadOnlyList<LowStockDto> items)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Low Stock");

        WriteHeader(
            worksheet,
            "Product",
            "SKU",
            "Category",
            "Quantity On Hand");

        var row = 2;
        foreach (var item in items)
        {
            worksheet.Cell(row, 1).Value = item.ProductName;
            worksheet.Cell(row, 2).Value = item.ProductSku;
            worksheet.Cell(row, 3).Value = item.CategoryName ?? "";
            worksheet.Cell(row, 4).Value = item.QuantityOnHand;
            row++;
        }

        FormatDecimalColumns(worksheet, 4, 4);

        return Save(workbook);
    }

    public byte[] CreateInventoryMovement(
        IReadOnlyList<InventoryMovementDto> items)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Inventory Movement");

        WriteHeader(
            worksheet,
            "Product",
            "SKU",
            "Opening Quantity",
            "Stock In",
            "Stock Out",
            "Adjustment",
            "Closing Quantity");

        var row = 2;
        foreach (var item in items)
        {
            worksheet.Cell(row, 1).Value = item.ProductName;
            worksheet.Cell(row, 2).Value = item.ProductSku;
            worksheet.Cell(row, 3).Value = item.OpeningQuantity;
            worksheet.Cell(row, 4).Value = item.StockInQuantity;
            worksheet.Cell(row, 5).Value = item.StockOutQuantity;
            worksheet.Cell(row, 6).Value = item.AdjustmentQuantity;
            worksheet.Cell(row, 7).Value = item.ClosingQuantity;
            row++;
        }

        FormatDecimalColumns(worksheet, 3, 7);

        return Save(workbook);
    }

    public byte[] CreateProductReports(
        IReadOnlyList<ProductReportDto> items)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Product Reports");

        WriteHeader(
            worksheet,
            "Product",
            "SKU",
            "Category",
            "Unit",
            "Quantity On Hand",
            "Cost Price",
            "Selling Price",
            "Status");

        var row = 2;
        foreach (var item in items)
        {
            worksheet.Cell(row, 1).Value = item.ProductName;
            worksheet.Cell(row, 2).Value = item.ProductSku;
            worksheet.Cell(row, 3).Value = item.CategoryName ?? "";
            worksheet.Cell(row, 4).Value = item.UnitName ?? "";
            worksheet.Cell(row, 5).Value = item.QuantityOnHand;
            worksheet.Cell(row, 6).Value = item.CostPrice;
            worksheet.Cell(row, 7).Value = item.SellingPrice;
            worksheet.Cell(row, 8).Value = item.IsActive ? "Active" : "Inactive";
            row++;
        }

        FormatDecimalColumns(worksheet, 5, 7);

        return Save(workbook);
    }

    private static void WriteHeader(
        IXLWorksheet worksheet,
        params string[] headers)
    {
        for (var column = 0; column < headers.Length; column++)
        {
            worksheet.Cell(1, column + 1).Value = headers[column];
        }

        worksheet.Row(1).Style.Font.Bold = true;
    }

    private static void FormatDecimalColumns(
        IXLWorksheet worksheet,
        int firstColumn,
        int lastColumn)
    {
        for (var column = firstColumn; column <= lastColumn; column++)
        {
            worksheet.Column(column).Style.NumberFormat.Format = "0.00";
        }
    }

    private static byte[] Save(XLWorkbook workbook)
    {
        foreach (var worksheet in workbook.Worksheets)
        {
            worksheet.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
