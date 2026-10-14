using InventoryPlatform.Domain.Common;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Domain.Exceptions;

namespace InventoryPlatform.Domain.Entities;

public sealed class StockCountSession : AuditableEntity
{
    private readonly List<StockCountLine> _lines = new();
    public IReadOnlyCollection<StockCountLine> Lines => _lines.AsReadOnly();

    public DateOnly CountDate { get; private set; }
    public StockCountStatus Status { get; private set; } = StockCountStatus.Draft;
    public string? Notes { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? SubmittedAtUtc { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }
    public DateTime? PostedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public int? StartedByUserId { get; private set; }
    public int? SubmittedByUserId { get; private set; }
    public int? ReviewedByUserId { get; private set; }
    public int? PostedByUserId { get; private set; }
    public int? CancelledByUserId { get; private set; }

    private StockCountSession() { }

    public static StockCountSession Create(DateOnly countDate, string? notes = null)
    {
        return new StockCountSession
        {
            CountDate = countDate,
            Notes = notes,
            Status = StockCountStatus.Draft
        };
    }

    public void StartCounting(IEnumerable<(int productId, decimal expectedQuantity)> lines, int startedByUserId)
    {
        if (Status != StockCountStatus.Draft && Status != StockCountStatus.Preparing)
        {
            throw new DomainException("Only Draft/Preparing session can be started.");
        }

        _lines.Clear();
        foreach (var line in lines)
        {
            if (_lines.Any(l => l.ProductId == line.productId))
                throw new DomainException("Duplicate product in session.");
            _lines.Add(StockCountLine.Create(Id, line.productId, line.expectedQuantity));
        }

        Status = StockCountStatus.Counting;
        StartedAtUtc = DateTime.UtcNow;
        StartedByUserId = startedByUserId;
    }

    public void AddOrUpdateCount(int productId, decimal countedQuantity)
    {
        if (Status != StockCountStatus.Counting)
            throw new DomainException("Only Counting sessions allow updating counts.");

        var line = _lines.SingleOrDefault(l => l.ProductId == productId);
        if (line == null) throw new DomainException("Line not found.");

        // Use reflection-free approach via method; internal method exists
        line.GetType().GetMethod("SetCountedQuantity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(line, new object[] { countedQuantity, false });
    }

    public void Submit(int submittedByUserId)
    {
        if (Status != StockCountStatus.Counting)
            throw new DomainException("Only Counting sessions can be submitted.");
        Status = StockCountStatus.Submitted;
        SubmittedAtUtc = DateTime.UtcNow;
        SubmittedByUserId = submittedByUserId;
    }

    public void Approve(int reviewedByUserId)
    {
        if (Status != StockCountStatus.Submitted && Status != StockCountStatus.InReview)
            throw new DomainException("Only Submitted/InReview sessions can be approved.");
        Status = StockCountStatus.Approved;
        ReviewedAtUtc = DateTime.UtcNow;
        ReviewedByUserId = reviewedByUserId;
    }

    public void Reject(int reviewedByUserId, string? notes = null)
    {
        if (Status != StockCountStatus.Submitted && Status != StockCountStatus.InReview && Status != StockCountStatus.Approved)
            throw new DomainException("Cannot reject from current state.");
        Notes = notes ?? Notes;
        Status = StockCountStatus.Rejected;
        ReviewedAtUtc = DateTime.UtcNow;
        ReviewedByUserId = reviewedByUserId;
    }

    public void ReturnToCounting()
    {
        if (Status != StockCountStatus.Rejected)
            throw new DomainException("Only Rejected sessions can return to Counting.");
        Status = StockCountStatus.Counting;
    }

    public void MarkPosted(int postedByUserId)
    {
        if (Status != StockCountStatus.Approved)
            throw new DomainException("Only Approved sessions can be posted.");
        Status = StockCountStatus.Posted;
        PostedAtUtc = DateTime.UtcNow;
        PostedByUserId = postedByUserId;
    }

    public void Cancel(int cancelledByUserId, string? notes = null)
    {
        if (Status == StockCountStatus.Posted)
            throw new DomainException("Cannot cancel posted session.");
        Notes = notes ?? Notes;
        Status = StockCountStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
        CancelledByUserId = cancelledByUserId;
    }

    public void AddLine(StockCountLine line)
    {
        if (Status != StockCountStatus.Draft && Status != StockCountStatus.Preparing)
            throw new DomainException("Cannot modify lines after counting started.");
        if (_lines.Any(l => l.ProductId == line.ProductId))
            throw new DomainException("Duplicate product.");
        _lines.Add(line);
    }
}
