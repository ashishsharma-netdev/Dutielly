using Dutielly.Shared.Data;

namespace Dutielly.Shared.Services;

public static class DutiellyCatalog
{
    public static IReadOnlyList<RoleProfile> Roles { get; } =
    [
        new("Super Admin", "Complete platform ownership across tenants, plans, roles, feature controls, and audit.", ["Tenant control", "Feature flags", "RBAC", "Audit logs"]),
        new("Company Admin", "Company setup, locations, branding, policies, calendar, and hierarchy.", ["Company profile", "Branches", "Policy setup", "Org chart"]),
        new("HR Manager", "Employee lifecycle from hiring to onboarding, performance, learning, and exit.", ["Employees", "Recruitment", "Onboarding", "Performance"]),
        new("Payroll Manager", "Salary structures, payroll preview, payroll lock, release status, and payroll reports.", ["Payroll", "Salary review", "Deductions", "Reports"]),
        new("Department Manager", "Team attendance, leave, expense, overtime, shift swaps, and productivity approvals.", ["Team view", "Approvals", "Timesheets", "Shift swaps"]),
        new("Employee", "Self-service for attendance, leave, claims, profile updates, requests, and helpdesk.", ["Check-in", "Apply leave", "Claims", "Tickets"]),
        new("Finance Team", "Expense approval, reimbursement review, payroll status review, and finance reports.", ["Claims", "Payroll review", "Reports", "Approvals"]),
        new("Recruiter", "Job requisitions, candidate flow, interview feedback, and offers.", ["Jobs", "Candidates", "Interviews", "Offers"])
    ];

