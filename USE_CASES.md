# Dutielly Use Cases

This catalogue describes the active Dutielly application scope after the provider-heavy features were removed from the current product.

## Actors

| Actor | Purpose |
| --- | --- |
| Super Admin | Manages tenants, role access, feature flags, audit, and platform setup. |
| Company Admin | Manages company profile, branches, locations, hierarchy, and policies. |
| HR Manager | Manages employees, recruitment, onboarding, attendance review, leave, and lifecycle processes. |
| Payroll Manager | Processes payroll runs, reviews salary totals, deduction estimates, lock/release status, and payroll reports. |
| Department Manager | Reviews team attendance, leave, claims, timesheets, overtime, and performance. |
| Employee | Uses ESS for attendance, leave, claims, profile status, requests, and helpdesk tickets. |
| Finance Team | Reviews expense claims, reimbursements, payroll status, and finance reports. |
| Recruiter | Tracks candidates, interviews, feedback, and offers. |

## Multi-Tenant Foundation

| ID | Use Case | Actors | Outcome |
| --- | --- | --- | --- |
| UC-001 | Sign in | All users | User signs in with email/password and receives role and tenant claims. |
| UC-002 | Select tenant | Super Admin, Company Admin | User selects an authorized organization. |
| UC-003 | Enforce tenant separation | System | Reads and writes are filtered by `TenantId`. |
| UC-004 | Manage company profile | Super Admin, Company Admin | Company, branch, and location details are maintained. |
| UC-005 | View RBAC roles | Super Admin, Company Admin | Role capabilities and module access are visible. |
| UC-006 | Manage feature flags | Super Admin, Company Admin | Active feature controls are toggled per tenant. |
| UC-007 | Review audit trail | Super Admin, HR Manager | Recent tenant actions are visible for review. |

## Employee Lifecycle

| ID | Use Case | Actors | Outcome |
| --- | --- | --- | --- |
| UC-010 | Add employee | HR Manager, Company Admin | New employee is created under the selected tenant only after required HR and payroll fields are entered. |
| UC-011 | Edit employee | HR Manager, Company Admin | Employee profile, role, department, location, manager, employment type, grade, salary input, leave balance, utilization, and status are validated and updated. |
| UC-012 | Search employee | HR Manager, Manager | Employee list is filtered by name, role, department, or location. |
| UC-013 | View employee analytics | HR Manager, Leadership | Headcount and department mix are visible. |
| UC-014 | Track onboarding checklist | HR Manager | HR, IT, manager, asset, and orientation checklist steps are visible. |
| UC-015 | Track separation | HR Manager | Resignation, notice, exit interview, asset recovery, and settlement review are tracked. |
| UC-016 | Upload employee document | HR Manager, Company Admin | JPG, JPEG, PNG, or PDF file is stored in SQL Server as binary with document metadata. |
| UC-017 | View/download employee document | HR Manager, Company Admin | Stored binary document is served back with the original file extension for preview or download. |

## Attendance And Workforce

| ID | Use Case | Actors | Outcome |
| --- | --- | --- | --- |
| UC-020 | Capture web check-in | Employee, HR Manager | Attendance log is created with employee, method, location, timestamp, and tenant audit signal. |
| UC-021 | Capture mobile check-in | Employee | Mobile attendance log is created for the selected employee. |
| UC-022 | Capture office kiosk entry | HR Manager | Office kiosk attendance method is recorded. |
| UC-023 | Capture manager entry | Manager | Manager-entered attendance log is recorded. |
| UC-024 | Capture checkout | Employee, HR Manager | Open attendance log is completed. |
| UC-025 | Request regularization | Employee | Missed check-in, time correction, or wrong attendance request is routed. |
| UC-026 | Review branch/site summary | Manager, HR Manager | Branch and client-site attendance summaries are visible. |
| UC-027 | Review shift roster | HR Manager, Manager | General, morning, evening, night, flexible, and rotational shift coverage is visible. |
| UC-028 | Track WFH notes | Employee, Manager | Remote work logs and productivity notes are visible. |
| UC-029 | Track daily reports | Employee, Manager | Tasks, meetings, attachments, and utilization notes are visible. |

