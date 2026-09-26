using Xunit;

namespace InventoryPlatform.IntegrationTests;

[Trait(TestTiers.TraitKey, TestTiers.ProviderNeutral)]
public sealed class TierClassificationRulesTests
{
    [Fact]
    public void ValidProviderNeutral_IsAccepted()
    {
        var errors = TierClassificationAudit.Validate(Descriptor(TestTiers.ProviderNeutral));

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidSqlServerRelational_IsAccepted()
    {
        var errors = TierClassificationAudit.Validate(Descriptor(TestTiers.SqlServerRelational));

        Assert.Empty(errors);
    }

    [Fact]
    public void MissingTier_IsRejectedWithTestIdentityAndObservedClassification()
    {
        var errors = TierClassificationAudit.Validate(Descriptor());

        var error = Assert.Single(errors);
        Assert.Contains(TestIdentity, error, StringComparison.Ordinal);
        Assert.Contains("expected exactly one TestTier value", error, StringComparison.Ordinal);
        Assert.Contains("<none>", error, StringComparison.Ordinal);
    }

    [Fact]
    public void DuplicateTier_IsRejectedWithObservedClassification()
    {
        var errors = TierClassificationAudit.Validate(
            Descriptor(TestTiers.ProviderNeutral, TestTiers.ProviderNeutral));

        var error = Assert.Single(errors);
        Assert.Contains(TestIdentity, error, StringComparison.Ordinal);
        Assert.Contains("observed 2", error, StringComparison.Ordinal);
        Assert.Contains("'ProviderNeutral', 'ProviderNeutral'", error, StringComparison.Ordinal);
    }

    [Fact]
    public void MultipleSupportedTiers_AreRejectedWithObservedClassification()
    {
        var errors = TierClassificationAudit.Validate(
            Descriptor(TestTiers.ProviderNeutral, TestTiers.SqlServerRelational));

        var error = Assert.Single(errors);
        Assert.Contains(TestIdentity, error, StringComparison.Ordinal);
        Assert.Contains("observed 2", error, StringComparison.Ordinal);
        Assert.Contains("'ProviderNeutral', 'SqlServerRelational'", error, StringComparison.Ordinal);
    }

    [Fact]
    public void UnknownTier_IsRejectedWithSupportedValues()
    {
        var errors = TierClassificationAudit.Validate(Descriptor("UnexpectedTier"));

        var error = Assert.Single(errors);
        Assert.Contains(TestIdentity, error, StringComparison.Ordinal);
        Assert.Contains("unknown TestTier value", error, StringComparison.Ordinal);
        Assert.Contains("UnexpectedTier", error, StringComparison.Ordinal);
        Assert.Contains("ProviderNeutral, SqlServerRelational", error, StringComparison.Ordinal);
    }

    private const string TestIdentity = "Synthetic.Tests.Example";

    private static TestTierDescriptor Descriptor(params string[] tierValues)
    {
        return new TestTierDescriptor(TestIdentity, tierValues);
    }
}

[Trait(TestTiers.TraitKey, TestTiers.ProviderNeutral)]
public sealed class TierClassificationLiveAuditTests
{
    private static readonly string[] LockedSqlServerRelationalInventory =
    [
        "InventoryPlatform.IntegrationTests.Relational.RelationalTestInfrastructureSqlServerTests.CreateAsync_OnApprovedLocalDB_CreatesRealDatabaseAndReportsReachability",
        "InventoryPlatform.IntegrationTests.Relational.RelationalTestInfrastructureSqlServerTests.CreateContext_ExecutesRealProviderOperationsOnTheOwnedDatabase",
        "InventoryPlatform.IntegrationTests.Relational.RelationalTestInfrastructureSqlServerTests.DisposeAsync_DropsTheOwnedDatabase",
        "InventoryPlatform.IntegrationTests.Relational.RelationalTestInfrastructureSqlServerTests.DisposeAsync_CleanupIsBestEffortAndReportsFailures",
        "InventoryPlatform.IntegrationTests.Relational.RelationalTestInfrastructureSqlServerTests.DisposeAsync_CleanupIsRestrictedToGuardValidatedOwnedTarget",
        "InventoryPlatform.IntegrationTests.Relational.RelationalTestInfrastructureSqlServerTests.CreateAsync_IndependentInstances_OwnIndependentDatabases",
        "InventoryPlatform.IntegrationTests.Relational.SqlServerConstraintTests.DuplicateProductSku_IsRejectedBySqlServerUniqueIndex",
        "InventoryPlatform.IntegrationTests.Relational.SqlServerConstraintTests.DeletingReferencedCategory_IsRejectedBySqlServerForeignKey",
        "InventoryPlatform.IntegrationTests.Relational.SqlServerInventoryMovementQueryTests.GetInventoryMovementAsync_OnRealSqlServer_TranslatesProductionQueryAndReturnsDeterministicAggregates",
        "InventoryPlatform.IntegrationTests.Relational.SqlServerMigrationTests.MigrateAsync_OnFreshGuardedLocalDBDatabase_AppliesFullMigrationChain",
        "InventoryPlatform.IntegrationTests.Relational.SqlServerMigrationTests.MigrateAsync_IsRepeatableAcrossIndependentFreshDatabases",
        "InventoryPlatform.IntegrationTests.Relational.SqlServerStorageSemanticsTests.SingleSaveChanges_WithOneConstraintViolation_PersistsNoPartialWrites",
        "InventoryPlatform.IntegrationTests.Relational.SqlServerStorageSemanticsTests.Decimal18_2Column_StoresDeterministicallyRoundedValue_ForSubscaleInput",
    ];

    [Fact]
    public void EveryIntegrationTest_HasExactlyOneSupportedTier_AndLockedSqlInventoryIsIntact()
    {
        Assert.Equal(
            [TestTiers.ProviderNeutral, TestTiers.SqlServerRelational],
            TierClassificationAudit.SupportedTierValues);

        var tests = TierClassificationAudit.DiscoverTestMethods(typeof(TierClassificationLiveAuditTests).Assembly);
        var classificationErrors = tests
            .SelectMany(TierClassificationAudit.Validate)
            .ToArray();

        Assert.True(
            classificationErrors.Length == 0,
            "IntegrationTest tier classification violations:" + Environment.NewLine +
            string.Join(Environment.NewLine, classificationErrors.Select(error => $"- {error}")));

        var observedSqlInventory = tests
            .Where(test => test.TierValues.SequenceEqual([TestTiers.SqlServerRelational]))
            .Select(test => test.TestIdentity)
            .OrderBy(identity => identity, StringComparer.Ordinal)
            .ToArray();
        var expectedSqlInventory = LockedSqlServerRelationalInventory
            .OrderBy(identity => identity, StringComparer.Ordinal)
            .ToArray();
        var missing = expectedSqlInventory.Except(observedSqlInventory, StringComparer.Ordinal).ToArray();
        var unexpected = observedSqlInventory.Except(expectedSqlInventory, StringComparer.Ordinal).ToArray();

        Assert.True(
            missing.Length == 0 && unexpected.Length == 0,
            "Locked SqlServerRelational inventory mismatch." + Environment.NewLine +
            $"Missing, renamed, or declassified tests: [{string.Join(", ", missing)}]" + Environment.NewLine +
            $"Unexpectedly classified SqlServerRelational tests: [{string.Join(", ", unexpected)}]" + Environment.NewLine +
            $"Expected count: {expectedSqlInventory.Length}; observed count: {observedSqlInventory.Length}.");
    }
}
