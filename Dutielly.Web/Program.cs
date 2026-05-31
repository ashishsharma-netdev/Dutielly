using Dutielly.Web.Components;
using Dutielly.Shared.Services;
using Dutielly.Web.Services;
using Dutielly.Shared.Data;
using Dutielly.Web.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add device-specific services used by the Dutielly.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddDbContext<DutiellyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DutiellyDb")));
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Dutielly.Auth";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(10);
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<PasswordHashService>();
builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<UserAccessService>();
builder.Services.AddScoped<AuthDatabaseSeeder>();
builder.Services.AddScoped<IDutiellyPlatformService, SqlServerDutiellyPlatformService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    if (AuthEndpoints.IsPublicPath(context.Request.Path))
    {
        await next();
        return;
    }

    if (context.User.Identity?.IsAuthenticated != true)
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Authentication required.");
            return;
        }

        var returnUrl = Uri.EscapeDataString(context.Request.PathBase + context.Request.Path + context.Request.QueryString);
        context.Response.Redirect($"/login?returnUrl={returnUrl}");
        return;
    }

    var access = context.RequestServices.GetRequiredService<UserAccessService>();
    if (!access.CanUsePath(context.Request.Path))
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Access denied.");
            return;
        }

        context.Response.Redirect("/access-denied");
        return;
    }

    await next();
});
app.UseAntiforgery();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<DutiellyDbContext>();
        db.Database.EnsureCreated();
        await EnsureEmployeeDocumentsTableAsync(db);
        var seeder = scope.ServiceProvider.GetRequiredService<AuthDatabaseSeeder>();
        await seeder.SeedAsync();
    }
    catch
    {
        // SQL Server may not be available during first local setup. The UI shows an empty SQL status until configured.
    }
}

app.MapDutiellyAuth();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Dutielly.Shared._Imports).Assembly);

