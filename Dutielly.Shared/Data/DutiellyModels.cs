namespace Dutielly.Shared.Data;

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected
}

public sealed record CompanyProfile(
    string Id,
    string Name,
    string Industry,
    string City,
    string BrandColor,
    int Branches,
    int Locations);

public sealed record RoleProfile(
    string Name,
    string Summary,
    string[] Capabilities);

public sealed record PlatformModule(
    string Name,
    string Area,
    string Description,
    string Icon,
    string[] Features,
    string Status = "Ready");

public sealed record DashboardMetric(
    string Label,
    string Value,
    string Delta,
    string Tone,
    string Icon);

public sealed class EmployeeProfile
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Manager { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public DateOnly JoiningDate { get; set; }
    public string Status { get; set; } = "Active";
    public decimal MonthlyCtc { get; set; }
    public int LeaveBalance { get; set; }
    public double Utilization { get; set; }
    public string[] Skills { get; set; } = [];
}

public sealed class EmployeeDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.Now;
    public string UploadedBy { get; set; } = string.Empty;
}

public sealed class EmployeeDocumentContent
{
    public EmployeeDocument Metadata { get; set; } = new();
    public byte[] Content { get; set; } = [];
}

public sealed class AttendanceLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public TimeOnly CheckIn { get; set; }
    public TimeOnly? CheckOut { get; set; }
    public string Method { get; set; } = "Web Check-In";
    public string Location { get; set; } = string.Empty;
    public string SecuritySignal { get; set; } = "Tenant access verified";
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Approved;
}

public sealed class LeaveRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string LeaveType { get; set; } = "Casual";
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ApprovalStatus ManagerStatus { get; set; } = ApprovalStatus.Pending;
    public ApprovalStatus HrStatus { get; set; } = ApprovalStatus.Pending;
}

public sealed class ExpenseClaim
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EmployeeName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public ApprovalStatus ManagerStatus { get; set; } = ApprovalStatus.Pending;
    public ApprovalStatus FinanceStatus { get; set; } = ApprovalStatus.Pending;
}

public sealed class CandidatePipelineItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CandidateName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Stage { get; set; } = "Applied";
    public int AiScore { get; set; }
    public string Owner { get; set; } = string.Empty;
}

public sealed class PayrollRun
{
    public string Month { get; set; } = string.Empty;
    public int Employees { get; set; }
    public decimal GrossPay { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetPay { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

public sealed class HelpdeskTicket
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Category { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Open";
}

public sealed record AuditEvent(
    DateTime At,
    string Actor,
    string Action,
    string Module);

public sealed record AiInsight(
    string Title,
    string Body,
    string Severity,
    string Action);

public sealed record IntegrationStatus(
    string Name,
    string Category,
    string Status);

public sealed class DashboardSnapshot
{
    public IReadOnlyList<DashboardMetric> Metrics { get; init; } = [];
    public IReadOnlyList<AiInsight> Insights { get; init; } = [];
    public IReadOnlyList<PlatformModule> Modules { get; init; } = [];
    public PayrollRun Payroll { get; init; } = new();
    public int PendingApprovals { get; init; }
    public int ActiveEmployees { get; init; }
    public double AttendanceRate { get; init; }
}
