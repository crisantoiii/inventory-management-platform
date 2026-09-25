using System.Net;
using System.Text.RegularExpressions;

namespace InventoryPlatform.Web.Tests.Http;

internal static class PurchaseOrderCreateFormExtraction
{
    private const string AntiforgeryTokenFieldName = "__RequestVerificationToken";

    private const string PurchaseOrderCreatePath =
        "/Purchasing/PurchaseOrders/Create";

    private const string SubmitButtonMarker = "Create Purchase Order";

    private static readonly Regex FormOpenTagRegex = new(
        @"<form\b(?<attributes>[^>]*)>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex InputTagRegex = new(
        @"<input\b(?<attributes>[^>]*?)/?>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static string ExtractCreateFormAction(string html)
    {
        var form = FindCreateForm(html);
        var action = GetAttributeValue(form.OpenTag.Groups["attributes"].Value, "action");

        return string.IsNullOrWhiteSpace(action)
            ? PurchaseOrderCreatePath
            : action;
    }

    public static string ExtractCreateFormAntiforgeryToken(string html)
    {
        var form = FindCreateForm(html);

        foreach (Match inputTag in InputTagRegex.Matches(form.Region))
        {
            var attributes = inputTag.Groups["attributes"].Value;

            if (!string.Equals(
                    GetAttributeValue(attributes, "name"),
                    AntiforgeryTokenFieldName,
                    StringComparison.Ordinal))
            {
                continue;
            }

            var value = GetAttributeValue(attributes, "value");

            return string.IsNullOrWhiteSpace(value)
                ? throw new InvalidOperationException(
                    "The rendered Purchase Order Create antiforgery input was empty.")
                : value;
        }

        throw new InvalidOperationException(
            "The rendered Purchase Order Create form did not contain an antiforgery token.");
    }

    private static (Match OpenTag, string Region) FindCreateForm(string html)
    {
        foreach (Match formOpenTag in FormOpenTagRegex.Matches(html))
        {
            var formCloseIndex = html.IndexOf(
                "</form>",
                formOpenTag.Index + formOpenTag.Length,
                StringComparison.OrdinalIgnoreCase);

            if (formCloseIndex < 0)
            {
                continue;
            }

            var formRegion = html.Substring(
                formOpenTag.Index,
                formCloseIndex - formOpenTag.Index);

            if (formRegion.Contains(SubmitButtonMarker, StringComparison.Ordinal) &&
                formRegion.Contains(
                    "name=\"PurchaseOrder.SupplierId\"",
                    StringComparison.Ordinal))
            {
                return (formOpenTag, formRegion);
            }
        }

        throw new InvalidOperationException(
            "The response did not contain the rendered Purchase Order Create form.");
    }

    private static string? GetAttributeValue(
        string attributes,
        string attributeName)
    {
        var match = Regex.Match(
            attributes,
            $"\\b{Regex.Escape(attributeName)}\\s*=\\s*\"(?<value>[^\"]*)\"",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        return match.Success
            ? WebUtility.HtmlDecode(match.Groups["value"].Value)
            : null;
    }
}
