# Dutielly

Unified multi-tenant HRMS, employee attendance, payroll, ESS, manager, HR, finance, recruitment, analytics, and AI platform.

## What Is Built

- Shared .NET 9 MAUI Blazor app for Web, Windows desktop, and Android shells.
- Interactive modules for Command Center, Employee Master, Attendance, Payroll, Approvals, Analytics, ESS Mobile, and Admin Center.
- Web login with cookie sessions, one seeded Super Admin account, tenant claims, page/API RBAC checks, and tenant access enforcement.
- SQL Server-ready web data layer with tenant-separated HRMS, attendance, payroll, candidate, ticket, feature flag, and audit tables.
- Presentation seed data for the `DutiellyEasy` tenant with employees, attendance, payroll, approvals, candidates, tickets, audit events, and feature flags.
- Web-hosted minimal APIs:
  - `GET /api/dashboard`
  - `GET /api/modules`
  - `GET /api/employees`
  - `GET /api/attendance`
  - `GET /api/payroll`
- `POST /api/attendance/check-in`
- `POST /api/employees`
- `PUT /api/employees/{employeeId}`
- Custom generated workforce wallpaper in `Dutielly.Shared/wwwroot/images/dutielly-workforce-wallpaper.png`.
- Native app icon, splash screen, favicon, and PWA manifest.
- Feature comparison notes in `FEATURE_AUDIT.md`.
- Deferred production/provider items in `FUTURE_MAINTENANCE.md`.
- Complete use-case catalogue in `USE_CASES.md`.
- Role-wise user guide and functionality manual in `USER_GUIDE.md`.
- Manual SQL schema in `database/sqlserver/001_initial_schema.sql`.
- AWS deployment template with Docker, Terraform, App Runner, ECR, RDS SQL Server, Secrets Manager, and GitHub Actions in `AWS_DEPLOYMENT.md`.

## Projects

- `Dutielly.Shared`: shared UI, domain models, services, CSS, images, and components.
- `Dutielly.Web`: ASP.NET Core web app plus lightweight API endpoints.
- `Dutielly`: .NET MAUI native app shell for Windows and Android.

## Run Locally

```powershell
dotnet run --project Dutielly.Web\Dutielly.Web.csproj --urls http://127.0.0.1:5088
```

Open `http://127.0.0.1:5088`.

Seeded web login:

```text
admin@dutielly.local / Dutielly@2026!
```

## SQL Server

The web host reads:

```json
"ConnectionStrings": {
  "DutiellyDb": "Server=localhost;Database=DutiellyDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

Set `ConnectionStrings:DutiellyDb` in `Dutielly.Web/appsettings.json`, user secrets, environment variables, or your hosting configuration for your real SQL Server. Use the `master` database connection only for setup, then run the app against `DutiellyDb`.

Create or upgrade the database, then load presentation data:

```powershell
sqlcmd -S localhost -d master -E -b -i database\sqlserver\001_initial_schema.sql
sqlcmd -S localhost -d master -E -b -i database\sqlserver\002_seed_presentation_data.sql
sqlcmd -S localhost -d master -E -b -i database\sqlserver\003_defer_provider_features.sql
```

Multi-tenant separation uses `Companies.Id` as the tenant ID. Tenant-owned tables include `TenantId`, and the web service filters reads/writes by the selected tenant. The presentation tenant is `tenant-dutiellyeasy`.

## Build

```powershell
dotnet build Dutielly.Web\Dutielly.Web.csproj -c Release
dotnet build Dutielly\Dutielly.csproj -c Release -f net9.0-windows10.0.19041.0
```

## Publish Artifacts

```powershell
dotnet publish Dutielly.Web\Dutielly.Web.csproj -c Release -o artifacts\web
dotnet publish Dutielly\Dutielly.csproj -c Release -f net9.0-windows10.0.19041.0 -p:WindowsPackageType=None -o artifacts\windows
```

The Windows desktop executable is `artifacts/windows/Dutielly.exe`. Keep the publish folder intact when moving it because MAUI desktop apps load companion runtime and WebView assets beside the executable.

Current generated deliverables:

- Web publish zip: `artifacts/Dutielly-Web.zip`
- Windows portable zip: `artifacts/Dutielly-Windows-Portable.zip`
- Windows executable: `artifacts/windows/Dutielly.exe`
- Android package zip: `artifacts/Dutielly-Android.zip`
- Android APK: `artifacts/android/com.dutielly.platform-Signed.apk`
- Android AAB: `artifacts/android/com.dutielly.platform-Signed.aab`

## Mobile Packaging Notes

Android APK/AAB generation is configured through the local SDK folder `.android-sdk`:

```powershell
dotnet publish Dutielly\Dutielly.csproj -c Release -f net9.0-android -p:AndroidSdkDirectory="$PWD\.android-sdk" -p:AcceptAndroidSDKLicenses=True -o artifacts\android
```

Generated files:

- `artifacts/android/com.dutielly.platform-Signed.apk`
- `artifacts/android/com.dutielly.platform-Signed.aab`

## AWS Deployment

The AWS deployment path is documented in `AWS_DEPLOYMENT.md`.

It includes:

- `Dockerfile`
- `.github/workflows/aws-deploy.yml`
- `infra/aws/terraform`
- `infra/aws/state-backend`
