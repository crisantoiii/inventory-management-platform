using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryPlatform.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; protected set; }
}
