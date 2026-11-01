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
        ValidateApprovalAttempt(invoice, approver, role);

        if (invoice.Amount > DirectorApprovalLimit)
        {
            if (invoice.ApprovalHistory.Count == 0)
            {
                invoice.ApprovalHistory.Add(approver);
                _repository.Update(invoice);
                return invoice;
            }

            if (invoice.ApprovalHistory.Count == 1)
            {
                invoice.ApprovalHistory.Add(approver);
                invoice.Status = InvoiceStatus.Approved;
                invoice.DecidedBy = approver;
                invoice.DecidedAt = DateTime.UtcNow;
                _repository.Update(invoice);
                return invoice;
            }

            throw new InvalidOperationException($"Invoice {invoice.Id} already has the required approvals.");
        }

        invoice.Status = InvoiceStatus.Approved;
        invoice.DecidedBy = approver;
        invoice.DecidedAt = DateTime.UtcNow;
        invoice.ApprovalHistory.Add(approver);
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

    private static void ValidateApprovalAttempt(Invoice invoice, string approver, string role)
    {
        if (string.IsNullOrWhiteSpace(approver))
        {
            throw new ArgumentException("Approver identity is required.", nameof(approver));
        }

        if (HasApprover(invoice, approver))
        {
            throw new InvalidOperationException($"User '{approver}' has already approved invoice {invoice.Id}.");
        }

        if (invoice.Amount > DirectorApprovalLimit)
        {
            if (invoice.ApprovalHistory.Count == 0)
            {
                if (!string.Equals(role, "Director", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("First approver for invoices above 500,000 must be a Director.");
                }

                return;
            }

            if (invoice.ApprovalHistory.Count == 1)
            {
                if (!string.Equals(role, "Director", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Second approver for invoices above 500,000 must be a Director.");
                }

                if (string.Equals(invoice.ApprovalHistory[0], approver, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"User '{approver}' cannot approve the same invoice twice.");
                }

                return;
            }

            throw new InvalidOperationException($"Invoice {invoice.Id} already has the required approvals.");
        }

        ValidateApprovalLimit(invoice, role);
    }

    private static bool HasApprover(Invoice invoice, string approver) =>
        invoice.ApprovalHistory.Any(existing => string.Equals(existing, approver, StringComparison.OrdinalIgnoreCase));

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