    public static IReadOnlyList<PlatformModule> Modules { get; } =
    [
        new("Organization", "HRMS", "Company, branch, department, team, designation, cost center, hierarchy, reporting structure, calendar, policies, and branding.", "building", ["Company", "Branches", "Departments", "Org chart"]),
        new("Multi-Company", "Enterprise", "Multiple companies and branches with tenant-separated employees, attendance, payroll, approvals, feature flags, and audit records.", "layers", ["Tenants", "Branches", "Separate data", "Controls"]),
        new("Employee Master", "HRMS", "Employee ID generation, profile, personal/contact/emergency info, education, experience, skills, joining details, salary inputs, and status.", "people", ["Auto ID", "Profile", "Skills", "Status"]),
        new("Recruitment Pipeline", "HRMS", "Jobs, vacancies, candidate database, interview scheduling, feedback, evaluation score, offers, and hiring stages.", "briefcase", ["Jobs", "Candidates", "Interviews", "Offers"]),
        new("Onboarding Checklist", "HRMS", "Welcome kit, employee registration, HR/IT/manager checklists, asset allocation, orientation, team, and shift assignment.", "spark", ["Welcome kit", "Checklists", "Assets", "Orientation"]),
        new("Attendance", "Workforce", "Web, mobile, office kiosk, and manager-entered attendance with check-in/out, corrections, approvals, and reports.", "clock", ["Check-in", "Breaks", "Approvals", "Reports"]),
        new("Branch And Site Visibility", "Workforce", "Branch, office, and client worksite status views for managers using tenant attendance records.", "map", ["Sites", "Branches", "Visits", "Summary"]),
        new("Shift Management", "Workforce", "General, morning, evening, night, flexible, rotational shifts, assignment, change requests, swaps, and overtime review.", "calendar", ["Shift setup", "Rotation", "Swap", "Overtime"]),
        new("Break Management", "Workforce", "Lunch, tea, and personal break start/end tracking, total break time, excess break review, and report impact.", "clock", ["Break start", "Break end", "Total time", "Review"]),
        new("Leave", "Workforce", "Casual, sick, earned, maternity, paternity, comp-off, LOP, leave application, multi-level approval, balances, and carry forward.", "leaf", ["Leave types", "Approval", "Balance", "Carry forward"]),
        new("Regularization", "Workforce", "Missed check-in/out, wrong attendance, supporting notes, employee-manager-HR workflow, and audit trail.", "shield", ["Missed punch", "Reason", "Workflow", "Audit"]),
        new("WFH Notes", "Workforce", "Remote check-in/out notes, task submission, work logs, productivity notes, and manager visibility.", "home", ["Remote attendance", "Tasks", "Review", "Logs"]),
        new("Field Notes", "Workforce", "Sales, marketing, delivery, and service team visit logs, worksite notes, and field attendance summaries.", "map", ["Visits", "Notes", "Field teams", "Summary"]),
        new("Overtime", "Workforce", "Hourly, daily, holiday, fixed-rate, and shift-based overtime review with manager and HR approval.", "clock", ["OT rules", "Review", "Manager approval", "HR approval"]),
        new("Timesheet", "Productivity", "Daily, weekly, and monthly project/task hours, utilization, billing-ready project logs, and timesheet reports.", "chart", ["Project", "Task", "Hours", "Utilization"]),
        new("Daily Reports", "Productivity", "Tasks completed, client meetings, site visits, remarks, attachments, and manager review.", "note", ["Tasks", "Meetings", "Attachments", "Review"]),
        new("Holiday", "Workforce", "Public, company, and regional holidays with monthly/yearly calendars and location-aware holiday rules.", "calendar", ["Public", "Company", "Regional", "Calendar"]),
        new("Visitor Attendance", "Workforce", "Visitor registration, check-in, check-out, host mapping, and visitor attendance reports.", "badge", ["Register", "Check-in", "Check-out", "Host"]),
        new("Payroll", "Payroll", "Monthly payroll generation, preview, employee count, gross pay, deduction estimate, net pay, lock, release, and history.", "wallet", ["Generate", "Preview", "Lock", "Release"]),
        new("Salary Review", "Payroll", "Salary structures, CTC, earnings, deductions, templates, fixed, variable, and percentage components.", "formula", ["CTC", "Components", "Templates", "Formula"]),
        new("Attendance Payroll Input", "Payroll", "Working days, present/absent days, paid/unpaid leaves, half days, late penalties, and payroll inputs.", "clock", ["Working days", "Leaves", "Late", "Payroll input"]),
        new("Expenses", "Finance", "Travel, food, fuel, accommodation, internet, mobile, medical reimbursements, claim submission, manager approval, and finance approval.", "card", ["Claims", "Receipts", "Manager", "Finance"]),
        new("Travel Requests", "Finance", "Travel requests, approval, travel budget, trip reimbursement, and policy checks.", "map", ["Request", "Approval", "Budget", "Reimbursement"]),
        new("Bonus And Incentive", "Payroll", "Performance, festival, joining, retention, referral bonus, incentives, commission, slab rules, approvals, and history.", "target", ["Bonus", "Incentive", "Commission", "History"]),
        new("Salary Revision", "Payroll", "Promotion changes, retro salary review, increment, revision history, effective date, and difference calculation.", "receipt", ["Retro", "Increment", "Revision", "History"]),
        new("Full And Final", "Payroll", "Final settlement review with pending salary, leave encashment, bonus, recovery, asset recovery, and exit status.", "receipt", ["FnF", "Recovery", "Encashment", "Exit"]),
        new("Performance", "Growth", "KPI setup, OKR management, goal assignment, self appraisal, manager appraisal, feedback, ratings, promotions, and increments.", "target", ["KPI", "OKR", "Feedback", "Ratings"]),
        new("Learning", "Growth", "Training calendar, courses, training materials, enrollment, assessments, certifications, and learning compliance.", "book", ["Courses", "Materials", "Assessment", "Certificate"]),
        new("ESS", "Portal", "Employee self-service for profile, attendance, leave, claims, requests, helpdesk, and reports.", "phone", ["Profile", "Leave", "Claims", "Requests"]),
        new("Manager Portal", "Portal", "Team dashboard, attendance, leave status, leave approval, attendance approval, expense approval, overtime approval, and performance review.", "team", ["Team", "Approvals", "Attendance", "Review"]),
        new("HR Portal", "Portal", "Employees, attendance, leave, shifts, payroll review, announcements, reports, recruitment, onboarding, and exits.", "badge", ["Employees", "Reports", "Announcements", "Lifecycle"]),
        new("Finance Portal", "Portal", "Expense review, payroll status review, reimbursement workflow, and finance reporting.", "money", ["Claims", "Payroll", "Reports", "Approvals"]),
        new("Asset Management", "Operations", "Laptop, mobile, SIM, ID card, vehicle issue, transfer, return, disposal, and asset approval.", "card", ["Assets", "Issue", "Transfer", "Return"]),
        new("Separation And Exit", "HRMS", "Resignation submission, notice period, exit interview, asset recovery, settlement review, and exit letters.", "door", ["Resignation", "Notice", "Interview", "Recovery"]),
        new("Helpdesk", "Experience", "Payroll, leave, HR, IT, attendance, and asset tickets with open, assigned, in-progress, resolved, closed, priority, SLA, and ownership.", "ticket", ["Tickets", "SLA", "Priority", "Status"]),
        new("Engagement", "Experience", "Satisfaction surveys, feedback surveys, employee of the month, reward points, appreciation wall, birthdays, anniversaries, and announcements.", "heart", ["Surveys", "Rewards", "Recognition", "Wishes"]),
        new("Announcements", "Experience", "HR announcements, circulars, policy updates, birthday wishes, work anniversary wishes, and employee notices.", "bell", ["Announcements", "Circulars", "Policies", "Wishes"]),
        new("Reports", "Analytics", "HR, attendance, payroll, recruitment, leave, department, late/early/absent/overtime, salary register, and export reports.", "analytics", ["HR", "Attendance", "Payroll", "Recruitment"]),
        new("Analytics", "Analytics", "Headcount, attrition, growth, attendance percentage, punctuality score, overtime trends, productivity, availability, payroll cost, and hiring pipeline.", "chart", ["Headcount", "Attendance", "Payroll", "Hiring"]),
        new("AI Assistant", "AI", "In-app assistant for answering questions using allowed HR, attendance, leave, recruitment, payroll, and policy records.", "ai", ["Attendance", "Salary", "Policy", "Leave"]),
        new("Security", "Security", "Cookie login, email/password authentication, password hashing, role-based access, tenant claims, protected APIs, audit logs, and activity monitoring.", "lock", ["Login", "RBAC", "Tenant access", "Audit"]),
        new("Enterprise", "Enterprise", "Multi-tenant architecture, tenant-aware APIs, feature controls, custom approval chains, workflow steps, audit trail, and release packaging.", "layers", ["Multi-tenant", "Workflow", "Feature flags", "Audit"])
    ];

