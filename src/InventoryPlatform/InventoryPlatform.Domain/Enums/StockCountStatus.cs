using System.ComponentModel.DataAnnotations;

namespace InventoryPlatform.Domain.Enums;

public enum StockCountStatus
{
    [Display(Name = "Draft")]
    Draft = 1,

    [Display(Name = "Preparing")]
    Preparing = 2,

    [Display(Name = "Counting")]
    Counting = 3,

    [Display(Name = "Submitted")]
    Submitted = 4,

    [Display(Name = "In Review")]
    InReview = 5,

    [Display(Name = "Approved")]
    Approved = 6,

    [Display(Name = "Posted")]
    Posted = 7,

    [Display(Name = "Rejected")]
    Rejected = 8,

    [Display(Name = "Cancelled")]
    Cancelled = 9
}
