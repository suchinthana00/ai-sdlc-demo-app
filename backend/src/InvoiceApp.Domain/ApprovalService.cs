using System.Globalization;

namespace InvoiceApp.Domain;

/// <summary>
/// Moves an invoice through its lifecycle: Draft -> PendingApproval -> Approved or Rejected.
/// One invoice at a time, decided as a whole by a single approver.
/// </summary>
public class ApprovalService
{
    private const decimal ManagerApprovalLimit = 50_000m;
    private const decimal DirectorApprovalLimit = 500_000m;

    private readonly IInvoiceRepository _repository;

    public ApprovalService(IInvoiceRepository repository)
    {
        _repository = repository;
    }

    public Invoice Submit(string invoiceId)
    {
        var invoice = Find(invoiceId);

        if (invoice.Status != InvoiceStatus.Draft)
        {
            throw new InvalidOperationException(
                $"Invoice {invoiceId} is {invoice.Status}; only drafts can be submitted.");
        }

        invoice.Status = InvoiceStatus.PendingApproval;
        _repository.Update(invoice);
        return invoice;
    }

    public Invoice Approve(string invoiceId, string approver) =>
        Approve(invoiceId, approver, "Approver");

    public Invoice Approve(string invoiceId, string approver, string role)
    {
        var invoice = Find(invoiceId);
        EnsurePending(invoice);
        ValidateApprovalLimit(invoice, role);

        invoice.Status = InvoiceStatus.Approved;
        invoice.DecidedBy = approver;
        invoice.DecidedAt = DateTime.UtcNow;
        _repository.Update(invoice);
        return invoice;
    }

    public Invoice Reject(string invoiceId, string approver, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A reason is required to reject an invoice.", nameof(reason));
        }

        var invoice = Find(invoiceId);
        EnsurePending(invoice);

        invoice.Status = InvoiceStatus.Rejected;
        invoice.DecidedBy = approver;
        invoice.DecidedAt = DateTime.UtcNow;
        invoice.DecisionNote = reason;
        _repository.Update(invoice);
        return invoice;
    }

    private Invoice Find(string invoiceId) =>
        _repository.GetById(invoiceId)
        ?? throw new KeyNotFoundException($"Invoice {invoiceId} was not found.");

    private static void EnsurePending(Invoice invoice)
    {
        if (invoice.Status != InvoiceStatus.PendingApproval)
        {
            throw new InvalidOperationException(
                $"Invoice {invoice.Id} is {invoice.Status}; only pending invoices can be decided.");
        }
    }

    private static void ValidateApprovalLimit(Invoice invoice, string role)
    {
        if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase) && invoice.Amount > ManagerApprovalLimit)
        {
            throw new InvalidOperationException(
                $"Manager approval rejected: amount {invoice.Amount.ToString("N2", CultureInfo.InvariantCulture)} exceeds the maximum of {ManagerApprovalLimit.ToString("N2", CultureInfo.InvariantCulture)} for Manager approvals.");
        }

        if (string.Equals(role, "Director", StringComparison.OrdinalIgnoreCase) && invoice.Amount > DirectorApprovalLimit)
        {
            throw new InvalidOperationException(
                $"Director approval rejected: amount {invoice.Amount.ToString("N2", CultureInfo.InvariantCulture)} exceeds the maximum of {DirectorApprovalLimit.ToString("N2", CultureInfo.InvariantCulture)} for Director approvals.");
        }
    }
}
