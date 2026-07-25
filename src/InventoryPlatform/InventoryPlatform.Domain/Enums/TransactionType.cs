using System.ComponentModel.DataAnnotations;

public enum TransactionType
{
    [Display(Name = "Stock In")]
    StockIn,

    [Display(Name = "Stock Out")]
    StockOut,

    [Display(Name = "Adjustment")]
    Adjustment
}