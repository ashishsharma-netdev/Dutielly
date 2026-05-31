USE [master];
GO

IF DB_ID(N'DutiellyDb') IS NULL
BEGIN
    CREATE DATABASE [DutiellyDb];
END;
GO

USE [DutiellyDb];
GO

IF OBJECT_ID(N'dbo.Companies', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Companies (
        Id nvarchar(64) NOT NULL CONSTRAINT PK_Companies PRIMARY KEY,
        Name nvarchar(180) NOT NULL,
        Industry nvarchar(120) NOT NULL CONSTRAINT DF_Companies_Industry DEFAULT N'',
        City nvarchar(120) NOT NULL CONSTRAINT DF_Companies_City DEFAULT N'',
        BrandColor nvarchar(20) NOT NULL CONSTRAINT DF_Companies_BrandColor DEFAULT N'#0f8f7a',
        Branches int NOT NULL CONSTRAINT DF_Companies_Branches DEFAULT 0,
        Locations int NOT NULL CONSTRAINT DF_Companies_Locations DEFAULT 0
    );
END;
GO

IF OBJECT_ID(N'dbo.Employees', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Employees (
        TenantId nvarchar(64) NOT NULL,
        Id nvarchar(64) NOT NULL CONSTRAINT PK_Employees PRIMARY KEY,
        Name nvarchar(180) NOT NULL,
        Role nvarchar(140) NOT NULL CONSTRAINT DF_Employees_Role DEFAULT N'',
        Department nvarchar(140) NOT NULL CONSTRAINT DF_Employees_Department DEFAULT N'',
        Location nvarchar(160) NOT NULL CONSTRAINT DF_Employees_Location DEFAULT N'',
        Manager nvarchar(160) NOT NULL CONSTRAINT DF_Employees_Manager DEFAULT N'',
        EmploymentType nvarchar(80) NOT NULL CONSTRAINT DF_Employees_EmploymentType DEFAULT N'Permanent',
        Grade nvarchar(80) NOT NULL CONSTRAINT DF_Employees_Grade DEFAULT N'',
        JoiningDate date NOT NULL,
        Status nvarchar(80) NOT NULL CONSTRAINT DF_Employees_Status DEFAULT N'Active',
        MonthlyCtc decimal(18,2) NOT NULL CONSTRAINT DF_Employees_MonthlyCtc DEFAULT 0,
        LeaveBalance int NOT NULL CONSTRAINT DF_Employees_LeaveBalance DEFAULT 0,
        Utilization float NOT NULL CONSTRAINT DF_Employees_Utilization DEFAULT 0,
        SkillsCsv nvarchar(1000) NOT NULL CONSTRAINT DF_Employees_SkillsCsv DEFAULT N''
    );
END
ELSE IF COL_LENGTH(N'dbo.Employees', N'TenantId') IS NULL
BEGIN
    ALTER TABLE dbo.Employees ADD TenantId nvarchar(64) NOT NULL CONSTRAINT DF_Employees_TenantId DEFAULT N'tenant-default';
END;
GO

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
END
ELSE IF COL_LENGTH(N'dbo.EmployeeDocuments', N'TenantId') IS NULL
BEGIN
    ALTER TABLE dbo.EmployeeDocuments ADD TenantId nvarchar(64) NOT NULL CONSTRAINT DF_EmployeeDocuments_TenantId DEFAULT N'tenant-default';
END;
GO

IF OBJECT_ID(N'dbo.AttendanceLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AttendanceLogs (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_AttendanceLogs PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        EmployeeId nvarchar(64) NOT NULL,
        EmployeeName nvarchar(180) NOT NULL,
        [Date] date NOT NULL,
        CheckIn time NOT NULL,
        CheckOut time NULL,
        Method nvarchar(120) NOT NULL,
        Location nvarchar(180) NOT NULL,
        SecuritySignal nvarchar(300) NOT NULL,
        Status nvarchar(40) NOT NULL
    );
END
ELSE IF COL_LENGTH(N'dbo.AttendanceLogs', N'TenantId') IS NULL
BEGIN
    ALTER TABLE dbo.AttendanceLogs ADD TenantId nvarchar(64) NOT NULL CONSTRAINT DF_AttendanceLogs_TenantId DEFAULT N'tenant-default';
END;
GO

IF OBJECT_ID(N'dbo.LeaveRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveRequests (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_LeaveRequests PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        EmployeeId nvarchar(64) NOT NULL,
        EmployeeName nvarchar(180) NOT NULL,
        LeaveType nvarchar(80) NOT NULL,
        FromDate date NOT NULL,
        ToDate date NOT NULL,
        Reason nvarchar(500) NOT NULL,
        ManagerStatus nvarchar(40) NOT NULL CONSTRAINT DF_LeaveRequests_ManagerStatus DEFAULT N'Pending',
        HrStatus nvarchar(40) NOT NULL CONSTRAINT DF_LeaveRequests_HrStatus DEFAULT N'Pending'
    );
END
ELSE IF COL_LENGTH(N'dbo.LeaveRequests', N'TenantId') IS NULL
BEGIN
    ALTER TABLE dbo.LeaveRequests ADD TenantId nvarchar(64) NOT NULL CONSTRAINT DF_LeaveRequests_TenantId DEFAULT N'tenant-default';
END;
GO

IF OBJECT_ID(N'dbo.ExpenseClaims', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExpenseClaims (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_ExpenseClaims PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        EmployeeName nvarchar(180) NOT NULL,
        Category nvarchar(80) NOT NULL,
        Amount decimal(18,2) NOT NULL,
        Notes nvarchar(500) NOT NULL,
        ManagerStatus nvarchar(40) NOT NULL CONSTRAINT DF_ExpenseClaims_ManagerStatus DEFAULT N'Pending',
        FinanceStatus nvarchar(40) NOT NULL CONSTRAINT DF_ExpenseClaims_FinanceStatus DEFAULT N'Pending'
    );
END
ELSE IF COL_LENGTH(N'dbo.ExpenseClaims', N'TenantId') IS NULL
BEGIN
    ALTER TABLE dbo.ExpenseClaims ADD TenantId nvarchar(64) NOT NULL CONSTRAINT DF_ExpenseClaims_TenantId DEFAULT N'tenant-default';
END;
GO

IF OBJECT_ID(N'dbo.Candidates', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Candidates (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_Candidates PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        CandidateName nvarchar(180) NOT NULL,
        Position nvarchar(160) NOT NULL,
        Stage nvarchar(80) NOT NULL,
        AiScore int NOT NULL CONSTRAINT DF_Candidates_AiScore DEFAULT 0,
        Owner nvarchar(160) NOT NULL
    );
END
ELSE IF COL_LENGTH(N'dbo.Candidates', N'TenantId') IS NULL
BEGIN
    ALTER TABLE dbo.Candidates ADD TenantId nvarchar(64) NOT NULL CONSTRAINT DF_Candidates_TenantId DEFAULT N'tenant-default';
END;
GO

IF OBJECT_ID(N'dbo.PayrollRuns', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PayrollRuns (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_PayrollRuns PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        [Month] nvarchar(80) NOT NULL,
        Employees int NOT NULL,
        GrossPay decimal(18,2) NOT NULL,
        Deductions decimal(18,2) NOT NULL,
        NetPay decimal(18,2) NOT NULL,
        Status nvarchar(80) NOT NULL,
        UpdatedAt datetime2 NOT NULL
    );
END
ELSE IF COL_LENGTH(N'dbo.PayrollRuns', N'TenantId') IS NULL
BEGIN
    ALTER TABLE dbo.PayrollRuns ADD TenantId nvarchar(64) NOT NULL CONSTRAINT DF_PayrollRuns_TenantId DEFAULT N'tenant-default';
END;
GO

IF OBJECT_ID(N'dbo.HelpdeskTickets', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HelpdeskTickets (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_HelpdeskTickets PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        Category nvarchar(80) NOT NULL,
        Subject nvarchar(240) NOT NULL,
        Owner nvarchar(160) NOT NULL,
        Priority nvarchar(40) NOT NULL,
        Status nvarchar(60) NOT NULL
    );
END
ELSE IF COL_LENGTH(N'dbo.HelpdeskTickets', N'TenantId') IS NULL
BEGIN
    ALTER TABLE dbo.HelpdeskTickets ADD TenantId nvarchar(64) NOT NULL CONSTRAINT DF_HelpdeskTickets_TenantId DEFAULT N'tenant-default';
END;
GO

IF OBJECT_ID(N'dbo.AuditEvents', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditEvents (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_AuditEvents PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        At datetime2 NOT NULL,
        Actor nvarchar(180) NOT NULL,
        Action nvarchar(500) NOT NULL,
        Module nvarchar(120) NOT NULL
    );
END
ELSE IF COL_LENGTH(N'dbo.AuditEvents', N'TenantId') IS NULL
BEGIN
    ALTER TABLE dbo.AuditEvents ADD TenantId nvarchar(64) NOT NULL CONSTRAINT DF_AuditEvents_TenantId DEFAULT N'tenant-default';
END;
GO

IF OBJECT_ID(N'dbo.FeatureFlags', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FeatureFlags (
        TenantId nvarchar(64) NOT NULL,
        Name nvarchar(160) NOT NULL,
        IsEnabled bit NOT NULL,
        CONSTRAINT PK_FeatureFlags PRIMARY KEY (TenantId, Name)
    );
END
ELSE
BEGIN
    IF COL_LENGTH(N'dbo.FeatureFlags', N'TenantId') IS NULL
    BEGIN
        ALTER TABLE dbo.FeatureFlags ADD TenantId nvarchar(64) NOT NULL CONSTRAINT DF_FeatureFlags_TenantId DEFAULT N'tenant-default';
    END;

    DECLARE @FeatureFlagsPk sysname =
    (
        SELECT [name]
        FROM sys.key_constraints
        WHERE [type] = N'PK'
          AND parent_object_id = OBJECT_ID(N'dbo.FeatureFlags')
    );

    IF @FeatureFlagsPk IS NOT NULL
       AND NOT EXISTS
       (
           SELECT 1
           FROM sys.key_constraints kc
           JOIN sys.index_columns ic ON kc.parent_object_id = ic.object_id AND kc.unique_index_id = ic.index_id
           JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
           WHERE kc.[type] = N'PK'
             AND kc.parent_object_id = OBJECT_ID(N'dbo.FeatureFlags')
             AND c.[name] = N'TenantId'
       )
    BEGIN
        DECLARE @DropFeatureFlagsPk nvarchar(max) = N'ALTER TABLE dbo.FeatureFlags DROP CONSTRAINT ' + QUOTENAME(@FeatureFlagsPk);
        EXEC sp_executesql @DropFeatureFlagsPk;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.key_constraints
        WHERE [type] = N'PK'
          AND parent_object_id = OBJECT_ID(N'dbo.FeatureFlags')
    )
    BEGIN
        ALTER TABLE dbo.FeatureFlags ADD CONSTRAINT PK_FeatureFlags PRIMARY KEY (TenantId, Name);
    END;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Employees_TenantId' AND object_id = OBJECT_ID(N'dbo.Employees'))
    CREATE INDEX IX_Employees_TenantId ON dbo.Employees (TenantId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EmployeeDocuments_TenantId_EmployeeId' AND object_id = OBJECT_ID(N'dbo.EmployeeDocuments'))
    CREATE INDEX IX_EmployeeDocuments_TenantId_EmployeeId ON dbo.EmployeeDocuments (TenantId, EmployeeId, UploadedAt DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AttendanceLogs_TenantId' AND object_id = OBJECT_ID(N'dbo.AttendanceLogs'))
    CREATE INDEX IX_AttendanceLogs_TenantId ON dbo.AttendanceLogs (TenantId, [Date] DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LeaveRequests_TenantId' AND object_id = OBJECT_ID(N'dbo.LeaveRequests'))
    CREATE INDEX IX_LeaveRequests_TenantId ON dbo.LeaveRequests (TenantId, FromDate DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExpenseClaims_TenantId' AND object_id = OBJECT_ID(N'dbo.ExpenseClaims'))
    CREATE INDEX IX_ExpenseClaims_TenantId ON dbo.ExpenseClaims (TenantId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Candidates_TenantId' AND object_id = OBJECT_ID(N'dbo.Candidates'))
    CREATE INDEX IX_Candidates_TenantId ON dbo.Candidates (TenantId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PayrollRuns_TenantId' AND object_id = OBJECT_ID(N'dbo.PayrollRuns'))
    CREATE INDEX IX_PayrollRuns_TenantId ON dbo.PayrollRuns (TenantId, UpdatedAt DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HelpdeskTickets_TenantId' AND object_id = OBJECT_ID(N'dbo.HelpdeskTickets'))
    CREATE INDEX IX_HelpdeskTickets_TenantId ON dbo.HelpdeskTickets (TenantId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditEvents_TenantId' AND object_id = OBJECT_ID(N'dbo.AuditEvents'))
    CREATE INDEX IX_AuditEvents_TenantId ON dbo.AuditEvents (TenantId, At DESC);
GO

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
GO