app.MapGet("/api/dashboard", (IDutiellyPlatformService platform, UserAccessService access, string? tenantId) =>
{
    if (!ApplyTenant(platform, access, tenantId))
    {
        return Results.Forbid();
    }

    return Results.Ok(platform.GetSnapshot("HR Manager"));
}).RequireAuthorization();
app.MapGet("/api/modules", (IDutiellyPlatformService platform) => platform.Modules).RequireAuthorization();
app.MapGet("/api/employees", (IDutiellyPlatformService platform, UserAccessService access, string? tenantId) =>
{
    if (!ApplyTenant(platform, access, tenantId))
    {
        return Results.Forbid();
    }

    return Results.Ok(platform.Employees);
}).RequireAuthorization();
app.MapGet("/api/employees/{employeeId}/documents/{documentId:guid}/view", (IDutiellyPlatformService platform, UserAccessService access, string employeeId, Guid documentId, string? tenantId) =>
{
    if (!ApplyTenant(platform, access, tenantId))
    {
        return Results.Forbid();
    }

    var document = platform.GetEmployeeDocumentContent(employeeId, documentId);
    if (document is null)
    {
        return Results.NotFound();
    }

    return Results.File(document.Content, document.Metadata.ContentType, enableRangeProcessing: true);
}).RequireAuthorization();
app.MapGet("/api/employees/{employeeId}/documents/{documentId:guid}/download", (IDutiellyPlatformService platform, UserAccessService access, string employeeId, Guid documentId, string? tenantId) =>
{
    if (!ApplyTenant(platform, access, tenantId))
    {
        return Results.Forbid();
    }

    var document = platform.GetEmployeeDocumentContent(employeeId, documentId);
    if (document is null)
    {
        return Results.NotFound();
    }

    return Results.File(document.Content, document.Metadata.ContentType, CleanDownloadFileName(document.Metadata), enableRangeProcessing: true);
}).RequireAuthorization();
app.MapPost("/api/employees", async (IDutiellyPlatformService platform, UserAccessService access, HttpRequest httpRequest) =>
{
    var request = await ReadJson<EmployeeCreateRequest>(httpRequest);
    if (request is null)
    {
        return Results.BadRequest("Invalid employee payload.");
    }

    if (!ApplyTenant(platform, access, request.TenantId))
    {
        return Results.Forbid();
    }

    if (!platform.CanManageEmployees)
    {
        return Results.Forbid();
    }

    try
    {
        var employee = platform.AddEmployee(
            request.Name,
            request.Role,
            request.Department,
            request.Location,
            request.Manager,
            request.EmploymentType,
            request.Grade,
            request.MonthlyCtc,
            request.LeaveBalance,
            request.Utilization,
            request.Skills);
        return Results.Created($"/api/employees/{employee.Id}", employee);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization();
app.MapPut("/api/employees/{employeeId}", async (IDutiellyPlatformService platform, UserAccessService access, string employeeId, HttpRequest httpRequest) =>
{
    var request = await ReadJson<EmployeeUpdateRequest>(httpRequest);
    if (request is null)
    {
        return Results.BadRequest("Invalid employee payload.");
    }

    if (!ApplyTenant(platform, access, request.TenantId))
    {
        return Results.Forbid();
    }

    if (!platform.CanManageEmployees)
    {
        return Results.Forbid();
    }

    EmployeeProfile? updated;
    try
    {
        updated = platform.UpdateEmployee(new EmployeeProfile
        {
            Id = employeeId,
            Name = request.Name,
            Role = request.Role,
            Department = request.Department,
            Location = request.Location,
            Manager = request.Manager,
            EmploymentType = request.EmploymentType,
            Grade = request.Grade,
            Status = request.Status,
            MonthlyCtc = request.MonthlyCtc,
            LeaveBalance = request.LeaveBalance,
            Utilization = request.Utilization,
            Skills = request.Skills
        });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }

    return updated is null ? Results.NotFound() : Results.Ok(updated);
}).RequireAuthorization();
app.MapGet("/api/attendance", (IDutiellyPlatformService platform, UserAccessService access, string? tenantId) =>
{
    if (!ApplyTenant(platform, access, tenantId))
    {
        return Results.Forbid();
    }

    return Results.Ok(platform.AttendanceLogs);
}).RequireAuthorization();
app.MapGet("/api/payroll", (IDutiellyPlatformService platform, UserAccessService access, string? tenantId) =>
{
    if (!ApplyTenant(platform, access, tenantId))
    {
        return Results.Forbid();
    }

    return Results.Ok(platform.PayrollHistory);
}).RequireAuthorization();
app.MapPost("/api/attendance/check-in", async (IDutiellyPlatformService platform, UserAccessService access, HttpRequest httpRequest) =>
{
    var request = await ReadJson<AttendanceCheckInRequest>(httpRequest);
    if (request is null)
    {
        return Results.BadRequest("Invalid attendance payload.");
    }

    if (!ApplyTenant(platform, access, request.TenantId))
    {
        return Results.Forbid();
    }

    AttendanceLog? log;
    try
    {
        log = platform.CheckIn(request.EmployeeId, request.Method, request.Location);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }

    if (log is null)
    {
        return Results.BadRequest("Employee record not found. Create the employee in SQL Server first.");
    }

    return Results.Created($"/api/attendance/{log.Id}", log);
}).RequireAuthorization();

app.Run();

static bool ApplyTenant(IDutiellyPlatformService platform, UserAccessService access, string? tenantId)
{
    if (string.IsNullOrWhiteSpace(tenantId))
    {
        platform.CurrentTenantId = access.GetDefaultTenantId();
        return !string.IsNullOrWhiteSpace(platform.CurrentTenantId);
    }

    if (!access.CanAccessTenant(tenantId))
    {
        return false;
    }

    platform.CurrentTenantId = tenantId;
    return true;
}

static async Task<T?> ReadJson<T>(HttpRequest request)
{
    try
    {
        return await JsonSerializer.DeserializeAsync<T>(request.Body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
    catch (JsonException)
    {
        return default;
    }
}

static Task EnsureEmployeeDocumentsTableAsync(DutiellyDbContext db)
{
    return db.Database.ExecuteSqlRawAsync("""
IF OBJECT_ID(N'dbo.EmployeeDocuments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeDocuments (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_EmployeeDocuments PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        EmployeeId nvarchar(64) NOT NULL,
        EmployeeName nvarchar(180) NOT NULL,
        DocumentName nvarchar(180) NOT NULL,
        FileName nvarchar(260) NOT NULL,
        FileExtension nvarchar(20) NOT NULL,
        ContentType nvarchar(120) NOT NULL,
        FileSizeBytes bigint NOT NULL,
        FileContent varbinary(max) NOT NULL,
        UploadedAt datetime2 NOT NULL,
        UploadedBy nvarchar(180) NOT NULL CONSTRAINT DF_EmployeeDocuments_UploadedBy DEFAULT N''
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EmployeeDocuments_TenantId_EmployeeId' AND object_id = OBJECT_ID(N'dbo.EmployeeDocuments'))
BEGIN
    CREATE INDEX IX_EmployeeDocuments_TenantId_EmployeeId ON dbo.EmployeeDocuments (TenantId, EmployeeId, UploadedAt DESC);
END;
""");
}

static string CleanDownloadFileName(EmployeeDocument document)
{
    var baseName = string.IsNullOrWhiteSpace(document.DocumentName) ? Path.GetFileNameWithoutExtension(document.FileName) : document.DocumentName;
    var safeBaseName = new string(baseName.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch).ToArray()).Trim();
    if (string.IsNullOrWhiteSpace(safeBaseName))
    {
        safeBaseName = "employee-document";
    }

    return $"{safeBaseName}.{document.FileExtension}";
}

internal sealed class AttendanceCheckInRequest
{
    public string EmployeeId { get; init; } = string.Empty;
    public string Method { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string? TenantId { get; init; }
}

internal sealed class EmployeeCreateRequest
{
    public string Name { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string Manager { get; init; } = string.Empty;
    public string EmploymentType { get; init; } = string.Empty;
    public string Grade { get; init; } = string.Empty;
    public decimal MonthlyCtc { get; init; }
    public int LeaveBalance { get; init; }
    public double Utilization { get; init; }
    public string[] Skills { get; init; } = [];
    public string? TenantId { get; init; }
}

internal sealed class EmployeeUpdateRequest
{
    public string Name { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string Manager { get; init; } = string.Empty;
    public string EmploymentType { get; init; } = string.Empty;
    public string Grade { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal MonthlyCtc { get; init; }
    public int LeaveBalance { get; init; }
    public double Utilization { get; init; }
    public string[] Skills { get; init; } = [];
    public string? TenantId { get; init; }
}
