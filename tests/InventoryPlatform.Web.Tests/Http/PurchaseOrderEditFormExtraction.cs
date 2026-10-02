using System.Net;
using System.Text.RegularExpressions;

namespace InventoryPlatform.Web.Tests.Http;

/// <summary>
/// Extracts the narrow semantic contract rendered by the Purchase Order Edit
/// page. This helper deliberately understands only the per-item Update and
/// Remove forms, their named inputs, the model-level validation summary, and
/// the Back-to-Details link needed by the frozen Sprint 22 HTTP scenarios.
/// </summary>
internal static class PurchaseOrderEditFormExtraction
{
    private const string UpdateButtonMarker = "Update";
    private const string RemoveButtonMarker = "Remove";
    private const string BackToDetailsMarker = "Back to Details";

    private static readonly Regex FormOpenTagRegex = new(
        @"<form\b(?<attributes>[^>]*)>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase |
        RegexOptions.CultureInvariant);

    private static readonly Regex InputTagRegex = new(
        @"<input\b(?<attributes>[^>]*?)/?>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase |
        RegexOptions.CultureInvariant);

    private static readonly Regex AnchorOpenTagRegex = new(
        @"<a\b(?<attributes>[^>]*)>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase |
        RegexOptions.CultureInvariant);

    public static PurchaseOrderEditForm ExtractUpdateForm(
        string html,
        int productId) =>
        ExtractItemForm(html, productId, UpdateButtonMarker);

    public static PurchaseOrderEditForm ExtractRemoveForm(
        string html,
        int productId) =>
        ExtractItemForm(html, productId, RemoveButtonMarker);

    public static IReadOnlyList<int> ExtractUpdateFormProductIds(string html)
    {
        var productIds = new List<int>();

        foreach (var form in EnumerateForms(html))
        {
            if (!ContainsButtonMarker(form.Region, UpdateButtonMarker))
            {
                continue;
            }

            var fields = ExtractNamedInputs(form.Region);

            if (fields.TryGetValue("productId", out var productId) &&
                int.TryParse(productId, out var parsedProductId))
            {
                productIds.Add(parsedProductId);
            }
        }

        return productIds;
    }

    public static string ExtractValidationSummaryText(string html)
    {
        var summary = Regex.Match(
            html,
            @"<div\b(?=[^>]*\bclass=""[^""]*\bvalidation-summary-errors\b[^""]*"")[^>]*>(?<content>.*?)</div>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline |
            RegexOptions.CultureInvariant);

        if (!summary.Success)
        {
            throw new InvalidOperationException(
                "The Purchase Order Edit page did not contain a model-level " +
                "validation summary with errors." + BuildHtmlExcerptDiagnostic(html));
        }

        var withoutTags = Regex.Replace(
            summary.Groups["content"].Value,
            "<[^>]+>",
            " ");

        return WebUtility.HtmlDecode(withoutTags);
    }

    public static string ExtractBackToDetailsHref(string html)
    {
        foreach (Match anchorOpenTag in AnchorOpenTagRegex.Matches(html))
        {
            var closeIndex = html.IndexOf(
                "</a>",
                anchorOpenTag.Index + anchorOpenTag.Length,
                StringComparison.OrdinalIgnoreCase);

            if (closeIndex < 0)
            {
                continue;
            }

            var region = html.Substring(
                anchorOpenTag.Index,
                closeIndex + "</a>".Length - anchorOpenTag.Index);

            if (!region.Contains(BackToDetailsMarker, StringComparison.Ordinal))
            {
                continue;
            }

            var href = GetAttributeValue(
                anchorOpenTag.Groups["attributes"].Value,
                "href");

            return string.IsNullOrWhiteSpace(href)
                ? throw new InvalidOperationException(
                    "The rendered Back-to-Details link did not carry an href." +
                    BuildHtmlExcerptDiagnostic(html))
                : href;
        }

        throw new InvalidOperationException(
            "The Purchase Order Edit page did not contain the Back-to-Details link." +
            BuildHtmlExcerptDiagnostic(html));
    }

    private static PurchaseOrderEditForm ExtractItemForm(
        string html,
        int productId,
        string buttonMarker)
    {
        foreach (var form in EnumerateForms(html))
        {
            if (!ContainsButtonMarker(form.Region, buttonMarker))
            {
                continue;
            }

            var fields = ExtractNamedInputs(form.Region);

            if (!fields.TryGetValue("productId", out var renderedProductId) ||
                !string.Equals(
                    renderedProductId,
                    productId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    StringComparison.Ordinal))
            {
                continue;
            }

            var action = GetAttributeValue(
                form.OpenTag.Groups["attributes"].Value,
                "action");

            if (string.IsNullOrWhiteSpace(action))
            {
                throw new InvalidOperationException(
                    $"The rendered Purchase Order Edit {buttonMarker} form for " +
                    $"product '{productId}' did not carry an action attribute." +
                    BuildHtmlExcerptDiagnostic(html));
            }

            if (!fields.TryGetValue("__RequestVerificationToken", out var token) ||
                string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException(
                    $"The rendered Purchase Order Edit {buttonMarker} form for " +
                    $"product '{productId}' did not contain a non-empty real " +
                    "antiforgery token." + BuildHtmlExcerptDiagnostic(html));
            }

            return new PurchaseOrderEditForm(action, fields);
        }

        throw new InvalidOperationException(
            $"The Purchase Order Edit page did not contain the {buttonMarker} " +
            $"form for product '{productId}'." + BuildHtmlExcerptDiagnostic(html));
    }

    private static IEnumerable<(Match OpenTag, string Region)> EnumerateForms(
        string html)
    {
        foreach (Match formOpenTag in FormOpenTagRegex.Matches(html))
        {
            var closeIndex = html.IndexOf(
                "</form>",
                formOpenTag.Index + formOpenTag.Length,
                StringComparison.OrdinalIgnoreCase);

            if (closeIndex < 0)
            {
                continue;
            }

            yield return (
                formOpenTag,
                html.Substring(
                    formOpenTag.Index,
                    closeIndex + "</form>".Length - formOpenTag.Index));
        }
    }

    private static IReadOnlyDictionary<string, string> ExtractNamedInputs(
        string formRegion)
    {
        var fields = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (Match inputTag in InputTagRegex.Matches(formRegion))
        {
            var attributes = inputTag.Groups["attributes"].Value;
            var name = GetAttributeValue(attributes, "name");

            if (name is null)
            {
                continue;
            }

            fields[name] = GetAttributeValue(attributes, "value") ?? string.Empty;
        }

        return fields;
    }

    private static bool ContainsButtonMarker(
        string formRegion,
        string marker) =>
        Regex.IsMatch(
            formRegion,
            $@"<button\b[^>]*>\s*{Regex.Escape(marker)}\s*</button>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline |
            RegexOptions.CultureInvariant);

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

    private static string BuildHtmlExcerptDiagnostic(string html)
    {
        var excerpt = html.Length <= 800
            ? html
            : $"{html.AsSpan(0, 800)}…";

        return $"{Environment.NewLine}Rendered HTML excerpt: {excerpt}";
    }
}

internal sealed record PurchaseOrderEditForm(
    string Action,
    IReadOnlyDictionary<string, string> Fields);
