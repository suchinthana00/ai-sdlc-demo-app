using InvoiceApp.Domain;
using Xunit;

namespace InvoiceApp.Tests;

public class ApprovalServiceTests
{
    private static Invoice InvoiceWith(string id, InvoiceStatus status, decimal amount = 100m, params string[] approvalHistory) =>
        new()
        {
            Id = id,
            CustomerName = "Test School",
            Amount = amount,
            Status = status,
            ApprovalHistory = approvalHistory.ToList()
        };

    private static ApprovalService ServiceFor(params Invoice[] invoices) =>
        new(new FakeInvoiceRepository(invoices));

    [Fact]
    public void Submit_MovesDraftToPendingApproval()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.Draft);

        var result = ServiceFor(invoice).Submit("INV-1");

        Assert.Equal(InvoiceStatus.PendingApproval, result.Status);
    }

    [Fact]
    public void Submit_RejectsInvoiceThatIsNotADraft()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.Approved));

        Assert.Throws<InvalidOperationException>(() => service.Submit("INV-1"));
    }

    [Fact]
    public void Approve_ManagerBelowLimit_SetsStatusApproverAndTimestamp()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 49_999m));

        var result = service.Approve("INV-1", "demo.manager", "Manager");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("demo.manager", result.DecidedBy);
        Assert.NotNull(result.DecidedAt);
    }

    [Fact]
    public void Approve_ManagerAtLimit_SetsStatusApproverAndTimestamp()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 50_000m));

        var result = service.Approve("INV-1", "demo.manager", "Manager");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
    }

    [Fact]
    public void Approve_ManagerAboveLimit_ThrowsClearReason()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 50_001m));

        var ex = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "demo.manager", "Manager"));

        Assert.Contains("50,000", ex.Message);
        Assert.Contains("Manager", ex.Message);
    }

    [Fact]
    public void Approve_DirectorBelowLimit_SetsStatusApproverAndTimestamp()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 499_999m));

        var result = service.Approve("INV-1", "demo.director", "Director");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("demo.director", result.DecidedBy);
        Assert.NotNull(result.DecidedAt);
    }

    [Fact]
    public void Approve_DirectorAtLimit_SetsStatusApproverAndTimestamp()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 500_000m));

        var result = service.Approve("INV-1", "demo.director", "Director");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
    }

    [Fact]
    public void Approve_DirectorAboveLimit_KeepInvoicePendingUntilSecondDistinctDirectorApproves()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 500_001m);
        var service = ServiceFor(invoice);

        var firstApproval = service.Approve("INV-1", "demo.director", "Director");

        Assert.Equal(InvoiceStatus.PendingApproval, firstApproval.Status);
        Assert.Equal("demo.director", firstApproval.ApprovalHistory.Single());

        var secondApproval = service.Approve("INV-1", "director.two", "Director");

        Assert.Equal(InvoiceStatus.Approved, secondApproval.Status);
        Assert.Equal(2, secondApproval.ApprovalHistory.Count);
        Assert.Contains("demo.director", secondApproval.ApprovalHistory);
        Assert.Contains("director.two", secondApproval.ApprovalHistory);
    }

    [Fact]
    public void Approve_HighValueInvoice_SecondApprovalFromManager_RejectsClearReason()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 600_000m, "director.one");
        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "demo.manager", "Manager"));

        Assert.Contains("Second approver", ex.Message);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Single(invoice.ApprovalHistory);
    }

    [Fact]
    public void Approve_HighValueInvoice_SameDirectorAttemptingTwice_Rejects()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 600_000m, "director.one");
        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "director.one", "Director"));

        Assert.Contains("already approved", ex.Message);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Single(invoice.ApprovalHistory);
    }

    [Fact]
    public void Approve_HighValueInvoice_FirstApproverRepeatingRequest_Rejects()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 600_000m, "director.one");
        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "director.one", "Director"));

        Assert.Contains("already approved", ex.Message);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Single(invoice.ApprovalHistory);
    }

    [Fact]
    public void Approve_HighValueInvoice_RejectedSecondApproval_PreservesStatus()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 600_000m, "director.one");
        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "demo.manager", "Manager"));

        Assert.Contains("Second approver", ex.Message);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Equal(1, invoice.ApprovalHistory.Count);
        Assert.Equal("director.one", invoice.ApprovalHistory[0]);
    }

    [Fact]
    public void Approve_HighValueInvoice_RejectedSecondApproval_PreservesApprovalHistory()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 600_000m, "director.one");
        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "demo.manager", "Manager"));

        Assert.Contains("Second approver", ex.Message);
        Assert.Single(invoice.ApprovalHistory);
        Assert.Contains("director.one", invoice.ApprovalHistory);
        Assert.DoesNotContain("demo.manager", invoice.ApprovalHistory);
    }

    [Fact]
    public void Approve_RejectedAttemptPreservesInvoiceState()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 75_000m);
        invoice.DecidedBy = "previous.owner";
        invoice.DecidedAt = new DateTime(2024, 5, 10, 12, 0, 0, DateTimeKind.Utc);
        invoice.DecisionNote = "Initial note";

        var service = ServiceFor(invoice);

        var originalStatus = invoice.Status;
        var originalDecidedBy = invoice.DecidedBy;
        var originalDecidedAt = invoice.DecidedAt;
        var originalDecisionNote = invoice.DecisionNote;

        var ex = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "demo.manager", "Manager"));

        Assert.Contains("50,000", ex.Message);
        Assert.Equal(originalStatus, invoice.Status);
        Assert.Equal(originalDecidedBy, invoice.DecidedBy);
        Assert.Equal(originalDecidedAt, invoice.DecidedAt);
        Assert.Equal(originalDecisionNote, invoice.DecisionNote);
    }

    [Fact]
    public void Approve_RejectsInvoiceThatIsStillADraft()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.Draft));

        Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "demo.approver", "Approver"));
    }

    [Fact]
    public void Approve_ThrowsWhenInvoiceDoesNotExist()
    {
        var service = ServiceFor();

        Assert.Throws<KeyNotFoundException>(() => service.Approve("INV-404", "demo.approver", "Approver"));
    }

    [Fact]
    public void Reject_RecordsReasonAndApprover()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval));

        var result = service.Reject("INV-1", "demo.approver", "Duplicate invoice");

        Assert.Equal(InvoiceStatus.Rejected, result.Status);
        Assert.Equal("Duplicate invoice", result.DecisionNote);
    }

    [Fact]
    public void Reject_RequiresAReason()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval));

        Assert.Throws<ArgumentException>(() => service.Reject("INV-1", "demo.approver", "  "));
    }
}
