using System.Net;
using System.Text.RegularExpressions;

namespace InventoryPlatform.Web.Tests.Http;

/// <summary>
/// T06 test helper: extracts the real rendered <c>&lt;form&gt;</c> action and the
/// real hidden antiforgery input value from a Category Create GET response.
/// Intentionally narrow: BCL regex and HTML attribute decoding only —
/// no HTML parser package, no token manufacturing, no antiforgery services.
/// </summary>
internal static class CategoryCreateFormExtraction
{
    private const string AntiforgeryTokenFieldName = "__RequestVerificationToken";

    private const string CategoryCreateActionMarker = "/Categories/Create";

    private const string CategoryCreatePath = "/Categories/Create";

    private static readonly Regex FormOpenTagRegex = new(
        @"<form\b(?<attributes>[^>]*)>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex InputTagRegex = new(
        @"<input\b(?<attributes>[^>]*?)/?>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static string ExtractCreateFormAction(string html)
    {
        var formOpenTag = FindCreateFormOpenTag(html);

        var action = GetAttributeValue(
            formOpenTag.Groups["attributes"].Value,
            "action");

        // Razor Pages renders `<form method="post">` without an action
        // attribute; the browser then posts to the current request URL
        // (/Categories/Create for this page).
        return string.IsNullOrWhiteSpace(action)
            ? CategoryCreatePath
            : action;
    }

    public static string ExtractCreateFormAntiforgeryToken(string html)
    {
        var formOpenTag = FindCreateFormOpenTag(html);

        var formCloseIndex = html.IndexOf(
            "</form>",
            formOpenTag.Index + formOpenTag.Length,
            StringComparison.OrdinalIgnoreCase);

        if (formCloseIndex < 0)
        {
            throw new InvalidOperationException(
                "The rendered Category Create form was not closed." +
                BuildHtmlExcerptDiagnostic(html));
        }

        var formRegion = html.Substring(
            formOpenTag.Index,
            formCloseIndex - formOpenTag.Index);

        foreach (Match inputTag in InputTagRegex.Matches(formRegion))
        {
            var attributes = inputTag.Groups["attributes"].Value;

            var name = GetAttributeValue(attributes, "name");

            if (string.Equals(
                    name,
                    AntiforgeryTokenFieldName,
                    StringComparison.Ordinal))
            {
                var value = GetAttributeValue(attributes, "value");

                return string.IsNullOrWhiteSpace(value)
                    ? throw new InvalidOperationException(
                        "The rendered antiforgery hidden input did not contain a token value." +
                        BuildHtmlExcerptDiagnostic(html))
                    : value;
            }
        }

        throw new InvalidOperationException(
            "The rendered Category Create form did not contain a hidden " +
            $"{AntiforgeryTokenFieldName} input." +
            BuildHtmlExcerptDiagnostic(html));
    }

    private static Match FindCreateFormOpenTag(string html)
    {
        foreach (Match formOpenTag in FormOpenTagRegex.Matches(html))
        {
            var attributes = formOpenTag.Groups["attributes"].Value;

            var action = GetAttributeValue(attributes, "action");

            // The Create form renders without an action attribute; other
            // forms on the page (e.g. the layout logout form) carry their
            // own explicit action and are excluded by this check.
            if (action is null && HasMethodPostAttribute(attributes))
            {
                return formOpenTag;
            }

            if (action is not null &&
                action.Contains(CategoryCreateActionMarker, StringComparison.Ordinal))
            {
                return formOpenTag;
            }
        }

        throw new InvalidOperationException(
            $"The response did not contain a rendered form posting to {CategoryCreateActionMarker}." +
            BuildHtmlExcerptDiagnostic(html));
    }

    private static bool HasMethodPostAttribute(string attributes)
    {
        return Regex.IsMatch(
            attributes,
            "\\bmethod\\s*=\\s*\"post\"",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
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
