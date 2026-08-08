-- Creates the DealerRequest table used by the "Request for Seller" form.
-- Run this once against the Azure SQL database (SSMS or the portal Query editor).
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DealerRequest')
BEGIN
    CREATE TABLE [dbo].[DealerRequest]
    (
        [ID]            INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserDetailId]  INT            NOT NULL,
        [FullName]      NVARCHAR(200)  NOT NULL,
        [PrimaryMobile] NVARCHAR(20)   NOT NULL,
        [Email]         NVARCHAR(100)  NULL,
        [CompanyName]   NVARCHAR(500)  NULL,
        [Address1]      NVARCHAR(200)  NULL,
        [City]          NVARCHAR(100)  NULL,
        [State]         NVARCHAR(100)  NULL,
        [Pincode]       NVARCHAR(20)   NULL,
        [Status]        NVARCHAR(50)   NOT NULL CONSTRAINT [DF_DealerRequest_Status]  DEFAULT ('Pending'),
        [CreatedDate]   DATETIME       NOT NULL CONSTRAINT [DF_DealerRequest_Created] DEFAULT (GETDATE())
    );
END
