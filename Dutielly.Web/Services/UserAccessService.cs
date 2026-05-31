using System.Security.Claims;
using Dutielly.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Dutielly.Web.Services;

public sealed class UserAccessService(IHttpContextAccessor httpContextAccessor, DutiellyDbContext db)
{
    public ClaimsPrincipal User => httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());

    public bool IsAuthenticated => User.Identity?.IsAuthenticated == true;

    public string DisplayName => User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Signed-in user";

    public string Email => User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    public string Role => User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    public bool IsSuperAdmin => User.IsInRole(AuthRoles.SuperAdmin);

    public IReadOnlyList<string> TenantClaims => User.FindAll("tenant").Select(claim => claim.Value).Distinct().ToList();

    public bool CanAccessTenant(string tenantId)
    {
        if (!IsAuthenticated || string.IsNullOrWhiteSpace(tenantId))
        {
            return false;
        }

        return IsSuperAdmin || TenantClaims.Contains(tenantId, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<string> GetVisibleTenantIds()
    {
        if (!IsAuthenticated)
        {
            return [];
        }

        if (!IsSuperAdmin)
        {
            return TenantClaims;
        }

        return db.Companies
            .AsNoTracking()
            .OrderBy(company => company.Name)
            .Select(company => company.Id)
            .ToList();
    }

    public string GetDefaultTenantId()
    {
        return GetVisibleTenantIds().FirstOrDefault() ?? string.Empty;
    }

    public bool CanUsePath(PathString path)
    {
        if (!IsAuthenticated)
        {
            return false;
        }

        var value = path.Value?.TrimEnd('/').ToLowerInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(value) || value == "/")
        {
            return true;
        }

        if (value.StartsWith("/api/employees", StringComparison.OrdinalIgnoreCase))
        {
            return AuthRoles.IsInAnyRole(User, AuthRoles.HrRoles);
        }

        if (value.StartsWith("/api/payroll", StringComparison.OrdinalIgnoreCase) || value.StartsWith("/payroll", StringComparison.OrdinalIgnoreCase))
        {
            return AuthRoles.IsInAnyRole(User, AuthRoles.PayrollRoles);
        }

        if (value.StartsWith("/api/attendance", StringComparison.OrdinalIgnoreCase) || value.StartsWith("/attendance", StringComparison.OrdinalIgnoreCase))
        {
            return AuthRoles.IsInAnyRole(User, AuthRoles.EmployeeRoles);
        }

        if (value.StartsWith("/employees", StringComparison.OrdinalIgnoreCase))
        {
            return AuthRoles.IsInAnyRole(User, AuthRoles.HrRoles);
        }

        if (value.StartsWith("/approvals", StringComparison.OrdinalIgnoreCase))
        {
            return AuthRoles.IsInAnyRole(User, AuthRoles.SuperAdmin, AuthRoles.CompanyAdmin, AuthRoles.HrManager, AuthRoles.DepartmentManager, AuthRoles.PayrollManager, AuthRoles.FinanceTeam, AuthRoles.Recruiter);
        }

        if (value.StartsWith("/analytics", StringComparison.OrdinalIgnoreCase))
        {
            return AuthRoles.IsInAnyRole(User, AuthRoles.SuperAdmin, AuthRoles.CompanyAdmin, AuthRoles.HrManager, AuthRoles.DepartmentManager, AuthRoles.PayrollManager, AuthRoles.FinanceTeam);
        }

        if (value.StartsWith("/setup", StringComparison.OrdinalIgnoreCase))
        {
            return AuthRoles.IsInAnyRole(User, AuthRoles.AdminRoles);
        }

        if (value.StartsWith("/ess", StringComparison.OrdinalIgnoreCase) || value.StartsWith("/features", StringComparison.OrdinalIgnoreCase) || value.StartsWith("/api/modules", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return true;
    }

    public bool CanManageEmployees()
    {
        return AuthRoles.IsInAnyRole(User, AuthRoles.HrRoles);
    }

    public bool CanManageAttendance()
    {
        return AuthRoles.IsInAnyRole(User, AuthRoles.EmployeeRoles);
    }

    public bool CanApproveLeave()
    {
        return AuthRoles.IsInAnyRole(User, AuthRoles.SuperAdmin, AuthRoles.CompanyAdmin, AuthRoles.HrManager, AuthRoles.DepartmentManager);
    }

    public bool CanApproveExpenses()
    {
        return AuthRoles.IsInAnyRole(User, AuthRoles.SuperAdmin, AuthRoles.CompanyAdmin, AuthRoles.DepartmentManager, AuthRoles.FinanceTeam);
    }

    public bool CanProcessPayroll()
    {
        return AuthRoles.IsInAnyRole(User, AuthRoles.PayrollRoles);
    }

    public bool CanManageSetup()
    {
        return AuthRoles.IsInAnyRole(User, AuthRoles.AdminRoles);
    }
}
