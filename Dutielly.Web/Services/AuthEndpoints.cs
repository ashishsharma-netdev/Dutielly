using System.Security.Claims;
using Dutielly.Web.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Dutielly.Web.Services;

public static class AuthEndpoints
{
    public static void MapDutiellyAuth(this WebApplication app)
    {
        app.MapGet("/login", (HttpContext context) =>
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                return Results.Redirect(GetLocalReturnUrl(context.Request.Query["returnUrl"], "/"));
            }

            var returnUrl = WebUtility.HtmlEncode(GetLocalReturnUrl(context.Request.Query["returnUrl"], "/"));
            return Results.Content(RenderLoginPage(returnUrl, null), "text/html");
        }).AllowAnonymous();

        app.MapPost("/auth/login", async (HttpContext context, DutiellyDbContext db, PasswordHashService passwordHashService) =>
        {
            var form = await context.Request.ReadFormAsync();
            var email = form["email"].ToString().Trim().ToLowerInvariant();
            var password = form["password"].ToString();
            var returnUrl = GetLocalReturnUrl(form["returnUrl"], "/");

            var user = await db.AppUsers.FirstOrDefaultAsync(item => item.Email == email && item.IsActive);
            if (user is null || !passwordHashService.Verify(password, user.PasswordHash))
            {
                return Results.Content(RenderLoginPage(WebUtility.HtmlEncode(returnUrl), "Invalid email or password."), "text/html", statusCode: StatusCodes.Status401Unauthorized);
            }

            var tenantIds = await db.AppUserTenants
                .AsNoTracking()
                .Where(item => item.UserId == user.Id)
                .OrderByDescending(item => item.IsDefault)
                .ThenBy(item => item.TenantId)
                .Select(item => item.TenantId)
                .ToListAsync();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.DisplayName),
                new(ClaimTypes.Role, user.Role)
            };

            claims.AddRange(tenantIds.Select(tenantId => new Claim("tenant", tenantId)));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(10)
            });

            user.LastLoginAt = DateTime.UtcNow;
            var auditTenantId = tenantIds.FirstOrDefault() ?? "tenant-default";
            db.AuditEvents.Add(new AuditEventEntity
            {
                TenantId = auditTenantId,
                At = DateTime.Now,
                Actor = user.DisplayName,
                Action = $"Signed in as {user.Role}",
                Module = "Security"
            });
            await db.SaveChangesAsync();

            return Results.Redirect(GetLandingUrl(user.Role, returnUrl));
        }).AllowAnonymous();

        app.MapGet("/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/login");
        });

        app.MapGet("/access-denied", () => Results.Content(RenderAccessDeniedPage(), "text/html"));
    }

    public static bool IsPublicPath(PathString path)
    {
        var value = path.Value ?? string.Empty;
        return value.Equals("/login", StringComparison.OrdinalIgnoreCase)
            || value.Equals("/auth/login", StringComparison.OrdinalIgnoreCase)
            || value.Equals("/access-denied", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/_content", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/manifest", StringComparison.OrdinalIgnoreCase)
            || value.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
            || value.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
            || value.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
            || value.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
            || value.EndsWith(".ico", StringComparison.OrdinalIgnoreCase)
            || value.EndsWith(".br", StringComparison.OrdinalIgnoreCase)
            || value.EndsWith(".gz", StringComparison.OrdinalIgnoreCase)
            || value.EndsWith(".map", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetLandingUrl(string role, string returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && returnUrl != "/")
        {
            return returnUrl;
        }

        return role switch
        {
            AuthRoles.Employee => "/ess",
            AuthRoles.PayrollManager => "/payroll",
            AuthRoles.FinanceTeam => "/approvals",
            AuthRoles.DepartmentManager => "/approvals",
            AuthRoles.CompanyAdmin => "/setup",
            AuthRoles.SuperAdmin => "/setup",
            _ => "/"
        };
    }

    private static string GetLocalReturnUrl(string? returnUrl, string fallback)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith('/') || returnUrl.StartsWith("//", StringComparison.Ordinal))
        {
            return fallback;
        }

        return returnUrl;
    }

    private static string RenderLoginPage(string returnUrl, string? error)
    {
        var errorBlock = string.IsNullOrWhiteSpace(error) ? string.Empty : $"""<div class="error">{WebUtility.HtmlEncode(error)}</div>""";
        return $$"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Dutielly Login</title>
    <style>
        body { margin:0; min-height:100vh; display:grid; place-items:center; font-family: Inter, Segoe UI, Arial, sans-serif; background:#eef6f4; color:#062f34; }
        main { width:min(960px, calc(100vw - 32px)); display:grid; grid-template-columns:1.1fr .9fr; overflow:hidden; border-radius:14px; box-shadow:0 24px 80px rgba(5,47,52,.18); background:#fff; }
        .brand { padding:48px; background:#062f34; color:#e9fffb; display:grid; align-content:center; gap:18px; }
        .brand h1 { margin:0; font-size:42px; letter-spacing:0; }
        .brand p { margin:0; line-height:1.6; color:#bdece4; }
        .panel { padding:42px; display:grid; gap:18px; }
        .panel h2 { margin:0; font-size:28px; }
        label { display:grid; gap:7px; font-weight:800; font-size:13px; color:#0b5b58; }
        input { min-height:44px; border:1px solid #c9ddda; border-radius:8px; padding:0 12px; font-size:15px; }
        button { min-height:46px; border:0; border-radius:8px; background:#0f8f7a; color:#fff; font-weight:900; cursor:pointer; }
        .error { background:#ffecec; color:#9f1f1f; border:1px solid #ffc7c7; border-radius:8px; padding:10px 12px; font-weight:800; }
        @media (max-width: 760px) { main { grid-template-columns:1fr; } .brand, .panel { padding:28px; } }
    </style>
</head>
<body>
    <main>
        <section class="brand">
            <h1>Dutielly</h1>
            <p>Secure access for HRMS, attendance, payroll, ESS, manager approvals, analytics, and tenant administration.</p>
            <p>Every signed-in user is scoped by role and tenant access.</p>
        </section>
        <section class="panel">
            <h2>Sign in</h2>
            {{errorBlock}}
            <form method="post" action="/auth/login">
                <input type="hidden" name="returnUrl" value="{{returnUrl}}" />
                <label>Email<input name="email" type="email" autocomplete="username" required autofocus /></label>
                <label>Password<input name="password" type="password" autocomplete="current-password" required /></label>
                <button type="submit">Sign in</button>
            </form>
        </section>
    </main>
</body>
</html>
""";
    }

    private static string RenderAccessDeniedPage()
    {
        return """
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Access denied - Dutielly</title>
    <style>
        body { margin:0; min-height:100vh; display:grid; place-items:center; font-family: Inter, Segoe UI, Arial, sans-serif; background:#eef6f4; color:#062f34; }
        main { width:min(520px, calc(100vw - 32px)); background:#fff; border-radius:12px; padding:32px; box-shadow:0 24px 80px rgba(5,47,52,.16); }
        h1 { margin-top:0; }
        a { color:#0f8f7a; font-weight:900; }
    </style>
</head>
<body>
    <main>
        <h1>Access denied</h1>
        <p>Your role does not have permission for this Dutielly module.</p>
        <p><a href="/">Go to Command Center</a> or <a href="/logout">sign in with another user</a>.</p>
    </main>
</body>
</html>
""";
    }
}
