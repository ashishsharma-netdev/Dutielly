using Dutielly.Shared.Data;

namespace Dutielly.Shared.Services;

public class DutiellyPlatformService : IDutiellyPlatformService
{
    private readonly List<CompanyProfile> _companies = [];
    private readonly List<EmployeeProfile> _employees = [];
    private readonly List<EmployeeDocumentContent> _employeeDocuments = [];
    private readonly List<AttendanceLog> _attendanceLogs = [];
    private readonly List<LeaveRequest> _leaveRequests = [];
    private readonly List<ExpenseClaim> _expenseClaims = [];
    private readonly List<CandidatePipelineItem> _candidates = [];
    private readonly List<PayrollRun> _payrollHistory = [];
    private readonly List<HelpdeskTicket> _tickets = [];
    private readonly List<AuditEvent> _auditEvents = [];

    public DutiellyPlatformService()
    {
        FeatureFlags = DutiellyCatalog.CreateDefaultFeatureFlags();
    }

    public virtual string DataSourceName => "Local empty workspace";
    public virtual string DataSourceStatus => "No sample records loaded. Web host can be connected to SQL Server.";
    public virtual string CurrentTenantId { get; set; } = string.Empty;
    public virtual CompanyProfile? CurrentTenant => Companies.FirstOrDefault(company => company.Id == CurrentTenantId)
        ?? Companies.FirstOrDefault();
    public virtual string CurrentUserName => "Local user";
    public virtual string CurrentUserRole => "Local workspace";
    public virtual bool CanManageEmployees => true;
    public virtual bool CanManageAttendance => true;
    public virtual bool CanProcessPayroll => true;
    public virtual bool CanApproveLeave => true;
    public virtual bool CanApproveExpenses => true;
    public virtual bool CanManageSetup => true;
    public virtual IReadOnlyList<CompanyProfile> Companies => _companies;
    public virtual IReadOnlyList<RoleProfile> Roles => DutiellyCatalog.Roles;
    public virtual IReadOnlyList<PlatformModule> Modules => DutiellyCatalog.Modules;
    public virtual IReadOnlyList<EmployeeProfile> Employees => _employees;
    public virtual IReadOnlyList<EmployeeDocument> GetEmployeeDocuments(string employeeId)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
        {
            return [];
        }

