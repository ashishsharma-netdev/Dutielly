using Dutielly.Shared.Data;

namespace Dutielly.Shared.Services;

public interface IDutiellyPlatformService
{
    string DataSourceName { get; }
    string DataSourceStatus { get; }
    string CurrentTenantId { get; set; }
    CompanyProfile? CurrentTenant { get; }
    string CurrentUserName { get; }
    string CurrentUserRole { get; }
    bool CanManageEmployees { get; }
    bool CanManageAttendance { get; }
    bool CanProcessPayroll { get; }
    bool CanApproveLeave { get; }
    bool CanApproveExpenses { get; }
    bool CanManageSetup { get; }
    IReadOnlyList<CompanyProfile> Companies { get; }
    IReadOnlyList<RoleProfile> Roles { get; }
    IReadOnlyList<PlatformModule> Modules { get; }
    IReadOnlyList<EmployeeProfile> Employees { get; }
    IReadOnlyList<EmployeeDocument> GetEmployeeDocuments(string employeeId);
    IReadOnlyList<AttendanceLog> AttendanceLogs { get; }
    IReadOnlyList<LeaveRequest> LeaveRequests { get; }
    IReadOnlyList<ExpenseClaim> ExpenseClaims { get; }
    IReadOnlyList<CandidatePipelineItem> Candidates { get; }
    IReadOnlyList<PayrollRun> PayrollHistory { get; }
    IReadOnlyList<HelpdeskTicket> Tickets { get; }
    IReadOnlyList<IntegrationStatus> Integrations { get; }
    Dictionary<string, bool> FeatureFlags { get; }
    IReadOnlyList<AuditEvent> AuditEvents { get; }

    DashboardSnapshot GetSnapshot(string role);
    EmployeeProfile AddEmployee(
        string name,
        string role,
        string department,
        string location,
        string manager,
        string employmentType,
        string grade,
        decimal monthlyCtc,
        int leaveBalance,
        double utilization,
        string[] skills);
    EmployeeProfile? UpdateEmployee(EmployeeProfile employee);
    EmployeeDocument AddEmployeeDocument(string employeeId, string documentName, string fileName, string contentType, byte[] content);
    EmployeeDocumentContent? GetEmployeeDocumentContent(string employeeId, Guid documentId);
    AttendanceLog? CheckIn(string employeeId, string method, string location);
    void CheckOut(Guid attendanceId);
    LeaveRequest? ApplyLeave(string employeeId, string leaveType, DateOnly fromDate, DateOnly toDate, string reason);
    ExpenseClaim SubmitExpense(string employeeName, string category, decimal amount, string notes);
    void ApproveLeave(Guid id, string approverRole);
    void RejectLeave(Guid id, string approverRole);
    void ApproveExpense(Guid id, string approverRole);
    PayrollRun ProcessPayroll();
    void ReleaseLatestPayroll();
    void ToggleFeatureFlag(string flag);
    string AskAssistant(string prompt);
    IReadOnlyList<AiInsight> GetInsights(string role);
    IReadOnlyList<string> GetWorkflowSteps(string module);
}
