SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH(N'dbo.Product', N'IsFeatured') IS NULL
    BEGIN
        ALTER TABLE dbo.Product
        ADD IsFeatured BIT NOT NULL
            CONSTRAINT DF_Product_IsFeatured DEFAULT (0) WITH VALUES;
    END;

    IF COL_LENGTH(N'dbo.Product', N'BadgeType') IS NULL
    BEGIN
        ALTER TABLE dbo.Product
        ADD BadgeType NVARCHAR(20) NULL;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.check_constraints
        WHERE name = N'CK_Product_BadgeType'
          AND parent_object_id = OBJECT_ID(N'dbo.Product')
    )
    BEGIN
        EXEC sys.sp_executesql N'
            ALTER TABLE dbo.Product WITH CHECK
            ADD CONSTRAINT CK_Product_BadgeType
            CHECK
            (
                BadgeType IS NULL
                OR BadgeType IN (N''Featured'', N''BestSeller'', N''New'')
            );

            ALTER TABLE dbo.Product
            CHECK CONSTRAINT CK_Product_BadgeType;';
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
