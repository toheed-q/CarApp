USE [ACD]
GO

-- =============================================================
-- PHASE 1: City-Based Location Foundation
-- Database: ACD
-- =============================================================


-- =============================================================
-- TASK 1: CREATE CityLocations TABLE
-- =============================================================

IF NOT EXISTS (
    SELECT * FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[CityLocations]') AND type IN (N'U')
)
BEGIN
    CREATE TABLE [dbo].[CityLocations]
    (
        [Id]       INT            IDENTITY(1,1) NOT NULL,
        [CityName] NVARCHAR(100)  NOT NULL,
        [Latitude] FLOAT          NOT NULL,
        [Longitude] FLOAT         NOT NULL,
        [IsActive] BIT            NOT NULL CONSTRAINT [DF_CityLocations_IsActive] DEFAULT (1),

        CONSTRAINT [PK_CityLocations] PRIMARY KEY CLUSTERED ([Id] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF,
                  ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
    )
    ON [PRIMARY]

    PRINT 'CityLocations table created successfully.'
END
ELSE
BEGIN
    PRINT 'CityLocations table already exists — skipping creation.'
END
GO


-- =============================================================
-- TASK 2: SEED DATA
-- =============================================================

SET IDENTITY_INSERT [dbo].[CityLocations] ON

-- Only insert if table is empty (idempotent seed)
IF NOT EXISTS (SELECT 1 FROM [dbo].[CityLocations])
BEGIN
    INSERT INTO [dbo].[CityLocations] ([Id], [CityName], [Latitude], [Longitude], [IsActive])
    VALUES
        (1, N'Mumbai',    19.0760,  72.8777, 1),
        (2, N'Delhi',     28.6139,  77.2090, 1),
        (3, N'Bangalore', 12.9716,  77.5946, 1),
        (4, N'Karachi',   24.8607,  67.0011, 1),
        (5, N'Lahore',    31.5204,  74.3587, 1)

    PRINT 'CityLocations seed data inserted successfully (5 cities).'
END
ELSE
BEGIN
    PRINT 'CityLocations already has data — skipping seed.'
END

SET IDENTITY_INSERT [dbo].[CityLocations] OFF
GO


-- =============================================================
-- TASK 3: INDEX ON CityName
-- =============================================================

IF NOT EXISTS (
    SELECT * FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'[dbo].[CityLocations]')
      AND name = N'IX_CityLocations_CityName'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CityLocations_CityName]
        ON [dbo].[CityLocations] ([CityName] ASC)
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF,
              SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF,
              ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
        ON [PRIMARY]

    PRINT 'Index IX_CityLocations_CityName created successfully.'
END
ELSE
BEGIN
    PRINT 'Index IX_CityLocations_CityName already exists — skipping.'
END
GO


-- =============================================================
-- TASK 4: VALIDATION QUERIES
-- =============================================================

-- 4a. Confirm table exists in sys.objects
SELECT
    OBJECT_NAME(object_id)  AS TableName,
    create_date             AS CreatedAt,
    modify_date             AS LastModified
FROM sys.objects
WHERE object_id = OBJECT_ID(N'[dbo].[CityLocations]')
  AND type = N'U';

-- 4b. Confirm index exists
SELECT
    i.name          AS IndexName,
    i.type_desc     AS IndexType,
    c.name          AS IndexedColumn
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
INNER JOIN sys.columns c        ON c.object_id  = ic.object_id AND c.column_id = ic.column_id
WHERE i.object_id = OBJECT_ID(N'[dbo].[CityLocations]');

-- 4c. Confirm all 5 rows are present with correct coordinates
SELECT
    Id,
    CityName,
    Latitude,
    Longitude,
    IsActive
FROM [dbo].[CityLocations]
ORDER BY Id;

-- 4d. Spot-check: recompute geography point for each city
--     and confirm STDistance between Mumbai and Delhi is ~1150 km
DECLARE @Mumbai    GEOGRAPHY = geography::Point(19.0760,  72.8777, 4326);
DECLARE @Delhi     GEOGRAPHY = geography::Point(28.6139,  77.2090, 4326);
DECLARE @Bangalore GEOGRAPHY = geography::Point(12.9716,  77.5946, 4326);
DECLARE @Karachi   GEOGRAPHY = geography::Point(24.8607,  67.0011, 4326);
DECLARE @Lahore    GEOGRAPHY = geography::Point(31.5204,  74.3587, 4326);

SELECT
    'Mumbai'    AS City1, 'Delhi'     AS City2,
    ROUND(@Mumbai.STDistance(@Delhi)     / 1000.0, 1) AS DistanceKm
UNION ALL SELECT
    'Mumbai',              'Bangalore',
    ROUND(@Mumbai.STDistance(@Bangalore) / 1000.0, 1)
UNION ALL SELECT
    'Mumbai',              'Karachi',
    ROUND(@Mumbai.STDistance(@Karachi)   / 1000.0, 1)
UNION ALL SELECT
    'Lahore',              'Karachi',
    ROUND(@Lahore.STDistance(@Karachi)   / 1000.0, 1);
GO
