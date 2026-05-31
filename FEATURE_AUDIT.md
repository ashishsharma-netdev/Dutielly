# Dutielly Feature Comparison Audit

Compared against the HRMS, attendance, mobile, payroll, and tenant requirements document.

## Current Active Scope

- Multi-tenant SQL Server schema with `TenantId` on tenant-owned tables.
- Web login with one seeded Super Admin user, hashed password, cookie sessions, role claims, tenant claims, protected pages, protected APIs, and tenant access enforcement.
- Tenant-scoped company, employee, attendance, leave, expense, candidate, payroll, ticket, feature flag, and audit data.
- Employee Master create/edit flow with SQL-backed API endpoints.
- Employee Document Vault with SQL Server binary storage for JPG, JPEG, PNG, and PDF files.
- Attendance check-in/out using web, mobile, office kiosk, and manager-entry methods.
- Leave, expense, manager, HR, finance, recruitment, helpdesk, analytics, ESS, and setup screens.
- Payroll run preview, deduction estimate, lock/release status, and payroll history.
- Presentation seed data for the single `DutiellyEasy` tenant.
- Web, Windows desktop, and Android build targets.

## Removed From Active Application

The following items are no longer displayed as implemented features and are tracked for later production maintenance:

- MFA, password reset, SSO, and external identity providers.
- Real GPS, geofence, biometric, face recognition, QR, NFC, Bluetooth, Wi-Fi, and device validation providers.
- Statutory payroll engine, tax/PF/ESI/PT/LWF/TDS rules, Form 16, and payslip PDF generation.
- Bank files, salary payment integrations, and accounting integrations.
- SMS, WhatsApp, external email, Teams, Slack, and push notification providers.
- Document storage, generated document repository, and e-signature providers.
- Mobile API sync, offline sync, and conflict resolution.
- iOS signed IPA packaging.
- Full automated test suite.

## SQL Server Scope

The web app uses `ConnectionStrings:DutiellyDb` and `DutiellyDbContext`.

Tables:

- `Companies`
- `Employees`
- `EmployeeDocuments`
- `AttendanceLogs`
- `LeaveRequests`
- `ExpenseClaims`
- `Candidates`
- `PayrollRuns`
- `HelpdeskTickets`
- `AuditEvents`
- `FeatureFlags`
- `AppUsers`
- `AppUserTenants`

Database scripts:

- `database/sqlserver/001_initial_schema.sql`
- `database/sqlserver/002_seed_presentation_data.sql`
- `database/sqlserver/003_defer_provider_features.sql`

## UX And Functional Hardening

Completed on 2026-05-31 after reviewing the employee, attendance, ESS, approvals, payroll, setup, and analytics flows.

- Employee create no longer saves incomplete placeholder records. Name, role, department, location, manager, employment type, grade, and positive monthly CTC are required before `Create employee` is enabled.
- Employee edit now has a clear selected-record editor and validates required profile, manager, employment, grade, salary, leave, and utilization values before saving.
- Dashboard action buttons now open the payroll and attendance modules instead of creating payroll or attendance records automatically.
- Attendance check-in requires selected employee, method, and location. The page defaults to the selected employee location without silently saving.
- ESS leave requires selected employee, leave type, date range, and reason. ESS claims require category, positive amount, and notes.
- Approvals no longer creates a zero-amount claim for the first employee. Claims are submitted only through an explicit intake form.
- Payroll processing is blocked until employee payroll data exists and percentage inputs are in a valid range.
- Setup and analytics buttons no longer claim to save/export provider-backed outputs; they now show review or preview status only.
- Service and API methods reject incomplete employee, attendance, leave, expense, and payroll requests so invalid writes are blocked even outside the UI.

Reference baseline from common HRMS platforms:

- BambooHR keeps time tracking, approvals, PTO, employee data, and payroll reporting in one flow: https://partners.bamboohr.com/time-tracking/
- Zoho People exposes attendance, time-off workflows, payroll reports, and expense management as structured self-service modules: https://www.zoho.com/people/core-hr.html
- Keka positions leave and attendance as configurable workflows integrated with payroll: https://www.keka.com/attendance-management-system
- greytHR documents leave and attendance as policy-driven capabilities rather than silent record creation: https://admin-help.greythr.com/admin/answers/123842423/

## Remaining Production Decisions

- Final hosting environment and SQL Server deployment topology.
- Whether high-security customers need database-per-tenant isolation.
- Final API design for native mobile clients.
- Provider choices and credentials for the deferred maintenance backlog.
- Final payroll statutory/business rules by customer and region.
