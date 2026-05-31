namespace Dutielly.Web.Services;

public static class AuthRoles
{
    public const string SuperAdmin = "Super Admin";
    public const string CompanyAdmin = "Company Admin";
    public const string HrManager = "HR Manager";
    public const string PayrollManager = "Payroll Manager";
    public const string DepartmentManager = "Department Manager";
    public const string Employee = "Employee";
    public const string FinanceTeam = "Finance Team";
    public const string Recruiter = "Recruiter";
    public const string ItAdmin = "IT Admin";

    public static readonly string[] AdminRoles = [SuperAdmin, CompanyAdmin];
    public static readonly string[] HrRoles = [SuperAdmin, CompanyAdmin, HrManager];
    public static readonly string[] PayrollRoles = [SuperAdmin, CompanyAdmin, PayrollManager, FinanceTeam];
    public static readonly string[] ManagerRoles = [SuperAdmin, CompanyAdmin, HrManager, DepartmentManager];
    public static readonly string[] FinanceRoles = [SuperAdmin, CompanyAdmin, FinanceTeam];
    public static readonly string[] EmployeeRoles = [SuperAdmin, CompanyAdmin, HrManager, DepartmentManager, Employee];

    public static bool IsInAnyRole(System.Security.Claims.ClaimsPrincipal user, params string[] roles)
    {
        return roles.Any(user.IsInRole);
    }
}
