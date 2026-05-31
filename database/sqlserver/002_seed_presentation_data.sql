USE [DutiellyDb];
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @TenantId nvarchar(64) = N'tenant-dutiellyeasy';

DELETE FROM dbo.AppUserTenants;

DELETE FROM dbo.AppUsers
WHERE Email <> N'admin@dutielly.local';

DELETE FROM dbo.FeatureFlags;
DELETE FROM dbo.AuditEvents;
DELETE FROM dbo.HelpdeskTickets;
DELETE FROM dbo.PayrollRuns;
DELETE FROM dbo.Candidates;
DELETE FROM dbo.ExpenseClaims;
DELETE FROM dbo.LeaveRequests;
DELETE FROM dbo.AttendanceLogs;
IF OBJECT_ID(N'dbo.EmployeeDocuments', N'U') IS NOT NULL
BEGIN
    DELETE FROM dbo.EmployeeDocuments;
END;
DELETE FROM dbo.Employees;
DELETE FROM dbo.Companies;

INSERT dbo.Companies (Id, Name, Industry, City, BrandColor, Branches, Locations)
VALUES (@TenantId, N'DutiellyEasy', N'Workforce Management and HR Services', N'Bengaluru', N'#0f8f7a', 5, 12);

INSERT dbo.Employees (TenantId, Id, Name, Role, Department, Location, Manager, EmploymentType, Grade, JoiningDate, Status, MonthlyCtc, LeaveBalance, Utilization, SkillsCsv)
VALUES
(@TenantId, N'DUTIE-1001', N'Aarav Mehta', N'HR Manager', N'Human Resources', N'Bengaluru HQ', N'Riya Sen', N'Permanent', N'M4', '2021-04-12', N'Active', 145000, 18, 92, N'Employee relations,Onboarding,Analytics'),
(@TenantId, N'DUTIE-1002', N'Isha Rao', N'Payroll Specialist', N'Finance', N'Bengaluru HQ', N'Vikram Sethi', N'Permanent', N'P3', '2022-07-01', N'Active', 92000, 14, 87, N'Payroll,Salary review,Deductions'),
(@TenantId, N'DUTIE-1003', N'Kabir Khan', N'Engineering Manager', N'Engineering', N'Pune Delivery Center', N'Riya Sen', N'Permanent', N'M5', '2020-11-20', N'Active', 235000, 21, 89, N'Platform,Architecture,People leadership'),
(@TenantId, N'DUTIE-1004', N'Meera Iyer', N'Product Designer', N'Product', N'Hyderabad Studio', N'Kabir Khan', N'Permanent', N'P3', '2023-02-06', N'Active', 145000, 16, 94, N'UX research,Figma,Design systems'),
(@TenantId, N'DUTIE-1005', N'Rohan Bansal', N'Operations Lead', N'Operations', N'Delhi Branch', N'Riya Sen', N'Permanent', N'M4', '2020-03-18', N'Active', 118000, 12, 91, N'Operations planning,Vendor management,Site audits'),
(@TenantId, N'DUTIE-1006', N'Sana Sheikh', N'Field Executive', N'Field Services', N'Noida Site', N'Rohan Bansal', N'Permanent', N'F2', '2023-09-11', N'Active', 48000, 10, 96, N'Client visits,Proof collection,Field attendance'),
(@TenantId, N'DUTIE-1007', N'Dev Malhotra', N'Service Engineer', N'Field Services', N'Jaipur Route', N'Rohan Bansal', N'Contract', N'F3', '2022-12-05', N'Active', 62000, 8, 88, N'Installations,Repair,Daily reports'),
(@TenantId, N'DUTIE-1008', N'Tara Gill', N'Employee Support Executive', N'People Operations', N'Bengaluru HQ', N'Aarav Mehta', N'Permanent', N'P2', '2021-08-16', N'Active', 68000, 17, 85, N'Attendance,Leave,Helpdesk');

