using Dutielly.Shared.Data;
using Dutielly.Shared.Services;
using Dutielly.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Dutielly.Web.Services;

public sealed class SqlServerDutiellyPlatformService(DutiellyDbContext db, TenantContext tenantContext, UserAccessService userAccess) : DutiellyPlatformService
{
    public override string DataSourceName => "SQL Server";
    public override string DataSourceStatus => CanConnect()
        ? "Connected to DutiellyDb with tenant-separated SQL tables."
        : "SQL Server is configured but not reachable yet. Set ConnectionStrings:DutiellyDb for your SQL Server.";

    public override string CurrentTenantId
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(tenantContext.CurrentTenantId) && userAccess.CanAccessTenant(tenantContext.CurrentTenantId))
            {
                return tenantContext.CurrentTenantId;
            }

            var firstTenantId = userAccess.GetDefaultTenantId();

            tenantContext.CurrentTenantId = firstTenantId;
            return tenantContext.CurrentTenantId;
        }
        set => tenantContext.CurrentTenantId = userAccess.CanAccessTenant(value ?? string.Empty) ? value ?? string.Empty : userAccess.GetDefaultTenantId();
    }

    public override CompanyProfile? CurrentTenant => Companies.FirstOrDefault(company => company.Id == CurrentTenantId)
        ?? Companies.FirstOrDefault();
    public override string CurrentUserName => userAccess.DisplayName;
    public override string CurrentUserRole => userAccess.Role;
    public override bool CanManageEmployees => userAccess.CanManageEmployees();
    public override bool CanManageAttendance => userAccess.CanManageAttendance();
    public override bool CanProcessPayroll => userAccess.CanProcessPayroll();
    public override bool CanApproveLeave => userAccess.CanApproveLeave();
    public override bool CanApproveExpenses => userAccess.CanApproveExpenses();
    public override bool CanManageSetup => userAccess.CanManageSetup();

    public override IReadOnlyList<CompanyProfile> Companies => Safe(() =>
    {
        var tenantIds = userAccess.GetVisibleTenantIds();
        if (tenantIds.Count == 0)
        {
            return [];
        }

        return db.Companies
            .AsNoTracking()
            .Where(item => tenantIds.Contains(item.Id))
            .OrderBy(item => item.Name)
            .Select(item => new CompanyProfile(item.Id, item.Name, item.Industry, item.City, item.BrandColor, item.Branches, item.Locations))
            .ToList();
    }, []);

    public override IReadOnlyList<EmployeeProfile> Employees => Safe(() =>
    {
        var tenantId = CurrentTenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return [];
        }

        return db.Employees
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId)
            .OrderBy(item => item.Name)
            .AsEnumerable()
            .Select(item => ToEmployeeProfile(item))
            .ToList();
    }, []);

    public override IReadOnlyList<EmployeeDocument> GetEmployeeDocuments(string employeeId) => Safe(() =>
    {
        var tenantId = CurrentTenantId;
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(employeeId))
        {
            return [];
        }

        return db.EmployeeDocuments
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId && item.EmployeeId == employeeId)
            .OrderByDescending(item => item.UploadedAt)
            .Select(item => new EmployeeDocument
            {
                Id = item.Id,
                EmployeeId = item.EmployeeId,
                EmployeeName = item.EmployeeName,
                DocumentName = item.DocumentName,
                FileName = item.FileName,
                FileExtension = item.FileExtension,
                ContentType = item.ContentType,
                FileSizeBytes = item.FileSizeBytes,
                UploadedAt = item.UploadedAt,
                UploadedBy = item.UploadedBy
            })
            .ToList();
    }, []);

    public override IReadOnlyList<AttendanceLog> AttendanceLogs => Safe(() =>
    {
        var tenantId = CurrentTenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return [];
        }

        return db.AttendanceLogs
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId)
            .OrderByDescending(item => item.Date)
            .ThenByDescending(item => item.CheckIn)
            .AsEnumerable()
            .Select(item => ToAttendanceLog(item))
            .ToList();
    }, []);

    public override IReadOnlyList<LeaveRequest> LeaveRequests => Safe(() =>
    {
        var tenantId = CurrentTenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return [];
        }

        return db.LeaveRequests
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId)
            .OrderByDescending(item => item.FromDate)
            .AsEnumerable()
            .Select(item => ToLeaveRequest(item))
            .ToList();
    }, []);

    public override IReadOnlyList<ExpenseClaim> ExpenseClaims => Safe(() =>
    {
        var tenantId = CurrentTenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return [];
        }

        return db.ExpenseClaims
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId)
            .OrderByDescending(item => item.Id)
            .AsEnumerable()
            .Select(item => ToExpenseClaim(item))
            .ToList();
    }, []);

    public override IReadOnlyList<CandidatePipelineItem> Candidates => Safe(() =>
    {
        var tenantId = CurrentTenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return [];
        }

        return db.Candidates
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId)
            .OrderByDescending(item => item.AiScore)
            .Select(item => new CandidatePipelineItem
            {
                Id = item.Id,
                CandidateName = item.CandidateName,
                Position = item.Position,
                Stage = item.Stage,
                AiScore = item.AiScore,
                Owner = item.Owner
            })
            .ToList();
    }, []);

    public override IReadOnlyList<PayrollRun> PayrollHistory => Safe(() =>
    {
        var tenantId = CurrentTenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return [];
        }

        return db.PayrollRuns
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId)
            .OrderByDescending(item => item.UpdatedAt)
            .Select(item => new PayrollRun
            {
                Month = item.Month,
                Employees = item.Employees,
                GrossPay = item.GrossPay,
                Deductions = item.Deductions,
                NetPay = item.NetPay,
                Status = item.Status,
                UpdatedAt = item.UpdatedAt
            })
            .ToList();
    }, []);

    public override IReadOnlyList<HelpdeskTicket> Tickets => Safe(() =>
    {
        var tenantId = CurrentTenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return [];
        }

        return db.HelpdeskTickets
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId)
            .OrderBy(item => item.Status)
            .ThenByDescending(item => item.Priority)
            .Select(item => new HelpdeskTicket
            {
                Id = item.Id,
                Category = item.Category,
                Subject = item.Subject,
                Owner = item.Owner,
                Priority = item.Priority,
                Status = item.Status
            })
            .ToList();
    }, []);

    public override Dictionary<string, bool> FeatureFlags => Safe(() =>
    {
        var tenantId = CurrentTenantId;
        var flags = DutiellyCatalog.CreateDefaultFeatureFlags();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return flags;
        }

        foreach (var flag in db.FeatureFlags.AsNoTracking().Where(item => item.TenantId == tenantId))
        {
            if (flags.ContainsKey(flag.Name))
            {
                flags[flag.Name] = flag.IsEnabled;
            }
        }

        return flags;
    }, DutiellyCatalog.CreateDefaultFeatureFlags());

    public override IReadOnlyList<AuditEvent> AuditEvents => Safe(() =>
    {
        var tenantId = CurrentTenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return [];
        }

        return db.AuditEvents
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId)
            .OrderByDescending(item => item.At)
            .Take(100)
            .Select(item => new AuditEvent(item.At, item.Actor, item.Action, item.Module))
            .ToList();
    }, []);

    public override EmployeeProfile AddEmployee(
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
        if (!userAccess.CanManageEmployees())
        {
            throw new UnauthorizedAccessException("Your role cannot create employees.");
        }

        var cleanName = RequireText(name, "Employee name");
        var cleanRole = RequireText(role, "Role");
        var cleanDepartment = RequireText(department, "Department");
        var cleanLocation = RequireText(location, "Location");
        var cleanManager = RequireText(manager, "Manager");
        var cleanEmploymentType = RequireText(employmentType, "Employment type");
        var cleanGrade = RequireText(grade, "Grade");
        ValidateEmployeeNumbers(monthlyCtc, leaveBalance, utilization);
        var tenantId = GetWritableTenantId();
        var entity = new EmployeeEntity
        {
            TenantId = tenantId,
            Id = GenerateEmployeeId(tenantId),
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
            SkillsCsv = string.Join(',', (skills ?? []).Select(skill => skill.Trim()).Where(skill => !string.IsNullOrWhiteSpace(skill)).Distinct(StringComparer.OrdinalIgnoreCase))
        };

        Safe(() =>
        {
            db.Employees.Add(entity);
            AddAuditEntity("HR Manager", $"Created employee {entity.Id}", "Employee Master");
            db.SaveChanges();
            return true;
        }, false);

        return ToEmployeeProfile(entity);
    }

    public override AttendanceLog? CheckIn(string employeeId, string method, string location)
    {
        if (!userAccess.CanManageAttendance())
        {
            throw new UnauthorizedAccessException("Your role cannot capture attendance.");
        }

        var cleanEmployeeId = RequireText(employeeId, "Employee");
        var cleanMethod = RequireText(method, "Attendance method");
        var cleanLocation = RequireText(location, "Location");
        var tenantId = CurrentTenantId;
        var employee = Safe(() => db.Employees
            .AsNoTracking()
            .FirstOrDefault(item => item.TenantId == tenantId && item.Id == cleanEmployeeId), null);
        if (employee is null)
        {
            return null;
        }

        var entity = new AttendanceLogEntity
        {
            TenantId = tenantId,
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            Date = DateOnly.FromDateTime(DateTime.Today),
            CheckIn = TimeOnly.FromDateTime(DateTime.Now),
            Method = cleanMethod,
            Location = cleanLocation,
            SecuritySignal = "Tenant access verified and attendance audit saved",
            Status = "Approved"
        };

        Safe(() =>
        {
            db.AttendanceLogs.Add(entity);
            AddAuditEntity(employee.Name, $"Checked in with {cleanMethod}", "Attendance");
            db.SaveChanges();
            return true;
        }, false);

        return ToAttendanceLog(entity);
    }

    public override EmployeeProfile? UpdateEmployee(EmployeeProfile employee)
    {
        if (!userAccess.CanManageEmployees())
        {
            throw new UnauthorizedAccessException("Your role cannot update employees.");
        }

        ValidateEmployeeProfile(employee);
        var tenantId = CurrentTenantId;
        var entity = Safe(() => db.Employees.FirstOrDefault(item => item.TenantId == tenantId && item.Id == employee.Id), null);
        if (entity is null)
        {
            return null;
        }

        entity.Name = employee.Name.Trim();
        entity.Role = employee.Role.Trim();
        entity.Department = employee.Department.Trim();
        entity.Location = employee.Location.Trim();
        entity.Manager = employee.Manager.Trim();
        entity.EmploymentType = employee.EmploymentType.Trim();
        entity.Grade = employee.Grade.Trim();
        entity.Status = employee.Status.Trim();
        entity.MonthlyCtc = employee.MonthlyCtc;
        entity.LeaveBalance = employee.LeaveBalance;
        entity.Utilization = employee.Utilization;
        entity.SkillsCsv = string.Join(',', employee.Skills.Select(skill => skill.Trim()).Where(skill => !string.IsNullOrWhiteSpace(skill)).Distinct(StringComparer.OrdinalIgnoreCase));

        Safe(() =>
        {
            AddAuditEntity(userAccess.DisplayName, $"Updated employee {entity.Id}", "Employee Master");
            db.SaveChanges();
            return true;
        }, false);

        return ToEmployeeProfile(entity);
    }

    public override EmployeeDocument AddEmployeeDocument(string employeeId, string documentName, string fileName, string contentType, byte[] content)
    {
        if (!userAccess.CanManageEmployees())
        {
            throw new UnauthorizedAccessException("Your role cannot upload employee documents.");
        }

        var cleanEmployeeId = RequireText(employeeId, "Employee");
        var cleanDocumentName = RequireText(documentName, "Document name");
        var cleanFileName = RequireText(fileName, "File name");
        var cleanContentType = RequireText(contentType, "Content type");
        var cleanExtension = ValidateDocumentFile(cleanFileName, content);
        var tenantId = GetWritableTenantId();
        var employee = Safe(() => db.Employees
            .AsNoTracking()
            .FirstOrDefault(item => item.TenantId == tenantId && item.Id == cleanEmployeeId), null)
            ?? throw new ArgumentException("Employee record was not found.");

        var entity = new EmployeeDocumentEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            DocumentName = cleanDocumentName,
            FileName = cleanFileName,
            FileExtension = cleanExtension,
            ContentType = NormalizeDocumentContentType(cleanExtension, cleanContentType),
            FileSizeBytes = content.LongLength,
            FileContent = content.ToArray(),
            UploadedAt = DateTime.Now,
            UploadedBy = userAccess.DisplayName
        };

        Safe(() =>
        {
            db.EmployeeDocuments.Add(entity);
            AddAuditEntity(userAccess.DisplayName, $"Uploaded {entity.DocumentName} for {entity.EmployeeName}", "Employee Documents");
            db.SaveChanges();
            return true;
        }, false);

        return ToEmployeeDocument(entity);
    }

    public override EmployeeDocumentContent? GetEmployeeDocumentContent(string employeeId, Guid documentId)
    {
        var tenantId = CurrentTenantId;
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(employeeId))
        {
            return null;
        }

        var entity = Safe(() => db.EmployeeDocuments
            .AsNoTracking()
            .FirstOrDefault(item => item.TenantId == tenantId && item.EmployeeId == employeeId && item.Id == documentId), null);
        if (entity is null)
        {
            return null;
        }

        return new EmployeeDocumentContent
        {
            Metadata = ToEmployeeDocument(entity),
            Content = entity.FileContent
        };
    }

    public override void CheckOut(Guid attendanceId)
    {
        if (!userAccess.CanManageAttendance())
        {
            throw new UnauthorizedAccessException("Your role cannot update attendance.");
        }

        Safe(() =>
        {
            var tenantId = CurrentTenantId;
            var log = db.AttendanceLogs.FirstOrDefault(item => item.TenantId == tenantId && item.Id == attendanceId);
            if (log is null)
            {
                return false;
            }

            log.CheckOut = TimeOnly.FromDateTime(DateTime.Now);
            AddAuditEntity(log.EmployeeName, "Checked out", "Attendance");
            db.SaveChanges();
            return true;
        }, false);
    }

    public override LeaveRequest? ApplyLeave(string employeeId, string leaveType, DateOnly fromDate, DateOnly toDate, string reason)
    {
        if (!userAccess.CanManageAttendance())
        {
            throw new UnauthorizedAccessException("Your role cannot submit leave.");
        }

        var cleanEmployeeId = RequireText(employeeId, "Employee");
        var cleanLeaveType = RequireText(leaveType, "Leave type");
        var cleanReason = RequireText(reason, "Reason");
        ValidateDateRange(fromDate, toDate);
        var tenantId = CurrentTenantId;
        var employee = Safe(() => db.Employees
            .AsNoTracking()
            .FirstOrDefault(item => item.TenantId == tenantId && item.Id == cleanEmployeeId), null);
        if (employee is null)
        {
            return null;
        }

        var entity = new LeaveRequestEntity
        {
            TenantId = tenantId,
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            LeaveType = cleanLeaveType,
            FromDate = fromDate,
            ToDate = toDate,
            Reason = cleanReason
        };

        Safe(() =>
        {
            db.LeaveRequests.Add(entity);
            AddAuditEntity(employee.Name, $"Applied {cleanLeaveType} leave", "Leave");
            db.SaveChanges();
            return true;
        }, false);

        return ToLeaveRequest(entity);
    }

    public override ExpenseClaim SubmitExpense(string employeeName, string category, decimal amount, string notes)
    {
        if (!userAccess.CanManageAttendance())
        {
            throw new UnauthorizedAccessException("Your role cannot submit expenses.");
        }

        var cleanEmployeeName = RequireText(employeeName, "Employee");
        var cleanCategory = RequireText(category, "Expense category");
        var cleanNotes = RequireText(notes, "Expense notes");
        if (amount <= 0)
        {
            throw new ArgumentException("Expense amount must be greater than zero.", nameof(amount));
        }

        var tenantId = GetWritableTenantId();
        var entity = new ExpenseClaimEntity
        {
            TenantId = tenantId,
            EmployeeName = cleanEmployeeName,
            Category = cleanCategory,
            Amount = amount,
            Notes = cleanNotes
        };

        Safe(() =>
        {
            db.ExpenseClaims.Add(entity);
            AddAuditEntity(entity.EmployeeName, $"Submitted {cleanCategory} claim", "Expenses");
            db.SaveChanges();
            return true;
        }, false);

        return ToExpenseClaim(entity);
    }

    public override void ApproveLeave(Guid id, string approverRole)
    {
        if (!userAccess.CanApproveLeave())
        {
            throw new UnauthorizedAccessException("Your role cannot approve leave.");
        }

        Safe(() =>
        {
            var tenantId = CurrentTenantId;
            var request = db.LeaveRequests.FirstOrDefault(item => item.TenantId == tenantId && item.Id == id);
            if (request is null)
            {
                return false;
            }

            if (approverRole == "HR")
            {
                request.HrStatus = "Approved";
            }
            else
            {
                request.ManagerStatus = "Approved";
            }

            AddAuditEntity(approverRole, $"Approved leave for {request.EmployeeName}", "Leave");
            db.SaveChanges();
            return true;
        }, false);
    }

    public override void RejectLeave(Guid id, string approverRole)
    {
        if (!userAccess.CanApproveLeave())
        {
            throw new UnauthorizedAccessException("Your role cannot reject leave.");
        }

        Safe(() =>
        {
            var tenantId = CurrentTenantId;
            var request = db.LeaveRequests.FirstOrDefault(item => item.TenantId == tenantId && item.Id == id);
            if (request is null)
            {
                return false;
            }

            if (approverRole == "HR")
            {
                request.HrStatus = "Rejected";
            }
            else
            {
                request.ManagerStatus = "Rejected";
            }

            AddAuditEntity(approverRole, $"Rejected leave for {request.EmployeeName}", "Leave");
            db.SaveChanges();
            return true;
        }, false);
    }

    public override void ApproveExpense(Guid id, string approverRole)
    {
        if (!userAccess.CanApproveExpenses())
        {
            throw new UnauthorizedAccessException("Your role cannot approve expenses.");
        }

        Safe(() =>
        {
            var tenantId = CurrentTenantId;
            var claim = db.ExpenseClaims.FirstOrDefault(item => item.TenantId == tenantId && item.Id == id);
            if (claim is null)
            {
                return false;
            }

            if (approverRole == "Finance")
            {
                claim.FinanceStatus = "Approved";
            }
            else
            {
                claim.ManagerStatus = "Approved";
            }

            AddAuditEntity(approverRole, $"Approved {claim.Category} claim", "Expenses");
            db.SaveChanges();
            return true;
        }, false);
    }

    public override PayrollRun ProcessPayroll()
    {
        if (!userAccess.CanProcessPayroll())
        {
            throw new UnauthorizedAccessException("Your role cannot process payroll.");
        }

        var tenantId = GetWritableTenantId();
        var employees = Employees;
        if (employees.Count == 0)
        {
            throw new InvalidOperationException("Add employees with salary details before processing payroll.");
        }

        var gross = employees.Sum(employee => employee.MonthlyCtc);
        var deductions = Math.Round(gross * 0.145m);
        var entity = new PayrollRunEntity
        {
            TenantId = tenantId,
            Month = DateTime.Now.ToString("MMMM yyyy"),
            Employees = employees.Count,
            GrossPay = gross,
            Deductions = deductions,
            NetPay = gross - deductions,
            Status = employees.Count == 0 ? "No employees" : "Locked",
            UpdatedAt = DateTime.Now
        };

        Safe(() =>
        {
            db.PayrollRuns.Add(entity);
            AddAuditEntity("Payroll Manager", $"Processed payroll for {entity.Month}", "Payroll");
            db.SaveChanges();
            return true;
        }, false);

        return new PayrollRun
        {
            Month = entity.Month,
            Employees = entity.Employees,
            GrossPay = entity.GrossPay,
            Deductions = entity.Deductions,
            NetPay = entity.NetPay,
            Status = entity.Status,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public override void ReleaseLatestPayroll()
    {
        if (!userAccess.CanProcessPayroll())
        {
            throw new UnauthorizedAccessException("Your role cannot release payroll.");
        }

        Safe(() =>
        {
            var tenantId = CurrentTenantId;
            var payroll = db.PayrollRuns
                .Where(item => item.TenantId == tenantId)
                .OrderByDescending(item => item.UpdatedAt)
                .FirstOrDefault();
            if (payroll is null)
            {
                return false;
            }

            payroll.Status = "Released";
            AddAuditEntity("Payroll Manager", $"Released payroll for {payroll.Month}", "Payroll");
            db.SaveChanges();
            return true;
        }, false);
    }

    public override void ToggleFeatureFlag(string flag)
    {
        if (!userAccess.CanManageSetup())
        {
            throw new UnauthorizedAccessException("Your role cannot manage feature flags.");
        }

        Safe(() =>
        {
            var defaults = DutiellyCatalog.CreateDefaultFeatureFlags();
            if (!defaults.ContainsKey(flag))
            {
                return false;
            }

            var tenantId = GetWritableTenantId();
            var entity = db.FeatureFlags.FirstOrDefault(item => item.TenantId == tenantId && item.Name == flag);
            if (entity is null)
            {
                entity = new FeatureFlagEntity
                {
                    TenantId = tenantId,
                    Name = flag,
                    IsEnabled = !defaults.GetValueOrDefault(flag)
                };
                db.FeatureFlags.Add(entity);
            }
            else
            {
                entity.IsEnabled = !entity.IsEnabled;
            }

            AddAuditEntity("Super Admin", $"Toggled {flag} to {(entity.IsEnabled ? "on" : "off")}", "Admin Center");
            db.SaveChanges();
            return true;
        }, false);
    }

    private bool CanConnect()
    {
        return Safe(() => db.Database.CanConnect(), false);
    }

    private string GenerateEmployeeId(string tenantId)
    {
        var next = Safe(() => db.Employees
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId)
            .Select(item => item.Id)
            .AsEnumerable()
            .Select(id => id.Split('-', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty)
            .Select(value => int.TryParse(value, out var number) ? number : 0)
            .DefaultIfEmpty(1000)
            .Max() + 1, 1001);

        var prefix = tenantId.Split('-', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "DUT";
        prefix = new string(prefix.Where(char.IsLetterOrDigit).Take(5).ToArray()).ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(prefix))
        {
            prefix = "DUT";
        }

        return $"{prefix}-{next:0000}";
    }

    private void AddAuditEntity(string actor, string action, string module)
    {
        db.AuditEvents.Add(new AuditEventEntity
        {
            TenantId = GetWritableTenantId(),
            At = DateTime.Now,
            Actor = actor,
            Action = action,
            Module = module
        });
    }

    private string GetWritableTenantId()
    {
        var tenantId = CurrentTenantId;
        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            return tenantId;
        }

        return Safe(() =>
        {
            var existingTenantId = db.Companies
                .OrderBy(item => item.Name)
                .Select(item => item.Id)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(existingTenantId))
            {
                tenantContext.CurrentTenantId = existingTenantId;
                return existingTenantId;
            }

            var company = new CompanyEntity
            {
                Id = "tenant-dutiellyeasy",
                Name = "DutiellyEasy",
                Industry = "HR Technology",
                City = "Pune",
                BrandColor = "#0f8f7a",
                Branches = 1,
                Locations = 1
            };

            db.Companies.Add(company);
            db.SaveChanges();
            tenantContext.CurrentTenantId = company.Id;
            return company.Id;
        }, string.Empty);
    }

    private static EmployeeProfile ToEmployeeProfile(EmployeeEntity item)
    {
        return new EmployeeProfile
        {
            Id = item.Id,
            Name = item.Name,
            Role = item.Role,
            Department = item.Department,
            Location = item.Location,
            Manager = item.Manager,
            EmploymentType = item.EmploymentType,
            Grade = item.Grade,
            JoiningDate = item.JoiningDate,
            Status = item.Status,
            MonthlyCtc = item.MonthlyCtc,
            LeaveBalance = item.LeaveBalance,
            Utilization = item.Utilization,
            Skills = item.SkillsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        };
    }

    private static EmployeeDocument ToEmployeeDocument(EmployeeDocumentEntity item)
    {
        return new EmployeeDocument
        {
            Id = item.Id,
            EmployeeId = item.EmployeeId,
            EmployeeName = item.EmployeeName,
            DocumentName = item.DocumentName,
            FileName = item.FileName,
            FileExtension = item.FileExtension,
            ContentType = item.ContentType,
            FileSizeBytes = item.FileSizeBytes,
            UploadedAt = item.UploadedAt,
            UploadedBy = item.UploadedBy
        };
    }

    private static AttendanceLog ToAttendanceLog(AttendanceLogEntity item)
    {
        return new AttendanceLog
        {
            Id = item.Id,
            EmployeeId = item.EmployeeId,
            EmployeeName = item.EmployeeName,
            Date = item.Date,
            CheckIn = item.CheckIn,
            CheckOut = item.CheckOut,
            Method = item.Method,
            Location = item.Location,
            SecuritySignal = item.SecuritySignal,
            Status = ParseStatus(item.Status)
        };
    }

    private static LeaveRequest ToLeaveRequest(LeaveRequestEntity item)
    {
        return new LeaveRequest
        {
            Id = item.Id,
            EmployeeId = item.EmployeeId,
            EmployeeName = item.EmployeeName,
            LeaveType = item.LeaveType,
            FromDate = item.FromDate,
            ToDate = item.ToDate,
            Reason = item.Reason,
            ManagerStatus = ParseStatus(item.ManagerStatus),
            HrStatus = ParseStatus(item.HrStatus)
        };
    }

    private static ExpenseClaim ToExpenseClaim(ExpenseClaimEntity item)
    {
        return new ExpenseClaim
        {
            Id = item.Id,
            EmployeeName = item.EmployeeName,
            Category = item.Category,
            Amount = item.Amount,
            Notes = item.Notes,
            ManagerStatus = ParseStatus(item.ManagerStatus),
            FinanceStatus = ParseStatus(item.FinanceStatus)
        };
    }

    private static ApprovalStatus ParseStatus(string value)
    {
        return Enum.TryParse<ApprovalStatus>(value, true, out var status) ? status : ApprovalStatus.Pending;
    }

    private static T Safe<T>(Func<T> action, T fallback)
    {
        try
        {
            return action();
        }
        catch
        {
            return fallback;
        }
    }
}
