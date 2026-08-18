using InventoryPlatform.Application.DTOs.Reporting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InventoryPlatform.Web.Reports.Pdf;

public sealed class PdfReportWriter
{
    public const string ContentType = "application/pdf";

    public byte[] CreateInventoryValuation(IReadOnlyList<InventoryValuationDto> items)
        => CreateReport("Inventory Valuation", false, content =>
        {
            content.Table(table =>
            {
                DefineColumns(table, 2, 1, 1, 1, 1);
                AddHeader(table, "Product", "Category", "Quantity On Hand", "Cost Price", "Inventory Value");

                foreach (var item in items)
                {
                    AddCell(table, item.ProductName);
                    AddCell(table, item.CategoryName ?? string.Empty);
                    AddNumberCell(table, item.QuantityOnHand);
                    AddNumberCell(table, item.CostPrice);
                    AddNumberCell(table, item.InventoryValue);
                }

                table.Cell().ColumnSpan(4).Element(TotalLabelStyle).Text("Total Inventory Value");
                table.Cell().Element(TotalValueStyle).AlignRight().Text(FormatNumber(items.Sum(x => x.InventoryValue)));
            });
        });

    public byte[] CreatePurchaseHistory(IReadOnlyList<PurchaseHistoryDto> items)
        => CreateReport("Purchase History", true, content =>
        {
            content.Table(table =>
            {
                DefineColumns(table, 1, 1.5f, 1, 1, 1, 1, 1, 1);
                AddHeader(table, "PO ID", "Supplier", "Order Date", "Status", "Total Amount", "Total Quantity", "Received Quantity", "Remaining Quantity");

                foreach (var item in items)
                {
                    AddCell(table, item.PurchaseOrderId.ToString());
                    AddCell(table, item.SupplierName);
                    AddCell(table, item.OrderDate.ToString("yyyy-MM-dd"));
                    AddCell(table, item.Status.ToString());
                    AddNumberCell(table, item.TotalAmount);
                    AddNumberCell(table, item.TotalQuantity);
                    AddNumberCell(table, item.ReceivedQuantity);
                    AddNumberCell(table, item.RemainingQuantity);
                }
            });
        });

    public byte[] CreateSupplierPurchaseAnalysis(IReadOnlyList<SupplierPurchaseAnalysisDto> items)
        => CreateReport("Supplier Purchase Analysis", true, content =>
        {
            content.Table(table =>
            {
                DefineColumns(table, 1.5f, 1, 1, 1, 1, 1, 1, 1);
                AddHeader(table, "Supplier", "First Order Date", "Last Order Date", "Purchase Order Count", "Total Quantity", "Received Quantity", "Remaining Quantity", "Total Amount");

                foreach (var item in items)
                {
                    AddCell(table, item.SupplierName);
                    AddCell(table, item.FirstOrderDate.ToString("yyyy-MM-dd"));
                    AddCell(table, item.LastOrderDate.ToString("yyyy-MM-dd"));
                    AddCell(table, item.PurchaseOrderCount.ToString());
                    AddNumberCell(table, item.TotalQuantity);
                    AddNumberCell(table, item.ReceivedQuantity);
                    AddNumberCell(table, item.RemainingQuantity);
                    AddNumberCell(table, item.TotalAmount);
                }
            });
        });

    public byte[] CreateStockMovement(IReadOnlyList<StockMovementDto> items)
        => CreateReport("Stock Movement", true, content =>
        {
            content.Table(table =>
            {
                DefineColumns(table, 1.2f, 1.5f, 1, 1, 0.8f, 1.2f, 2);
                AddHeader(table, "Date (UTC)", "Product", "SKU", "Movement Type", "Quantity", "Reference Number", "Remarks");

                foreach (var item in items)
                {
                    AddCell(table, item.TransactionDateUtc.ToString("yyyy-MM-dd HH:mm:ss"));
                    AddCell(table, item.ProductName);
                    AddCell(table, item.ProductSku);
                    AddCell(table, item.TransactionType.ToString());
                    AddNumberCell(table, item.Quantity);
                    AddCell(table, item.ReferenceNumber);
                    AddCell(table, item.Remarks ?? string.Empty);
                }
            });
        });

