# Dutielly Architecture

## Platform Shape

Dutielly is implemented as a shared .NET 9 MAUI Blazor solution:

- One shared UI and product layer in `Dutielly.Shared`.
- One ASP.NET Core web host in `Dutielly.Web`.
- One native MAUI host in `Dutielly` for Windows and Android.

This keeps web, desktop, and mobile features aligned. The web host now contains the SQL Server data layer, while the native shell keeps an empty local workspace until it is pointed at the production API.

## Product Modules

The shared service models the requirement document as module groups:

- HRMS: organization, employee master, recruitment, onboarding, performance, learning.
- Workforce: attendance, live tracking, shifts, leave, regularization, WFH, timesheets, daily reports.
- Payroll: salary review, payroll runs, deduction estimates, lock/release status, and payroll history.
- Finance: expenses, reimbursements, approval routing, payroll review, and finance reports.
- Portals: ESS, manager, HR, finance, recruiter.
- Experience: engagement, helpdesk, and in-app notifications.
- Analytics and AI: dashboards, forecasts, attrition signals, assistant responses.
- Enterprise and security: cookie login, password hashing, RBAC, tenant claims, audit logs, feature flags, white label, and workflow builder.

## Recommended Production Evolution

1. Replace native-shell local service calls with typed API clients that call the web/API host.
2. Expand the backend into Clean Architecture projects:
   - `Dutielly.Api`
   - `Dutielly.Application`
   - `Dutielly.Domain`
   - `Dutielly.Infrastructure`
3. Continue persisting tenant data in SQL Server and cache live attendance/session state in Redis.
4. Add JWT + refresh tokens and any organization-specific identity rules after provider decisions are finalized.
5. Add SignalR for live attendance, approvals, notification center, and dashboard updates.
6. Add background workers for payroll processing, report generation, notification dispatch, and scheduled maintenance.
7. Keep provider-heavy work in `FUTURE_MAINTENANCE.md` until credentials, signing assets, and business rules are available.

## Package Targets

- Web: publish from `Dutielly.Web`.
- Windows: publish `Dutielly` with `net9.0-windows10.0.19041.0`.
- Android: build `Dutielly` with `net9.0-android` using the local `.android-sdk` folder.
