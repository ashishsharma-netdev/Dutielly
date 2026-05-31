# Dutielly User Guide

This guide describes the active Dutielly application scope: web, Windows desktop, Android shell, SQL Server tenant separation, login/RBAC, employee master, attendance, approvals, payroll run tracking, ESS, analytics, and setup.

## Seeded Login

Only the Super Admin account is seeded for presentation access:

```text
admin@dutielly.local / Dutielly@2026!
```

## Installation And Run

### Web Version

```powershell
dotnet run --project Dutielly.Web\Dutielly.Web.csproj --urls http://127.0.0.1:5088
```

Open:

```text
http://127.0.0.1:5088
```

### Windows Version

Use the generated portable package:

```text
artifacts/Dutielly-Windows-Portable.zip
```

The executable is:

```text
artifacts/windows/Dutielly.exe
```

Keep the full publish folder together with the executable.

### Android Version

Use the generated package folder:

```text
artifacts/android
```

Expected Android outputs:

```text
com.dutielly.platform-Signed.apk
com.dutielly.platform-Signed.aab
```

## SQL Server Setup

Use the setup connection:

```text
Server=localhost;Database=master;Trusted_Connection=True;
```

Run:

```powershell
sqlcmd -S localhost -d master -E -b -i database\sqlserver\001_initial_schema.sql
sqlcmd -S localhost -d master -E -b -i database\sqlserver\002_seed_presentation_data.sql
sqlcmd -S localhost -d master -E -b -i database\sqlserver\003_defer_provider_features.sql
```

The application connection is:

```json
"ConnectionStrings": {
  "DutiellyDb": "Server=localhost;Database=DutiellyDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

Tenant separation uses `Companies.Id` and `TenantId` on tenant-owned tables.

## How To Login

1. Open the web app.
2. Enter `admin@dutielly.local`.
3. Enter `Dutielly@2026!`.
4. Click login.
5. The app opens the Super Admin workspace.

The presentation tenant is `DutiellyEasy`.

## Admin And Tenant Setup

Use `Admin Center`.

1. Select a company from the company selector.
2. Review company, branch, and location summary.
3. Review RBAC roles and capabilities.
4. Toggle active feature flags when allowed.
5. Review login, tenant access, protected APIs, password hashing, and audit status.
6. Review the recent audit trail.

## Add A New Employee

Use `Employees`.

1. Login as Super Admin, Company Admin, or HR.
2. Select the correct company/tenant.
3. Open `Employees`.
4. Enter employee name, designation, department, location, manager, employment type, grade, monthly CTC, leave balance, utilization, and skills.
5. Click `Create employee`.
6. The employee is created under the selected tenant.
7. Select the new employee later whenever profile edits are needed.

## Edit An Employee

1. Open `Employees`.
2. Select the employee from the employee list.
3. Update fields such as name, role, department, location, manager, employment type, grade, status, monthly CTC, leave balance, utilization, or skills.
4. Click `Save employee`.
5. The SQL record updates only for the selected tenant.

## Add Employee Document

Use `Employees`.

1. Select the employee from the employee list.
2. Go to `Document vault`.
3. Enter `Document name`, such as PAN card, Aadhaar, offer letter, or bank proof.
4. Click the file picker and select a `.jpg`, `.jpeg`, `.png`, or `.pdf` file.
5. Dutielly stores the file in SQL Server as binary with file name, extension, content type, size, employee ID, tenant ID, uploader, and uploaded date.
6. Use `View` to preview the document in the page.
7. Use `Download` to convert the stored binary back to its original file extension and download it locally.

## Attendance

Use `Attendance`.

1. Select an employee.
2. Choose an attendance method:
   - Web Check-In
   - Mobile Check-In
   - Office Kiosk
   - Manager Entry
3. Enter location.
4. Click `Check in`.
5. The attendance log appears in the register.
6. Click `Checkout` to complete an open attendance log.
7. Use `Request regularization` to create a missed or wrong attendance request message.

## Leave

Use `ESS Mobile` or `Approvals`.

Employee flow:

1. Open `ESS Mobile`.
2. Select employee.
3. Select leave type.
4. Enter leave dates and reason.
5. Click `Apply leave`.

Manager/HR flow:

1. Open `Approvals`.
2. Review leave requests.
3. Click `Manager approve`, `HR approve`, or `Reject`.
4. The status and audit trail update.

## Expenses

Employee flow:

1. Open `ESS Mobile`.
2. Select employee.
3. Enter claim amount.
4. Click `Claims`.
5. The claim is routed to manager and finance.

Manager/Finance flow:

1. Open `Approvals`.
2. Review expense claims.
3. Click `Manager approve` or `Finance approve`.

## Payroll

Use `Payroll`.

1. Login as Payroll Manager, Super Admin, or another allowed payroll role.
2. Open `Payroll`.
3. Review employee count, gross pay, deductions, net pay, and payroll history.
4. Use the formula builder to preview monthly salary, basic amount, allowance amount, deduction estimate, and net salary.
5. Click `Process payroll` after employee salary data exists.
6. Click `Release payroll` when the run is ready.

The current payroll scope is run tracking and deduction estimation. Detailed tax rules, payment files, and salary PDF documents are not part of the active application.

## Recruitment

Use `Approvals`.

1. Open `Approvals`.
2. Review the candidate pipeline.
3. Candidates are grouped by stage.
4. Hiring funnel counts are visible in Analytics.

## ESS Mobile

Use `ESS Mobile`.

1. Select employee.
2. Use `Attendance` for mobile check-in.
3. Use `Leave` to submit leave.
4. Use `Claims` to submit an expense claim.
5. Use `Tickets` to review helpdesk ticket status.

## Analytics

Use `Analytics`.

1. Review department mix and utilization.
2. Review attendance, absence, hiring, and payroll metrics.
3. Review payroll cost split.
4. Select report pack items.
5. Click `Preview report pack`.

## Feature Matrix

Use `Feature Matrix`.

1. Search by module, area, or feature.
2. Filter by area.
3. Review the active module catalogue.

## API Examples

Get employees:

```http
GET /api/employees?tenantId=tenant-dutiellyeasy
```

Create employee:

```http
POST /api/employees
Content-Type: application/json

{
  "name": "New Employee",
  "role": "Developer",
  "department": "Engineering",
  "location": "Pune HQ",
  "manager": "HR Manager",
  "employmentType": "Permanent",
  "grade": "G5",
  "monthlyCtc": 90000,
  "leaveBalance": 18,
  "utilization": 80,
  "skills": ["C#", "SQL"]
}
```

Update employee:

```http
PUT /api/employees/DUTIE-1001
Content-Type: application/json

{
  "id": "DUTIE-1001",
  "name": "Aarav Mehta",
  "role": "HR Manager",
  "department": "Human Resources",
  "location": "Bengaluru HQ",
  "manager": "Riya Sen",
  "employmentType": "Permanent",
  "grade": "M4",
  "status": "Active",
  "monthlyCtc": 145000,
  "leaveBalance": 18,
  "utilization": 92,
  "skills": ["Employee relations", "Onboarding"]
}
```

Check in:

```http
POST /api/attendance/check-in
Content-Type: application/json

{
  "employeeId": "DUTIE-1001",
  "method": "Web Check-In",
  "location": "Bengaluru HQ"
}
```

## Deferred Maintenance

Items requiring provider credentials, signing assets, or deeper business rules are listed in `FUTURE_MAINTENANCE.md` and are not part of the active application UI.
