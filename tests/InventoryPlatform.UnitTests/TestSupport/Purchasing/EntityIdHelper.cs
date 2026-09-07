using InventoryPlatform.Domain.Common;

namespace InventoryPlatform.UnitTests.TestSupport.Purchasing;

/// <summary>
/// Assigns entity ids for Purchasing tests. <see cref="BaseEntity.Id"/> exposes a
/// protected setter, so tests assign ids via reflection.
///
/// This mirrors the established private <c>SetEntityId</c> precedent in
/// <c>CapabilityAuthorizationServiceTests</c>; the helper is promoted to shared
/// Purchasing support (T01) because multiple Purchasing test classes need known
/// aggregate ids (T02-T05 response/mapping assertions) - the accepted Rule-of-Three
/// disposition in plan/SPRINT_13_PLANNING_REPORT.md Section 12.1.
///
/// Production setters and constructors are not altered by this helper.
/// </summary>
public static class EntityIdHelper
{
    /// <summary>
    /// Sets the entity's <see cref="BaseEntity.Id"/> via reflection and returns the same
    /// entity instance for fluent use in test arrangement.
    /// </summary>
    public static TEntity SetEntityId<TEntity>(TEntity entity, int id)
        where TEntity : BaseEntity
    {
        typeof(BaseEntity)
            .GetProperty(nameof(BaseEntity.Id))!
            .SetValue(entity, id);

        return entity;
    }
}