INSERT dbo.AttendanceLogs (Id, TenantId, EmployeeId, EmployeeName, [Date], CheckIn, CheckOut, Method, Location, SecuritySignal, Status)
VALUES
('10000000-0000-0000-0000-000000000101', @TenantId, N'DUTIE-1001', N'Aarav Mehta', '2026-05-31', '09:08', NULL, N'Web Check-In', N'Bengaluru HQ', N'Tenant access verified and attendance audit saved', N'Approved'),
('10000000-0000-0000-0000-000000000102', @TenantId, N'DUTIE-1002', N'Isha Rao', '2026-05-31', '09:17', '18:12', N'Office Kiosk', N'Bengaluru HQ', N'Tenant access verified and attendance audit saved', N'Approved'),
('10000000-0000-0000-0000-000000000103', @TenantId, N'DUTIE-1006', N'Sana Sheikh', '2026-05-31', '08:42', NULL, N'Mobile Check-In', N'Noida Site', N'Tenant access verified and attendance audit saved', N'Approved'),
('10000000-0000-0000-0000-000000000104', @TenantId, N'DUTIE-1007', N'Dev Malhotra', '2026-05-31', '07:58', '16:44', N'Manager Entry', N'Jaipur Route', N'Tenant access verified and attendance audit saved', N'Approved');

INSERT dbo.LeaveRequests (Id, TenantId, EmployeeId, EmployeeName, LeaveType, FromDate, ToDate, Reason, ManagerStatus, HrStatus)
VALUES
('20000000-0000-0000-0000-000000000101', @TenantId, N'DUTIE-1004', N'Meera Iyer', N'Earned', '2026-06-10', '2026-06-12', N'Family travel', N'Approved', N'Pending'),
('20000000-0000-0000-0000-000000000102', @TenantId, N'DUTIE-1008', N'Tara Gill', N'Sick', '2026-06-03', '2026-06-03', N'Medical appointment', N'Pending', N'Pending'),
('20000000-0000-0000-0000-000000000103', @TenantId, N'DUTIE-1006', N'Sana Sheikh', N'Casual', '2026-06-08', '2026-06-08', N'Personal work', N'Pending', N'Pending'),
('20000000-0000-0000-0000-000000000104', @TenantId, N'DUTIE-1003', N'Kabir Khan', N'Comp Off', '2026-06-14', '2026-06-14', N'Weekend deployment support', N'Approved', N'Approved');

INSERT dbo.ExpenseClaims (Id, TenantId, EmployeeName, Category, Amount, Notes, ManagerStatus, FinanceStatus)
VALUES
('30000000-0000-0000-0000-000000000101', @TenantId, N'Kabir Khan', N'Travel', 6850, N'Client workshop travel', N'Approved', N'Pending'),
('30000000-0000-0000-0000-000000000102', @TenantId, N'Meera Iyer', N'Internet', 1499, N'WFH broadband reimbursement', N'Pending', N'Pending'),
('30000000-0000-0000-0000-000000000103', @TenantId, N'Sana Sheikh', N'Fuel', 2840, N'Noida site visits', N'Approved', N'Approved'),
('30000000-0000-0000-0000-000000000104', @TenantId, N'Dev Malhotra', N'Accommodation', 5200, N'Jaipur route overnight stay', N'Pending', N'Pending');

INSERT dbo.Candidates (Id, TenantId, CandidateName, Position, Stage, AiScore, Owner)
VALUES
('40000000-0000-0000-0000-000000000101', @TenantId, N'Ananya Das', N'Senior .NET Engineer', N'Technical Interview', 91, N'Aarav Mehta'),
('40000000-0000-0000-0000-000000000102', @TenantId, N'Neil George', N'Product Analyst', N'Offer', 86, N'Aarav Mehta'),
('40000000-0000-0000-0000-000000000103', @TenantId, N'Mohit Verma', N'Operations Coordinator', N'HR Screening', 78, N'Tara Gill'),
('40000000-0000-0000-0000-000000000104', @TenantId, N'Pooja Menon', N'Field Executive', N'Final Interview', 84, N'Tara Gill');