    public byte[] CreateLowStock(IReadOnlyList<LowStockDto> items)
        => CreateReport("Low Stock", false, content =>
        {
            content.Table(table =>
            {
                DefineColumns(table, 2, 1.2f, 1.5f, 1);
                AddHeader(table, "Product", "SKU", "Category", "Quantity On Hand");

                foreach (var item in items)
                {
                    AddCell(table, item.ProductName);
                    AddCell(table, item.ProductSku);
                    AddCell(table, item.CategoryName ?? string.Empty);
                    AddNumberCell(table, item.QuantityOnHand);
                }
            });
        });

    public byte[] CreateInventoryMovement(IReadOnlyList<InventoryMovementDto> items)
        => CreateReport("Inventory Movement", true, content =>
        {
            content.Table(table =>
            {
                DefineColumns(table, 1.7f, 1.2f, 1, 1, 1, 1, 1);
                AddHeader(table, "Product", "SKU", "Opening Quantity", "Stock In", "Stock Out", "Adjustment", "Closing Quantity");

                foreach (var item in items)
                {
                    AddCell(table, item.ProductName);
                    AddCell(table, item.ProductSku);
                    AddNumberCell(table, item.OpeningQuantity);
                    AddNumberCell(table, item.StockInQuantity);
                    AddNumberCell(table, item.StockOutQuantity);
                    AddNumberCell(table, item.AdjustmentQuantity);
                    AddNumberCell(table, item.ClosingQuantity);
                }
            });
        });

    public byte[] CreateProductReports(IReadOnlyList<ProductReportDto> items)
        => CreateReport("Product Reports", true, content =>
        {
            content.Table(table =>
            {
                DefineColumns(table, 1.7f, 1.2f, 1.2f, 1, 1, 1, 1, 0.9f);
                AddHeader(table, "Product", "SKU", "Category", "Unit", "Quantity On Hand", "Cost Price", "Selling Price", "Status");

                foreach (var item in items)
                {
                    AddCell(table, item.ProductName);
                    AddCell(table, item.ProductSku);
                    AddCell(table, item.CategoryName ?? string.Empty);
                    AddCell(table, item.UnitName ?? string.Empty);
                    AddNumberCell(table, item.QuantityOnHand);
                    AddNumberCell(table, item.CostPrice);
                    AddNumberCell(table, item.SellingPrice);
                    AddCell(table, item.IsActive ? "Active" : "Inactive");
                }
            });
        });

    private static byte[] CreateReport(string title, bool landscape, Action<IContainer> composeContent)
    {
        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(landscape ? PageSizes.A4.Landscape() : PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Header()
                    .PaddingBottom(12)
                    .Column(column =>
                    {
                        column.Item().Text("Inventory Platform").FontSize(18).Bold();
                        column.Item().Text(title).FontSize(13).SemiBold();
                        column.Item().Text($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1);
                    });

                page.Content().Element(composeContent);

                page.Footer()
                    .PaddingTop(10)
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
            });
        }).GeneratePdf();
    }

    private static void DefineColumns(TableDescriptor table, params float[] relativeWidths)
    {
        table.ColumnsDefinition(columns =>
        {
            foreach (var width in relativeWidths)
                columns.RelativeColumn(width);
        });
    }

    private static void AddHeader(TableDescriptor table, params string[] headers)
    {
        table.Header(header =>
        {
            foreach (var headerText in headers)
                header.Cell().Element(HeaderCellStyle).Text(headerText).Bold();
        });
    }

    private static void AddCell(TableDescriptor table, string value)
        => table.Cell().Element(CellStyle).Text(value);

    private static void AddNumberCell(TableDescriptor table, decimal value)
        => table.Cell().Element(CellStyle).AlignRight().Text(FormatNumber(value));

    private static string FormatNumber(decimal value)
        => value.ToString("0.00");

    private static IContainer HeaderCellStyle(IContainer container)
        => container.Background(Colors.Grey.Lighten3).BorderBottom(1).Padding(5);

    private static IContainer CellStyle(IContainer container)
        => container.BorderBottom(0.5f).Padding(5);

    private static IContainer TotalLabelStyle(IContainer container)
        => container.BorderTop(1).Padding(5).AlignRight();

    private static IContainer TotalValueStyle(IContainer container)
        => container.BorderTop(1).Padding(5);
}
