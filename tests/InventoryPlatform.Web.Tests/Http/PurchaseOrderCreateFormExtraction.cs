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

    private static readonly Regex SelectTagRegex = new(
        @"<select\b(?<attributes>[^>]*)>(?<content>.*?)</select>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase |
        RegexOptions.Singleline | RegexOptions.CultureInvariant);

    private static readonly Regex OptionTagRegex = new(
        @"<option\b(?<attributes>[^>]*)>(?<content>.*?)</option>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase |
        RegexOptions.Singleline | RegexOptions.CultureInvariant);

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

    public static string ExtractValidationSummaryText(string html)
    {
        var form = FindCreateForm(html);
        var summary = Regex.Match(
            form.Region,
            @"<div\b(?=[^>]*\bclass=""[^""]*\bvalidation-summary-errors\b[^""]*"")[^>]*>(?<content>.*?)</div>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline |
            RegexOptions.CultureInvariant);

        if (!summary.Success)
        {
            throw new InvalidOperationException(
                "The redisplayed Purchase Order Create form did not contain a validation summary.");
        }

        var withoutTags = Regex.Replace(summary.Groups["content"].Value, "<[^>]+>", " ");
        return WebUtility.HtmlDecode(withoutTags);
    }

    public static string ExtractSelectedOptionValue(string html, string selectName)
    {
        var select = FindSelect(FindCreateForm(html).Region, selectName);

        foreach (Match option in OptionTagRegex.Matches(select.Groups["content"].Value))
        {
            var attributes = option.Groups["attributes"].Value;

            if (Regex.IsMatch(
                    attributes,
                    @"\bselected(?:\s*=\s*""[^""]*"")?",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                return GetAttributeValue(attributes, "value") ?? string.Empty;
            }
        }

        throw new InvalidOperationException(
            $"Select '{selectName}' did not contain a selected option.");
    }

    public static string ExtractInputValue(string html, string inputName)
    {
        var form = FindCreateForm(html);

        foreach (Match input in InputTagRegex.Matches(form.Region))
        {
            var attributes = input.Groups["attributes"].Value;

            if (string.Equals(
                    GetAttributeValue(attributes, "name"),
                    inputName,
                    StringComparison.Ordinal))
            {
                return GetAttributeValue(attributes, "value") ?? string.Empty;
            }
        }

        throw new InvalidOperationException(
            $"Input '{inputName}' was not rendered in the Purchase Order Create form.");
    }

    public static string ExtractTextareaValue(string html, string textareaName)
    {
        var form = FindCreateForm(html);
        var textarea = Regex.Match(
            form.Region,
            $"<textarea\\b(?=[^>]*\\bname=\\\"{Regex.Escape(textareaName)}\\\")[^>]*>" +
            "(?<content>.*?)</textarea>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline |
            RegexOptions.CultureInvariant);

        if (!textarea.Success)
        {
            throw new InvalidOperationException(
                $"Textarea '{textareaName}' was not rendered in the Purchase Order Create form.");
        }

        return WebUtility.HtmlDecode(textarea.Groups["content"].Value).Trim();
    }

    public static int CountItemRows(string html)
    {
        var form = FindCreateForm(html);

        return Regex.Matches(
            form.Region,
            @"\bdata-item-row(?:\s|>|=)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Count;
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

    private static Match FindSelect(string formRegion, string selectName)
    {
        foreach (Match select in SelectTagRegex.Matches(formRegion))
        {
            if (string.Equals(
                    GetAttributeValue(select.Groups["attributes"].Value, "name"),
                    selectName,
                    StringComparison.Ordinal))
            {
                return select;
            }
        }

        throw new InvalidOperationException(
            $"Select '{selectName}' was not rendered in the Purchase Order Create form.");
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
