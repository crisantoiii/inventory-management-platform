using Xunit;

namespace InventoryPlatform.IntegrationTests;

[Trait(TestTiers.TraitKey, TestTiers.ProviderNeutral)]
public class IntegrationTest1
{
    [Fact]
    public void IntegrationTestInfrastructure_IsExecutable()
    {
        // Verifies the integration test project compiles and xUnit discovers tests.
        Assert.True(true);
    }
}