        return _employeeDocuments
            .Where(item => item.Metadata.EmployeeId == employeeId)
            .OrderByDescending(item => item.Metadata.UploadedAt)
            .Select(item => CloneDocument(item.Metadata))
            .ToList();
    }

    public virtual IReadOnlyList<AttendanceLog> AttendanceLogs => _attendanceLogs;
    public virtual IReadOnlyList<LeaveRequest> LeaveRequests => _leaveRequests;
    public virtual IReadOnlyList<ExpenseClaim> ExpenseClaims => _expenseClaims;
    public virtual IReadOnlyList<CandidatePipelineItem> Candidates => _candidates;
    public virtual IReadOnlyList<PayrollRun> PayrollHistory => _payrollHistory;
    public virtual IReadOnlyList<HelpdeskTicket> Tickets => _tickets;
    public virtual IReadOnlyList<IntegrationStatus> Integrations => DutiellyCatalog.Integrations;
    public virtual Dictionary<string, bool> FeatureFlags { get; }
    public virtual IReadOnlyList<AuditEvent> AuditEvents => _auditEvents;

    public virtual DashboardSnapshot GetSnapshot(string role)
    {
        var activeEmployees = Employees.Count(employee => employee.Status == "Active");
        var present = AttendanceLogs.Count(log => log.Date == DateOnly.FromDateTime(DateTime.Today));
        var attendanceRate = activeEmployees == 0 ? 0 : Math.Round((double)present / activeEmployees * 100, 1);
        var pendingApprovals = LeaveRequests.Count(IsPending) + ExpenseClaims.Count(IsPending);
        var payroll = PayrollHistory.FirstOrDefault() ?? EmptyPayrollRun();
        var hasBusinessData = Companies.Count > 0 || Employees.Count > 0 || PayrollHistory.Count > 0 || AttendanceLogs.Count > 0;

        return new DashboardSnapshot
        {
            ActiveEmployees = activeEmployees,
            AttendanceRate = attendanceRate,
            PendingApprovals = pendingApprovals,
            Payroll = payroll,
            Modules = Modules,
            Insights = GetInsights(role),
            Metrics =
            [
                new("Headcount", activeEmployees.ToString("N0"), "Awaiting SQL records", "neutral", "people"),
                new("Attendance", $"{attendanceRate:0.#}%", present == 0 ? "No logs today" : "Live today", "neutral", "clock"),
                new("Pending approvals", pendingApprovals.ToString("N0"), "Leave and claims", pendingApprovals > 0 ? "watch" : "neutral", "bell"),
                new("Payroll cost", ToCurrency(payroll.NetPay), payroll.Status, "neutral", "wallet"),
                new("Feature modules", Modules.Count.ToString("N0"), "Requirement coverage", "good", "layers"),
                new("Open tickets", Tickets.Count(t => t.Status != "Closed").ToString("N0"), "Helpdesk queue", "neutral", "ticket")
            ]
        };
    }

    public virtual EmployeeProfile AddEmployee(
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
        string[] skills)
    {
        var cleanName = RequireText(name, "Employee name");
        var cleanRole = RequireText(role, "Role");
        var cleanDepartment = RequireText(department, "Department");
        var cleanLocation = RequireText(location, "Location");
        var cleanManager = RequireText(manager, "Manager");
        var cleanEmploymentType = RequireText(employmentType, "Employment type");
        var cleanGrade = RequireText(grade, "Grade");
        ValidateEmployeeNumbers(monthlyCtc, leaveBalance, utilization);

        var employee = new EmployeeProfile
        {
            Id = GenerateEmployeeId(),
            Name = cleanName,
            Role = cleanRole,
            Department = cleanDepartment,
            Location = cleanLocation,
            Manager = cleanManager,
            EmploymentType = cleanEmploymentType,
            Grade = cleanGrade,
            JoiningDate = DateOnly.FromDateTime(DateTime.Today),
            Status = "Active",
            MonthlyCtc = monthlyCtc,
            LeaveBalance = leaveBalance,
            Utilization = utilization,
            Skills = skills ?? []
        };

        _employees.Insert(0, employee);
        AddAudit("HR Manager", $"Created employee {employee.Id}", "Employee Master");
        return employee;
    }

    public virtual EmployeeProfile? UpdateEmployee(EmployeeProfile employee)
    {
        ValidateEmployeeProfile(employee);

        var existing = _employees.FirstOrDefault(item => item.Id == employee.Id);
        if (existing is null)
        {
            return null;
        }

        existing.Name = employee.Name.Trim();
        existing.Role = employee.Role.Trim();
        existing.Department = employee.Department.Trim();
        existing.Location = employee.Location.Trim();
        existing.Manager = employee.Manager.Trim();
        existing.EmploymentType = employee.EmploymentType.Trim();
        existing.Grade = employee.Grade.Trim();
        existing.Status = employee.Status.Trim();
        existing.MonthlyCtc = employee.MonthlyCtc;
        existing.LeaveBalance = employee.LeaveBalance;
        existing.Utilization = employee.Utilization;
        existing.Skills = employee.Skills;
        AddAudit("HR Manager", $"Updated employee {existing.Id}", "Employee Master");
        return existing;
    }

    public virtual EmployeeDocument AddEmployeeDocument(string employeeId, string documentName, string fileName, string contentType, byte[] content)
    {
        var cleanEmployeeId = RequireText(employeeId, "Employee");
        var cleanDocumentName = RequireText(documentName, "Document name");
        var cleanFileName = RequireText(fileName, "File name");
        var cleanContentType = RequireText(contentType, "Content type");
        var cleanExtension = ValidateDocumentFile(cleanFileName, content);
        var employee = Employees.FirstOrDefault(item => item.Id == cleanEmployeeId)
            ?? throw new ArgumentException("Employee record was not found.");

        var metadata = new EmployeeDocument
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            DocumentName = cleanDocumentName,
            FileName = cleanFileName,
            FileExtension = cleanExtension,
            ContentType = NormalizeDocumentContentType(cleanExtension, cleanContentType),
            FileSizeBytes = content.LongLength,
            UploadedAt = DateTime.Now,
            UploadedBy = CurrentUserName
        };

        _employeeDocuments.Insert(0, new EmployeeDocumentContent
        {
            Metadata = metadata,
            Content = content.ToArray()
        });
        AddAudit(CurrentUserName, $"Uploaded {metadata.DocumentName} for {employee.Name}", "Employee Documents");
        return CloneDocument(metadata);
    }

    public virtual EmployeeDocumentContent? GetEmployeeDocumentContent(string employeeId, Guid documentId)
    {
        var document = _employeeDocuments.FirstOrDefault(item => item.Metadata.EmployeeId == employeeId && item.Metadata.Id == documentId);
        if (document is null)
        {
            return null;
        }

        return new EmployeeDocumentContent
        {
            Metadata = CloneDocument(document.Metadata),
            Content = document.Content.ToArray()
        };
    }

    public virtual AttendanceLog? CheckIn(string employeeId, string method, string location)
    {
        var cleanEmployeeId = RequireText(employeeId, "Employee");
        var cleanMethod = RequireText(method, "Attendance method");
        var cleanLocation = RequireText(location, "Location");

        var employee = Employees.FirstOrDefault(item => item.Id == cleanEmployeeId);
        if (employee is null)
        {
            return null;
        }

        var log = new AttendanceLog
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            CheckIn = TimeOnly.FromDateTime(DateTime.Now),
            Method = cleanMethod,
            Location = cleanLocation,
            SecuritySignal = "Tenant access verified and attendance audit saved"
        };

        _attendanceLogs.Insert(0, log);
        AddAudit(employee.Name, $"Checked in with {cleanMethod}", "Attendance");
        return log;
    }

    public virtual void CheckOut(Guid attendanceId)
    {
        var log = _attendanceLogs.FirstOrDefault(item => item.Id == attendanceId);
        if (log is null)
        {
            return;
        }

        log.CheckOut = TimeOnly.FromDateTime(DateTime.Now);
        AddAudit(log.EmployeeName, "Checked out", "Attendance");
    }

    public virtual LeaveRequest? ApplyLeave(string employeeId, string leaveType, DateOnly fromDate, DateOnly toDate, string reason)
    {
        var cleanEmployeeId = RequireText(employeeId, "Employee");
        var cleanLeaveType = RequireText(leaveType, "Leave type");
        var cleanReason = RequireText(reason, "Reason");
        ValidateDateRange(fromDate, toDate);

        var employee = Employees.FirstOrDefault(item => item.Id == cleanEmployeeId);
        if (employee is null)
        {
            return null;
        }

        var request = new LeaveRequest
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            LeaveType = cleanLeaveType,
            FromDate = fromDate,
            ToDate = toDate,
            Reason = cleanReason
        };

        _leaveRequests.Insert(0, request);
        AddAudit(employee.Name, $"Applied {cleanLeaveType} leave", "Leave");
        return request;
    }

    public virtual ExpenseClaim SubmitExpense(string employeeName, string category, decimal amount, string notes)
    {
        var cleanEmployeeName = RequireText(employeeName, "Employee");
        var cleanCategory = RequireText(category, "Expense category");
        var cleanNotes = RequireText(notes, "Expense notes");
        if (amount <= 0)
        {
            throw new ArgumentException("Expense amount must be greater than zero.", nameof(amount));
        }

        var claim = new ExpenseClaim
        {
            EmployeeName = cleanEmployeeName,
            Category = cleanCategory,
            Amount = amount,
            Notes = cleanNotes
        };

        _expenseClaims.Insert(0, claim);
        AddAudit(claim.EmployeeName, $"Submitted {cleanCategory} claim", "Expenses");
        return claim;
    }

    public virtual void ApproveLeave(Guid id, string approverRole)
    {
        var request = _leaveRequests.FirstOrDefault(item => item.Id == id);
        if (request is null)
        {
            return;
        }

        if (approverRole == "HR")
        {
            request.HrStatus = ApprovalStatus.Approved;
        }
        else
        {
            request.ManagerStatus = ApprovalStatus.Approved;
        }

        AddAudit(approverRole, $"Approved leave for {request.EmployeeName}", "Leave");
    }

    public virtual void RejectLeave(Guid id, string approverRole)
    {
        var request = _leaveRequests.FirstOrDefault(item => item.Id == id);
        if (request is null)
        {
            return;
        }

        if (approverRole == "HR")
        {
            request.HrStatus = ApprovalStatus.Rejected;
        }
        else
        {
            request.ManagerStatus = ApprovalStatus.Rejected;
        }

        AddAudit(approverRole, $"Rejected leave for {request.EmployeeName}", "Leave");
    }

    public virtual void ApproveExpense(Guid id, string approverRole)
    {
        var claim = _expenseClaims.FirstOrDefault(item => item.Id == id);
        if (claim is null)
        {
            return;
        }

        if (approverRole == "Finance")
        {
            claim.FinanceStatus = ApprovalStatus.Approved;
        }
        else
        {
            claim.ManagerStatus = ApprovalStatus.Approved;
        }

        AddAudit(approverRole, $"Approved {claim.Category} claim", "Expenses");
    }

    public virtual PayrollRun ProcessPayroll()
    {
        if (Employees.Count == 0)
        {
            throw new InvalidOperationException("Add employees with salary details before processing payroll.");
        }

        var current = CreatePayrollRun(DateTime.Now.ToString("MMMM yyyy"), "Locked");
        _payrollHistory.Insert(0, current);
        AddAudit("Payroll Manager", $"Processed payroll for {current.Month}", "Payroll");
        return current;
    }

    public virtual void ReleaseLatestPayroll()
    {
        var payroll = _payrollHistory.FirstOrDefault();
        if (payroll is null)
        {
            return;
        }

        payroll.Status = "Released";
        AddAudit("Payroll Manager", $"Released payroll for {payroll.Month}", "Payroll");
    }

    public virtual void ToggleFeatureFlag(string flag)
    {
        if (!FeatureFlags.ContainsKey(flag))
        {
            return;
        }

        FeatureFlags[flag] = !FeatureFlags[flag];
        AddAudit("Super Admin", $"Toggled {flag} to {(FeatureFlags[flag] ? "on" : "off")}", "Admin Center");
    }

    public virtual string AskAssistant(string prompt)
    {
        var normalized = (prompt ?? string.Empty).Trim().ToLowerInvariant();
        AddAudit("AI Assistant", $"Answered prompt: {prompt}", "AI Assistant");

        if (Employees.Count == 0 && PayrollHistory.Count == 0 && AttendanceLogs.Count == 0)
        {
            return "No live HR, attendance, or payroll records are available yet. Connect SQL Server and create real company and employee records to enable data-aware answers.";
        }

        if (normalized.Contains("leave"))
        {
            return $"{LeaveRequests.Count} leave requests are currently stored, with {LeaveRequests.Count(IsPending)} waiting for action.";
        }

        if (normalized.Contains("salary") || normalized.Contains("pay"))
        {
            var payroll = PayrollHistory.FirstOrDefault() ?? EmptyPayrollRun();
            return $"The latest payroll status is {payroll.Status}. Net payable is {ToCurrency(payroll.NetPay)}.";
        }

        if (normalized.Contains("attendance") || normalized.Contains("late"))
        {
            return $"Today's attendance is {GetSnapshot("HR Manager").AttendanceRate:0.#}% from {AttendanceLogs.Count} captured logs.";
        }

        if (normalized.Contains("hire") || normalized.Contains("candidate"))
        {
            return $"{Candidates.Count} candidates are currently in the recruitment pipeline.";
        }

        return "Dutielly can answer attendance, salary, leave, recruitment, payroll, and policy questions once SQL Server data is available.";
    }

    public virtual IReadOnlyList<AiInsight> GetInsights(string role)
    {
        var hasBusinessData = Companies.Count > 0 || Employees.Count > 0 || PayrollHistory.Count > 0 || AttendanceLogs.Count > 0;
        return DutiellyCatalog.GetInsights(role, hasBusinessData);
    }

    public virtual IReadOnlyList<string> GetWorkflowSteps(string module)
    {
        return DutiellyCatalog.GetWorkflowSteps(module);
    }

    public static string ToCurrency(decimal value)
    {
        return $"Rs {value:N0}";
    }

    protected virtual void AddAudit(string actor, string action, string module)
    {
        _auditEvents.Insert(0, new AuditEvent(DateTime.Now, actor, action, module));
    }

    protected static bool IsPending(LeaveRequest request)
    {
        return request.ManagerStatus == ApprovalStatus.Pending || request.HrStatus == ApprovalStatus.Pending;
    }

    protected static bool IsPending(ExpenseClaim claim)
    {
        return claim.ManagerStatus == ApprovalStatus.Pending || claim.FinanceStatus == ApprovalStatus.Pending;
    }

    protected static PayrollRun EmptyPayrollRun()
    {
        return new PayrollRun
        {
            Month = DateTime.Now.ToString("MMMM yyyy"),
            Employees = 0,
            GrossPay = 0,
            Deductions = 0,
            NetPay = 0,
            Status = "Not started",
            UpdatedAt = DateTime.Now
        };
    }

    protected static string RequireText(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{fieldName} is required.");
        }

        return value.Trim();
    }

    protected static void ValidateEmployeeProfile(EmployeeProfile employee)
    {
        RequireText(employee.Id, "Employee ID");
        RequireText(employee.Name, "Employee name");
        RequireText(employee.Role, "Role");
        RequireText(employee.Department, "Department");
        RequireText(employee.Location, "Location");
        RequireText(employee.Manager, "Manager");
        RequireText(employee.EmploymentType, "Employment type");
        RequireText(employee.Grade, "Grade");
        RequireText(employee.Status, "Status");

        ValidateEmployeeNumbers(employee.MonthlyCtc, employee.LeaveBalance, employee.Utilization);
    }

    protected static void ValidateDateRange(DateOnly fromDate, DateOnly toDate)
    {
        if (toDate < fromDate)
        {
            throw new ArgumentException("To date cannot be earlier than from date.");
        }
    }

    protected static void ValidateEmployeeNumbers(decimal monthlyCtc, int leaveBalance, double utilization)
    {
        if (monthlyCtc <= 0)
        {
            throw new ArgumentException("Monthly CTC must be greater than zero.");
        }

        if (leaveBalance < 0)
        {
            throw new ArgumentException("Leave balance cannot be negative.");
        }

        if (utilization is < 0 or > 100)
        {
            throw new ArgumentException("Utilization must be between 0 and 100.");
        }
    }

    protected static string ValidateDocumentFile(string fileName, byte[] content)
    {
        if (content.Length == 0)
        {
            throw new ArgumentException("Selected document file is empty.");
        }

        var extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        if (extension is not ("jpg" or "jpeg" or "png" or "pdf"))
        {
            throw new ArgumentException("Only JPG, JPEG, PNG, and PDF files are allowed.");
        }

        return extension;
    }

    protected static string NormalizeDocumentContentType(string extension, string contentType)
    {
        return extension switch
        {
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "pdf" => "application/pdf",
            _ => contentType
        };
    }

    protected static EmployeeDocument CloneDocument(EmployeeDocument document)
    {
        return new EmployeeDocument
        {
            Id = document.Id,
            EmployeeId = document.EmployeeId,
            EmployeeName = document.EmployeeName,
            DocumentName = document.DocumentName,
            FileName = document.FileName,
            FileExtension = document.FileExtension,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            UploadedAt = document.UploadedAt,
            UploadedBy = document.UploadedBy
        };
    }

    private PayrollRun CreatePayrollRun(string month, string status)
    {
        var gross = Employees.Sum(employee => employee.MonthlyCtc);
        var deductions = Math.Round(gross * 0.145m);
        var net = gross - deductions;

        return new PayrollRun
        {
            Month = month,
            Employees = Employees.Count,
            GrossPay = gross,
            Deductions = deductions,
            NetPay = net,
            Status = status,
            UpdatedAt = DateTime.Now
        };
    }

    private string GenerateEmployeeId()
    {
        var next = Employees
            .Select(employee => employee.Id.Replace("DUT-", string.Empty, StringComparison.OrdinalIgnoreCase))
            .Select(value => int.TryParse(value, out var number) ? number : 0)
            .DefaultIfEmpty(1000)
            .Max() + 1;

        return $"DUT-{next}";
    }
}
