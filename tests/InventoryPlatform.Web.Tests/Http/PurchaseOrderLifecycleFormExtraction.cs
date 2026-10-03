using System.Net;
using System.Text.RegularExpressions;

namespace InventoryPlatform.Web.Tests.Http;

internal static class PurchaseOrderLifecycleFormExtraction
{
    private static readonly string[] NavigationFields =
    ["Search", "FromDate", "ToDate", "Status", "SortBy", "Descending", "PageNum", "PageSize"];
    private static readonly Regex FormRegex = new(
        @"<form\b(?<attributes>[^>]*)>(?<content>.*?)</form>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
    private static readonly Regex InputRegex = new(@"<input\b(?<attributes>[^>]*?)/?>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static PurchaseOrderLifecycleForm ExtractApproveForm(string html) =>
        ExtractOne(html, "Approve Purchase Order", "id");
    public static PurchaseOrderLifecycleForm ExtractCancelForm(string html) =>
        ExtractOne(html, "Cancel Purchase Order", "id");

    public static PurchaseOrderLifecycleForm ExtractReceiveForm(string html, int productId)
    {
        var form = Extract(html, "Receive").Single(candidate =>
            candidate.Fields.GetValueOrDefault("productId") ==
            productId.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Validate(form, "purchaseOrderId", "productId", "quantity");
        return form;
    }

    public static IReadOnlyList<string> ExtractModelLevelErrors(string html)
    {
        var summary = Regex.Match(html,
            @"<div\b[^>]*data-valmsg-summary\s*=\s*""true""[^>]*>(?<content>.*?)</div>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        if (!summary.Success) return [];
        return Regex.Matches(summary.Groups["content"].Value,
                @"<li\b[^>]*>(?<content>.*?)</li>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)
            .Select(match => WebUtility.HtmlDecode(
                Regex.Replace(match.Groups["content"].Value, "<[^>]+>", string.Empty)).Trim())
            .Where(message => message.Length > 0).ToArray();
    }

    private static PurchaseOrderLifecycleForm ExtractOne(
        string html, string marker, string identifier)
    {
        var form = Extract(html, marker).Single();
        Validate(form, identifier);
        return form;
    }

    private static IEnumerable<PurchaseOrderLifecycleForm> Extract(string html, string marker)
    {
        foreach (Match form in FormRegex.Matches(html))
        {
            var content = form.Groups["content"].Value;
            if (!content.Contains(marker, StringComparison.Ordinal)) continue;
            var action = Attribute(form.Groups["attributes"].Value, "action")
                ?? throw new InvalidOperationException($"The '{marker}' form has no action.");
            var fields = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (Match input in InputRegex.Matches(content))
            {
                var attributes = input.Groups["attributes"].Value;
                var name = Attribute(attributes, "name");
                if (name is not null) fields[name] = Attribute(attributes, "value") ?? string.Empty;
            }
            yield return new(action, fields);
        }
    }

    private static void Validate(PurchaseOrderLifecycleForm form, params string[] actionFields)
    {
        foreach (var name in actionFields.Concat(NavigationFields)
                     .Prepend("__RequestVerificationToken"))
            if (!form.Fields.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"Lifecycle form field '{name}' is missing.");
    }

    private static string? Attribute(string attributes, string name)
    {
        var match = Regex.Match(attributes,
            $"\\b{Regex.Escape(name)}\\s*=\\s*\"(?<value>[^\"]*)\"",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return match.Success ? WebUtility.HtmlDecode(match.Groups["value"].Value) : null;
    }
}

public sealed record PurchaseOrderLifecycleForm(
    string Action, IReadOnlyDictionary<string, string> Fields);
