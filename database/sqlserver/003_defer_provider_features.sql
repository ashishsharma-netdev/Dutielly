USE [DutiellyDb];
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @TenantId nvarchar(64) = N'tenant-dutiellyeasy';

DELETE FROM dbo.FeatureFlags
WHERE Name IN
(
    N'Face Recognition',
    N'Dynamic QR',
    N'Offline Attendance',
    N'AI Fraud Detection',
    N'E-Signature',
    N'MFA Enforcement',
    N'Multi-Currency',
    N'Smart Watch Attendance',
    N'AI Attrition Prediction'
);

IF EXISTS (SELECT 1 FROM dbo.Companies WHERE Id = @TenantId)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.FeatureFlags WHERE TenantId = @TenantId AND Name = N'AI Insights')
        INSERT dbo.FeatureFlags (TenantId, Name, IsEnabled) VALUES (@TenantId, N'AI Insights', 1);
    IF NOT EXISTS (SELECT 1 FROM dbo.FeatureFlags WHERE TenantId = @TenantId AND Name = N'White Label Branding')
        INSERT dbo.FeatureFlags (TenantId, Name, IsEnabled) VALUES (@TenantId, N'White Label Branding', 1);
    IF NOT EXISTS (SELECT 1 FROM dbo.FeatureFlags WHERE TenantId = @TenantId AND Name = N'Workflow Builder')
        INSERT dbo.FeatureFlags (TenantId, Name, IsEnabled) VALUES (@TenantId, N'Workflow Builder', 1);
    IF NOT EXISTS (SELECT 1 FROM dbo.FeatureFlags WHERE TenantId = @TenantId AND Name = N'Multi-Language')
        INSERT dbo.FeatureFlags (TenantId, Name, IsEnabled) VALUES (@TenantId, N'Multi-Language', 1);
END;

COMMIT TRANSACTION;
GO

SELECT TenantId, Name, IsEnabled
FROM dbo.FeatureFlags
ORDER BY TenantId, Name;
GO
