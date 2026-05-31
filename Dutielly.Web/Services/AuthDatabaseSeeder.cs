using Dutielly.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Dutielly.Web.Services;

public sealed class AuthDatabaseSeeder(DutiellyDbContext db, PasswordHashService passwordHashService)
{
    public async Task SeedAsync()
    {
        await EnsureAuthTablesAsync();
        await EnsureDefaultTenantAsync();

        var tenantIds = await db.Companies
            .AsNoTracking()
            .OrderBy(company => company.Name)
            .Select(company => company.Id)
            .ToListAsync();

        if (tenantIds.Count == 0)
        {
            return;
        }

        const string password = "Dutielly@2026!";

        await EnsureUserAsync("admin@dutielly.local", "Dutielly Super Admin", AuthRoles.SuperAdmin, password, tenantIds);
    }

    private async Task EnsureDefaultTenantAsync()
    {
        if (await db.Companies.AnyAsync())
        {
            return;
        }

        db.Companies.Add(new CompanyEntity
        {
            Id = "tenant-dutiellyeasy",
            Name = "DutiellyEasy",
            Industry = "Workforce Management and HR Services",
            City = "Bengaluru",
            BrandColor = "#0f8f7a",
            Branches = 1,
            Locations = 1
        });

        await db.SaveChangesAsync();
    }

    private async Task EnsureUserAsync(string email, string displayName, string role, string password, IReadOnlyList<string> tenantIds)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await db.AppUsers.FirstOrDefaultAsync(item => item.Email == normalizedEmail);
        if (user is null)
        {
            user = new AppUserEntity
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                DisplayName = displayName,
                Role = role,
                PasswordHash = passwordHashService.Hash(password),
                IsActive = true,
                MustChangePassword = false,
                CreatedAt = DateTime.UtcNow
            };

            db.AppUsers.Add(user);
        }
        else
        {
            user.DisplayName = displayName;
            user.Role = role;
            user.IsActive = true;
        }

        var normalizedTenantIds = tenantIds.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var existingLinks = await db.AppUserTenants
            .Where(item => item.UserId == user.Id)
            .ToListAsync();
        var staleLinks = existingLinks
            .Where(item => !normalizedTenantIds.Contains(item.TenantId, StringComparer.OrdinalIgnoreCase))
            .ToList();
        db.AppUserTenants.RemoveRange(staleLinks);

        foreach (var tenantId in normalizedTenantIds)
        {
            var existingLink = existingLinks.FirstOrDefault(item => item.TenantId.Equals(tenantId, StringComparison.OrdinalIgnoreCase));
            if (existingLink is null)
            {
                db.AppUserTenants.Add(new AppUserTenantEntity
                {
                    UserId = user.Id,
                    TenantId = tenantId,
                    IsDefault = tenantId == normalizedTenantIds[0]
                });
            }
            else
            {
                existingLink.IsDefault = tenantId == normalizedTenantIds[0];
            }
        }

        await db.SaveChangesAsync();
    }

    private async Task EnsureAuthTablesAsync()
    {
        await db.Database.ExecuteSqlRawAsync("""
IF OBJECT_ID(N'dbo.AppUsers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppUsers (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_AppUsers PRIMARY KEY,
        Email nvarchar(256) NOT NULL,
        DisplayName nvarchar(180) NOT NULL,
        Role nvarchar(80) NOT NULL,
        PasswordHash nvarchar(500) NOT NULL,
        IsActive bit NOT NULL CONSTRAINT DF_AppUsers_IsActive DEFAULT 1,
        MustChangePassword bit NOT NULL CONSTRAINT DF_AppUsers_MustChangePassword DEFAULT 0,
        CreatedAt datetime2 NOT NULL,
        LastLoginAt datetime2 NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_AppUsers_Email' AND object_id = OBJECT_ID(N'dbo.AppUsers'))
    CREATE UNIQUE INDEX UX_AppUsers_Email ON dbo.AppUsers (Email);

IF OBJECT_ID(N'dbo.AppUserTenants', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppUserTenants (
        UserId uniqueidentifier NOT NULL,
        TenantId nvarchar(64) NOT NULL,
        IsDefault bit NOT NULL CONSTRAINT DF_AppUserTenants_IsDefault DEFAULT 0,
        CONSTRAINT PK_AppUserTenants PRIMARY KEY (UserId, TenantId)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AppUserTenants_TenantId' AND object_id = OBJECT_ID(N'dbo.AppUserTenants'))
    CREATE INDEX IX_AppUserTenants_TenantId ON dbo.AppUserTenants (TenantId);
""");
    }
}
