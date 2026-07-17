using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryPlatform.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}
