-- =============================================================
-- Adds a nullable CityId column to CarDetail, linking a car to a
-- CityLocations row. Used for exact city-based filtering in dbo.GetCars.
-- Idempotent: safe to run multiple times.
-- Existing rows keep CityId = NULL (no backfill) and simply won't appear
-- under a city filter until they are edited and a city is chosen.
-- =============================================================

-- 1) Column ------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[CarDetail]') AND name = 'CityId')
BEGIN
    ALTER TABLE [dbo].[CarDetail] ADD [CityId] INT NULL;
    PRINT 'CarDetail.CityId column added.';
END
ELSE
    PRINT 'CarDetail.CityId already exists - skipped.';
GO

-- 2) Foreign key -> CityLocations(Id) ----------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CarDetail_CityLocations')
BEGIN
    ALTER TABLE [dbo].[CarDetail]
        ADD CONSTRAINT [FK_CarDetail_CityLocations]
        FOREIGN KEY ([CityId]) REFERENCES [dbo].[CityLocations]([Id])
        ON DELETE SET NULL;
    PRINT 'FK_CarDetail_CityLocations added.';
END
ELSE
    PRINT 'FK_CarDetail_CityLocations already exists - skipped.';
GO

-- 3) Index for fast filtering by city ----------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_CarDetail_CityId' AND object_id = OBJECT_ID(N'[dbo].[CarDetail]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CarDetail_CityId]
        ON [dbo].[CarDetail]([CityId]);
    PRINT 'IX_CarDetail_CityId created.';
END
ELSE
    PRINT 'IX_CarDetail_CityId already exists - skipped.';
GO