## Leave And Approvals

| ID | Use Case | Actors | Outcome |
| --- | --- | --- | --- |
| UC-030 | Apply leave | Employee | Leave request with leave type, date range, and reason is created and routed to manager and HR. |
| UC-031 | Approve leave as manager | Manager | Manager approval status is updated. |
| UC-032 | Approve leave as HR | HR Manager | HR approval status is updated. |
| UC-033 | Reject leave | Manager, HR Manager | Leave request is rejected and audited. |
| UC-034 | Approve all manager queue | Manager | Pending manager items are approved. |

## Expenses And Finance

| ID | Use Case | Actors | Outcome |
| --- | --- | --- | --- |
| UC-040 | Submit expense claim | Employee | Claim is submitted with category, positive amount, and notes. |
| UC-041 | Manager approves claim | Manager | Manager approval status is updated. |
| UC-042 | Finance approves claim | Finance Team | Finance approval status is updated. |
| UC-043 | Review reimbursement queue | Finance Team | Claims requiring finance action are visible. |

## Payroll

| ID | Use Case | Actors | Outcome |
| --- | --- | --- | --- |
| UC-050 | Process payroll | Payroll Manager | Payroll run is created from current tenant employees and salary totals after payroll data exists. |
| UC-051 | Preview payroll totals | Payroll Manager | Gross pay, deduction estimate, and net pay are visible. |
| UC-052 | Review payroll history | Payroll Manager, Finance Team | Current and past payroll runs are visible. |
| UC-053 | Release payroll | Payroll Manager | Latest payroll run is marked released for the selected tenant. |
| UC-054 | Review payroll analytics | Payroll Manager, Leadership | Payroll cost trend and cost split are visible. |

## Recruitment And Growth

| ID | Use Case | Actors | Outcome |
| --- | --- | --- | --- |
| UC-060 | View candidate pipeline | Recruiter, HR Manager | Candidates are grouped by stage. |
| UC-061 | Review hiring funnel | Recruiter, Leadership | Active candidates and pipeline health are visible. |
| UC-062 | Review performance module | HR Manager, Manager | KPI, OKR, feedback, rating, promotion, and increment areas are listed. |
| UC-063 | Review learning module | HR Manager | Courses, materials, assessment, and certificates are listed. |

## ESS And Helpdesk

| ID | Use Case | Actors | Outcome |
| --- | --- | --- | --- |
| UC-070 | Use ESS dashboard | Employee | Employee sees attendance, leave balance, claims, tickets, and profile status. |
| UC-071 | Apply leave from ESS | Employee | Leave request is submitted from the employee app screen with dates and reason. |
| UC-072 | Submit claim from ESS | Employee | Claim is submitted from the employee app screen with category, positive amount, and notes. |
| UC-073 | Open ticket list | Employee | Helpdesk ticket summary is displayed. |
| UC-074 | Review helpdesk queue | HR Manager, IT, Finance | Tickets by category, priority, owner, and status are visible. |

## Reporting And AI

| ID | Use Case | Actors | Outcome |
| --- | --- | --- | --- |
| UC-080 | View command center | All roles | Tenant metrics, modules, insights, attendance, payroll, and workflow steps are visible. |
| UC-081 | Preview report pack | HR Manager, Leadership | HR analytics, attendance exceptions, payroll cost, and workforce summary are prepared for review. |
| UC-082 | Ask assistant | Employee, Manager, HR Manager, Payroll Manager | Assistant answers from permitted attendance, salary, leave, recruitment, payroll, and policy records. |
| UC-083 | View feature matrix | Super Admin, Company Admin | Active modules and areas are searchable and filterable. |

## Release Packaging

| ID | Use Case | Actors | Outcome |
| --- | --- | --- | --- |
| UC-090 | Publish web app | Developer, Release Manager | ASP.NET Core web artifact is generated. |
| UC-091 | Publish Windows app | Developer, Release Manager | Windows portable desktop artifact is generated. |
| UC-092 | Publish Android app | Developer, Release Manager | Android APK/AAB artifact is generated when the SDK build completes. |
