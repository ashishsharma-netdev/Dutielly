using Microsoft.EntityFrameworkCore;

namespace Dutielly.Web.Data;

public sealed class DutiellyDbContext(DbContextOptions<DutiellyDbContext> options) : DbContext(options)
{
    public DbSet<CompanyEntity> Companies => Set<CompanyEntity>();
    public DbSet<EmployeeEntity> Employees => Set<EmployeeEntity>();
    public DbSet<EmployeeDocumentEntity> EmployeeDocuments => Set<EmployeeDocumentEntity>();
    public DbSet<AttendanceLogEntity> AttendanceLogs => Set<AttendanceLogEntity>();
    public DbSet<LeaveRequestEntity> LeaveRequests => Set<LeaveRequestEntity>();
    public DbSet<ExpenseClaimEntity> ExpenseClaims => Set<ExpenseClaimEntity>();
    public DbSet<CandidateEntity> Candidates => Set<CandidateEntity>();
    public DbSet<PayrollRunEntity> PayrollRuns => Set<PayrollRunEntity>();
    public DbSet<HelpdeskTicketEntity> HelpdeskTickets => Set<HelpdeskTicketEntity>();
    public DbSet<AuditEventEntity> AuditEvents => Set<AuditEventEntity>();
    public DbSet<FeatureFlagEntity> FeatureFlags => Set<FeatureFlagEntity>();
    public DbSet<AppUserEntity> AppUsers => Set<AppUserEntity>();
    public DbSet<AppUserTenantEntity> AppUserTenants => Set<AppUserTenantEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompanyEntity>(entity =>
        {
            entity.ToTable("Companies");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasMaxLength(64);
            entity.Property(item => item.Name).HasMaxLength(180).IsRequired();
            entity.Property(item => item.Industry).HasMaxLength(120);
            entity.Property(item => item.City).HasMaxLength(120);
            entity.Property(item => item.BrandColor).HasMaxLength(20);
        });

        modelBuilder.Entity<EmployeeEntity>(entity =>
        {
            entity.ToTable("Employees");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(item => item.Id).HasMaxLength(64);
            entity.Property(item => item.Name).HasMaxLength(180).IsRequired();
            entity.Property(item => item.Role).HasMaxLength(140);
            entity.Property(item => item.Department).HasMaxLength(140);
            entity.Property(item => item.Location).HasMaxLength(160);
            entity.Property(item => item.Manager).HasMaxLength(160);
            entity.Property(item => item.EmploymentType).HasMaxLength(80);
            entity.Property(item => item.Grade).HasMaxLength(80);
            entity.Property(item => item.Status).HasMaxLength(80);
            entity.Property(item => item.MonthlyCtc).HasColumnType("decimal(18,2)");
            entity.Property(item => item.SkillsCsv).HasMaxLength(1000);
            entity.HasIndex(item => item.TenantId);
        });

        modelBuilder.Entity<EmployeeDocumentEntity>(entity =>
        {
            entity.ToTable("EmployeeDocuments");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(item => item.EmployeeId).HasMaxLength(64).IsRequired();
            entity.Property(item => item.EmployeeName).HasMaxLength(180).IsRequired();
            entity.Property(item => item.DocumentName).HasMaxLength(180).IsRequired();
            entity.Property(item => item.FileName).HasMaxLength(260).IsRequired();
            entity.Property(item => item.FileExtension).HasMaxLength(20).IsRequired();
            entity.Property(item => item.ContentType).HasMaxLength(120).IsRequired();
            entity.Property(item => item.FileContent).IsRequired();
            entity.Property(item => item.UploadedBy).HasMaxLength(180);
            entity.HasIndex(item => new { item.TenantId, item.EmployeeId });
        });

        modelBuilder.Entity<AttendanceLogEntity>(entity =>
        {
            entity.ToTable("AttendanceLogs");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(item => item.EmployeeId).HasMaxLength(64);
            entity.Property(item => item.EmployeeName).HasMaxLength(180);
            entity.Property(item => item.Method).HasMaxLength(120);
            entity.Property(item => item.Location).HasMaxLength(180);
            entity.Property(item => item.SecuritySignal).HasMaxLength(300);
            entity.Property(item => item.Status).HasMaxLength(40);
            entity.HasIndex(item => item.TenantId);
        });

        modelBuilder.Entity<LeaveRequestEntity>(entity =>
        {
            entity.ToTable("LeaveRequests");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(item => item.EmployeeId).HasMaxLength(64);
            entity.Property(item => item.EmployeeName).HasMaxLength(180);
            entity.Property(item => item.LeaveType).HasMaxLength(80);
            entity.Property(item => item.Reason).HasMaxLength(500);
            entity.Property(item => item.ManagerStatus).HasMaxLength(40);
            entity.Property(item => item.HrStatus).HasMaxLength(40);
            entity.HasIndex(item => item.TenantId);
        });

        modelBuilder.Entity<ExpenseClaimEntity>(entity =>
        {
            entity.ToTable("ExpenseClaims");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(item => item.EmployeeName).HasMaxLength(180);
            entity.Property(item => item.Category).HasMaxLength(80);
            entity.Property(item => item.Amount).HasColumnType("decimal(18,2)");
            entity.Property(item => item.Notes).HasMaxLength(500);
            entity.Property(item => item.ManagerStatus).HasMaxLength(40);
            entity.Property(item => item.FinanceStatus).HasMaxLength(40);
            entity.HasIndex(item => item.TenantId);
        });