INSERT dbo.PayrollRuns (Id, TenantId, [Month], Employees, GrossPay, Deductions, NetPay, Status, UpdatedAt)
VALUES
('50000000-0000-0000-0000-000000000101', @TenantId, N'May 2026', 8, 913000, 132385, 780615, N'Locked', '2026-05-30T18:15:00'),
('50000000-0000-0000-0000-000000000102', @TenantId, N'April 2026', 8, 913000, 132385, 780615, N'Released', '2026-04-30T17:30:00');

INSERT dbo.HelpdeskTickets (Id, TenantId, Category, Subject, Owner, Priority, Status)
VALUES
('60000000-0000-0000-0000-000000000101', @TenantId, N'Payroll', N'Deduction estimate clarification for June payroll', N'Isha Rao', N'Medium', N'Open'),
('60000000-0000-0000-0000-000000000102', @TenantId, N'IT', N'New laptop asset allocation for design intern', N'Tara Gill', N'Low', N'In Progress'),
('60000000-0000-0000-0000-000000000103', @TenantId, N'Attendance', N'Attendance entry query on Jaipur service route', N'Tara Gill', N'High', N'Open'),
('60000000-0000-0000-0000-000000000104', @TenantId, N'Asset', N'Replacement mobile device for field check-ins', N'Rohan Bansal', N'Medium', N'Assigned');

INSERT dbo.AuditEvents (Id, TenantId, At, Actor, Action, Module)
VALUES
('70000000-0000-0000-0000-000000000101', @TenantId, '2026-05-31T09:10:00', N'Aarav Mehta', N'Checked in with Web Check-In', N'Attendance'),
('70000000-0000-0000-0000-000000000102', @TenantId, '2026-05-31T10:05:00', N'Isha Rao', N'Locked May 2026 payroll preview', N'Payroll'),
('70000000-0000-0000-0000-000000000103', @TenantId, '2026-05-31T08:45:00', N'Sana Sheikh', N'Checked in from Noida Site', N'Attendance'),
('70000000-0000-0000-0000-000000000104', @TenantId, '2026-05-31T11:20:00', N'Tara Gill', N'Created attendance entry helpdesk ticket', N'Helpdesk');

INSERT dbo.FeatureFlags (TenantId, Name, IsEnabled)
VALUES
(@TenantId, N'AI Insights', 1),
(@TenantId, N'White Label Branding', 1),
(@TenantId, N'Workflow Builder', 1),
(@TenantId, N'Multi-Language', 1);

DECLARE @AdminUserId uniqueidentifier = (SELECT TOP 1 Id FROM dbo.AppUsers WHERE Email = N'admin@dutielly.local');
IF @AdminUserId IS NOT NULL
BEGIN
    DELETE FROM dbo.AppUserTenants WHERE UserId = @AdminUserId AND TenantId <> @TenantId;

    IF NOT EXISTS (SELECT 1 FROM dbo.AppUserTenants WHERE UserId = @AdminUserId AND TenantId = @TenantId)
    BEGIN
        INSERT dbo.AppUserTenants (UserId, TenantId, IsDefault)
        VALUES (@AdminUserId, @TenantId, 1);
    END;

    UPDATE dbo.AppUserTenants
    SET IsDefault = CASE WHEN TenantId = @TenantId THEN 1 ELSE 0 END
    WHERE UserId = @AdminUserId;
END;

COMMIT TRANSACTION;
GO

SELECT
    c.Id AS TenantId,
    c.Name AS TenantName,
    COUNT(DISTINCT e.Id) AS Employees,
    COUNT(DISTINCT a.Id) AS AttendanceLogs,
    COUNT(DISTINCT p.Id) AS PayrollRuns
FROM dbo.Companies c
LEFT JOIN dbo.Employees e ON e.TenantId = c.Id
LEFT JOIN dbo.AttendanceLogs a ON a.TenantId = c.Id
LEFT JOIN dbo.PayrollRuns p ON p.TenantId = c.Id
GROUP BY c.Id, c.Name
ORDER BY c.Name;
GO
