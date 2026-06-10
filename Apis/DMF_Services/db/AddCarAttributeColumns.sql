-- =============================================================
-- Adds the remaining listing attributes to CarDetail so they can be
-- persisted from the Add-Car form and shown on the detail page:
--   BodyType, IsNegotiable, ReverseCamera, Sunroof
-- Idempotent: safe to run multiple times.
-- =============================================================

IF NOT EXISTS (SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[CarDetail]') AND name = 'BodyType')
BEGIN
    ALTER TABLE [dbo].[CarDetail] ADD [BodyType] VARCHAR(50) NULL;
    PRINT 'CarDetail.BodyType added.';
END
ELSE PRINT 'CarDetail.BodyType already exists - skipped.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[CarDetail]') AND name = 'IsNegotiable')
BEGIN
    ALTER TABLE [dbo].[CarDetail] ADD [IsNegotiable] BIT NULL;
    PRINT 'CarDetail.IsNegotiable added.';
END
ELSE PRINT 'CarDetail.IsNegotiable already exists - skipped.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[CarDetail]') AND name = 'ReverseCamera')
BEGIN
    ALTER TABLE [dbo].[CarDetail] ADD [ReverseCamera] BIT NULL;
    PRINT 'CarDetail.ReverseCamera added.';
END
ELSE PRINT 'CarDetail.ReverseCamera already exists - skipped.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[CarDetail]') AND name = 'Sunroof')
BEGIN
    ALTER TABLE [dbo].[CarDetail] ADD [Sunroof] BIT NULL;
    PRINT 'CarDetail.Sunroof added.';
END
ELSE PRINT 'CarDetail.Sunroof already exists - skipped.';
GO
