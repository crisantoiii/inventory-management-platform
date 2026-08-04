using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryPlatform.Domain.Enums;

public enum PurchaseOrderStatus
{
    Draft = 1,

    Submitted = 2,

    Approved = 3,

    Receiving = 4,

    Completed = 5,

    Cancelled = 6
}
