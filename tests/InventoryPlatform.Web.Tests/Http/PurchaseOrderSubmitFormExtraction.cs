using System.Net;
using System.Text.RegularExpressions;

namespace InventoryPlatform.Web.Tests.Http;

/// <summary>
/// T07 test helper: extracts the real rendered Purchase Order Details
/// <c>&lt;form&gt;</c> that carries the Submit action, its effective action
/// URL, and every hidden input value (identifier, navigation state, and the
/// real antiforgery token) from a Details GET response.
///
/// Intentionally narrow: BCL regex and HTML attribute decoding only — no HTML
/// parser package, no token manufacturing, no antiforgery services. The Submit
/// form is identified among the page's several forms (Submit, Cancel, layout
/// logout) by its production "Submit Purchase Order" button text.
/// </summary>
internal static class PurchaseOrderSubmitFormExtraction
{
    private const string SubmitFormButtonMarker = "Submit Purchase Order";

    private static readonly Regex FormOpenTagRegex = new(
        @"<form\b(?<attributes>[^>]*)>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex InputTagRegex = new(
        @"<input\b(?<attributes>[^>]*?)/?>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static PurchaseOrderSubmitForm ExtractSubmitForm(string html)
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

            if (!formRegion.Contains(
                    SubmitFormButtonMarker,
                    StringComparison.Ordinal))
            {
                continue;
            }

            var action = GetAttributeValue(
                formOpenTag.Groups["attributes"].Value,
                "action");

            if (string.IsNullOrWhiteSpace(action))
            {
                throw new InvalidOperationException(
                    "The rendered Purchase Order Submit form did not carry an " +
                    "action attribute." +
                    BuildHtmlExcerptDiagnostic(html));
            }

            var hiddenFields = new Dictionary<string, string>(
                StringComparer.Ordinal);

            foreach (Match inputTag in InputTagRegex.Matches(formRegion))
            {
                var attributes = inputTag.Groups["attributes"].Value;

                var inputType = GetAttributeValue(attributes, "type");

                if (!string.Equals(
                        inputType,
                        "hidden",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var name = GetAttributeValue(attributes, "name");

                if (name is null)
                {
                    continue;
                }

                hiddenFields[name] =
                    GetAttributeValue(attributes, "value") ?? string.Empty;
            }

            return new PurchaseOrderSubmitForm(action, hiddenFields);
        }

        throw new InvalidOperationException(
            "The response did not contain a rendered Purchase Order Submit form " +
            $"carrying the '{SubmitFormButtonMarker}' button." +
            BuildHtmlExcerptDiagnostic(html));
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

    private static string BuildHtmlExcerptDiagnostic(string html)
    {
        var excerpt = html.Length <= 800
            ? html
            : $"{html.AsSpan(0, 800)}…";

        return $"{Environment.NewLine}Rendered HTML excerpt: {excerpt}";
    }
}

/// <summary>
/// The effective Submit form contract consumed from a real rendered Details
/// response: the form action to POST to and every rendered hidden field.
/// </summary>
public sealed record PurchaseOrderSubmitForm(
    string Action,
    IReadOnlyDictionary<string, string> HiddenFields);
