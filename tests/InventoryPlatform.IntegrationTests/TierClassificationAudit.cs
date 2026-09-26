using System.Reflection;
using Xunit;

namespace InventoryPlatform.IntegrationTests;

internal sealed record TestTierDescriptor(
    string TestIdentity,
    IReadOnlyList<string> TierValues);

internal static class TierClassificationAudit
{
    public static IReadOnlyList<string> SupportedTierValues { get; } =
    [
        TestTiers.ProviderNeutral,
        TestTiers.SqlServerRelational,
    ];

    public static IReadOnlyList<TestTierDescriptor> DiscoverTestMethods(Assembly assembly)
    {
        return assembly
            .GetTypes()
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .SelectMany(type => type
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(IsXunitTestMethod)
                .OrderBy(method => method.Name, StringComparer.Ordinal)
                .Select(method => new TestTierDescriptor(
                    $"{type.FullName}.{method.Name}",
                    ResolveTierValues(type, method))))
            .ToArray();
    }

    public static IReadOnlyList<string> Validate(TestTierDescriptor test)
    {
        var observed = test.TierValues.Count == 0
            ? "<none>"
            : string.Join(", ", test.TierValues.Select(value => $"'{value}'"));
        var errors = new List<string>();
        var unknownValues = test.TierValues
            .Where(value => !SupportedTierValues.Contains(value, StringComparer.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        if (unknownValues.Length > 0)
        {
            errors.Add(
                $"{test.TestIdentity}: unknown {TestTiers.TraitKey} value(s) " +
                $"[{string.Join(", ", unknownValues.Select(value => $"'{value}'"))}]. " +
                $"Observed classification: [{observed}]. Supported values: " +
                $"[{string.Join(", ", SupportedTierValues)}].");
        }

        if (test.TierValues.Count != 1)
        {
            errors.Add(
                $"{test.TestIdentity}: expected exactly one {TestTiers.TraitKey} value but observed " +
                $"{test.TierValues.Count}: [{observed}]. Apply exactly one supported tier through " +
                "class-level or method-level metadata, not both.");
        }

        return errors;
    }

    private static bool IsXunitTestMethod(MethodInfo method)
    {
        return method.GetCustomAttributes(inherit: true)
            .Any(attribute => attribute is FactAttribute);
    }

    private static IReadOnlyList<string> ResolveTierValues(Type type, MethodInfo method)
    {
        return type.CustomAttributes
            .Concat(method.CustomAttributes)
            .Where(attribute => attribute.AttributeType == typeof(TraitAttribute))
            .Where(attribute =>
                attribute.ConstructorArguments.Count == 2 &&
                string.Equals(
                    attribute.ConstructorArguments[0].Value as string,
                    TestTiers.TraitKey,
                    StringComparison.Ordinal))
            .Select(attribute => attribute.ConstructorArguments[1].Value as string ?? string.Empty)
            .ToArray();
    }
}