    public static IReadOnlyList<IntegrationStatus> Integrations { get; } = [];

    public static Dictionary<string, bool> CreateDefaultFeatureFlags()
    {
        return new()
        {
            ["AI Insights"] = false,
            ["White Label Branding"] = false,
            ["Workflow Builder"] = false,
            ["Multi-Language"] = false
        };
    }

    public static IReadOnlyList<AiInsight> GetInsights(string role, bool hasBusinessData)
    {
        if (!hasBusinessData)
        {
            return
            [
                new("Database ready", "No business records are loaded yet. Connect SQL Server, create companies, and start entering employee data.", "neutral", "Configure ConnectionStrings:DutiellyDb and run database setup."),
                new("Current scope loaded", "The module catalog shows the active Dutielly scope for tenant setup, HR, attendance, payroll, approvals, ESS, reporting, and audit.", "good", "Use the modules as the implementation checklist.")
            ];
        }

        return
        [
            new("Payroll forecast", "Payroll forecast is calculated from live salary, attendance, and reimbursement records.", "watch", "Review salary templates before locking payroll."),
            new("Attendance signal", "Attendance exceptions are monitored from live attendance records.", "watch", "Review regularization requests and open attendance tickets."),
            new("Payroll review", "Payroll totals, deduction estimates, lock status, and release status are available after payroll processing.", "good", "Review payroll history before release."),
            new("Hiring velocity", "Recruitment pipeline health depends on open jobs, candidates, screening, and offer records.", "neutral", "Review open vacancies and candidate stages.")
        ];
    }

    public static IReadOnlyList<string> GetWorkflowSteps(string module)
    {
        return module switch
        {
            "Recruitment" => ["Job creation", "Candidate profile", "Screening", "Interview feedback", "Offer approval", "Hired"],
            "Onboarding" => ["Welcome kit", "HR checklist", "IT checklist", "Manager checklist", "Assets", "Orientation"],
            "Attendance" => ["Employee selected", "Method selected", "Location entered", "Timestamp saved", "Manager review", "Payroll input"],
            "Payroll" => ["Employee review", "Salary total", "Deduction estimate", "Payroll preview", "Approval", "Lock", "Release"],
            "Expense" => ["Claim", "Receipt note", "Manager approval", "Finance approval", "Reimbursement review", "Audit log"],
            "Exit" => ["Resignation", "Notice period", "Exit interview", "Asset recovery", "Settlement review", "Exit closure"],
            _ => ["Request", "Manager review", "HR or finance review", "Audit log", "Status update"]
        };
    }
}