        modelBuilder.Entity<CandidateEntity>(entity =>
        {
            entity.ToTable("Candidates");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(item => item.CandidateName).HasMaxLength(180);
            entity.Property(item => item.Position).HasMaxLength(160);
            entity.Property(item => item.Stage).HasMaxLength(80);
            entity.Property(item => item.Owner).HasMaxLength(160);
            entity.HasIndex(item => item.TenantId);
        });

        modelBuilder.Entity<PayrollRunEntity>(entity =>
        {
            entity.ToTable("PayrollRuns");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(item => item.Month).HasMaxLength(80);
            entity.Property(item => item.GrossPay).HasColumnType("decimal(18,2)");
            entity.Property(item => item.Deductions).HasColumnType("decimal(18,2)");
            entity.Property(item => item.NetPay).HasColumnType("decimal(18,2)");
            entity.Property(item => item.Status).HasMaxLength(80);
            entity.HasIndex(item => item.TenantId);
        });

        modelBuilder.Entity<HelpdeskTicketEntity>(entity =>
        {
            entity.ToTable("HelpdeskTickets");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(item => item.Category).HasMaxLength(80);
            entity.Property(item => item.Subject).HasMaxLength(240);
            entity.Property(item => item.Owner).HasMaxLength(160);
            entity.Property(item => item.Priority).HasMaxLength(40);
            entity.Property(item => item.Status).HasMaxLength(60);
            entity.HasIndex(item => item.TenantId);
        });

        modelBuilder.Entity<AuditEventEntity>(entity =>
        {
            entity.ToTable("AuditEvents");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(item => item.Actor).HasMaxLength(180);
            entity.Property(item => item.Action).HasMaxLength(500);
            entity.Property(item => item.Module).HasMaxLength(120);
            entity.HasIndex(item => item.TenantId);
        });

        modelBuilder.Entity<FeatureFlagEntity>(entity =>
        {
            entity.ToTable("FeatureFlags");
            entity.HasKey(item => new { item.TenantId, item.Name });
            entity.Property(item => item.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(item => item.Name).HasMaxLength(160);
        });

        modelBuilder.Entity<AppUserEntity>(entity =>
        {
            entity.ToTable("AppUsers");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Email).HasMaxLength(256).IsRequired();
            entity.Property(item => item.DisplayName).HasMaxLength(180).IsRequired();
            entity.Property(item => item.Role).HasMaxLength(80).IsRequired();
            entity.Property(item => item.PasswordHash).HasMaxLength(500).IsRequired();
            entity.HasIndex(item => item.Email).IsUnique();
        });

        modelBuilder.Entity<AppUserTenantEntity>(entity =>
        {
            entity.ToTable("AppUserTenants");
            entity.HasKey(item => new { item.UserId, item.TenantId });
            entity.Property(item => item.TenantId).HasMaxLength(64).IsRequired();
            entity.HasIndex(item => item.TenantId);
        });
    }
}

public sealed class CompanyEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string BrandColor { get; set; } = "#0f8f7a";
    public int Branches { get; set; }
    public int Locations { get; set; }
}

public sealed class EmployeeEntity
{
    public string TenantId { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Manager { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public DateOnly JoiningDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public string Status { get; set; } = "Active";
    public decimal MonthlyCtc { get; set; }
    public int LeaveBalance { get; set; }
    public double Utilization { get; set; }
    public string SkillsCsv { get; set; } = string.Empty;
}

public sealed class EmployeeDocumentEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public byte[] FileContent { get; set; } = [];
    public DateTime UploadedAt { get; set; } = DateTime.Now;
    public string UploadedBy { get; set; } = string.Empty;
}

public sealed class AttendanceLogEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public TimeOnly CheckIn { get; set; }
    public TimeOnly? CheckOut { get; set; }
    public string Method { get; set; } = "Web Check-In";
    public string Location { get; set; } = string.Empty;
    public string SecuritySignal { get; set; } = string.Empty;
    public string Status { get; set; } = "Approved";
}

public sealed class LeaveRequestEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string LeaveType { get; set; } = "Casual";
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string ManagerStatus { get; set; } = "Pending";
    public string HrStatus { get; set; } = "Pending";
}

public sealed class ExpenseClaimEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string ManagerStatus { get; set; } = "Pending";
    public string FinanceStatus { get; set; } = "Pending";
}

public sealed class CandidateEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Stage { get; set; } = "Applied";
    public int AiScore { get; set; }
    public string Owner { get; set; } = string.Empty;
}

public sealed class PayrollRunEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public int Employees { get; set; }
    public decimal GrossPay { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetPay { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

public sealed class HelpdeskTicketEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Open";
}

public sealed class AuditEventEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;
    public DateTime At { get; set; } = DateTime.Now;
    public string Actor { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}

public sealed class FeatureFlagEntity
{
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}

public sealed class AppUserEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}

public sealed class AppUserTenantEntity
{
    public Guid UserId { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
