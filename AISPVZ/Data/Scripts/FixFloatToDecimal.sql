-- =====================================================
-- Fix legacy FLOAT/REAL columns to DECIMAL(18,2)
-- Run this in SSMS if you see "Unable to cast System.Double to System.Decimal"
-- =====================================================

USE AISPVZ_DB;
GO

SET XACT_ABORT ON;
GO

-- Helper procedure: fix a single column
CREATE OR ALTER PROCEDURE dbo.FixColumnToDecimal
    @TableName NVARCHAR(128),
    @ColumnName NVARCHAR(128),
    @NewType NVARCHAR(50) = 'DECIMAL(18,2)'
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM sys.columns c
        JOIN sys.types t ON c.user_type_id = t.user_type_id
        WHERE c.object_id = OBJECT_ID(@TableName)
          AND c.name = @ColumnName
          AND t.name IN ('float', 'real')
    )
    BEGIN
        PRINT @TableName + '.' + @ColumnName + ' is already correct or does not exist.';
        RETURN;
    END

    DECLARE @sql NVARCHAR(MAX);

    -- Drop default constraint if any
    SELECT @sql = 'ALTER TABLE ' + QUOTENAME(@TableName) + ' DROP CONSTRAINT ' + QUOTENAME(d.name)
    FROM sys.default_constraints d
    JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
    WHERE c.object_id = OBJECT_ID(@TableName) AND c.name = @ColumnName;

    IF @sql IS NOT NULL
    BEGIN
        PRINT 'Dropping default: ' + @sql;
        EXEC sp_executesql @sql;
    END

    -- Alter column
    SET @sql = 'ALTER TABLE ' + QUOTENAME(@TableName) + ' ALTER COLUMN ' + QUOTENAME(@ColumnName) + ' ' + @NewType + ' NOT NULL;';
    PRINT 'Altering: ' + @sql;
    EXEC sp_executesql @sql;

    -- Re-add default 0
    SET @sql = 'ALTER TABLE ' + QUOTENAME(@TableName) + ' ADD DEFAULT 0 FOR ' + QUOTENAME(@ColumnName) + ';';
    BEGIN TRY
        PRINT 'Re-adding default: ' + @sql;
        EXEC sp_executesql @sql;
    END TRY
    BEGIN CATCH
        PRINT 'Could not re-add default (non-critical): ' + ERROR_MESSAGE();
    END CATCH

    PRINT @TableName + '.' + @ColumnName + ' fixed to ' + @NewType + '.';
END
GO

-- Run fixes
EXEC dbo.FixColumnToDecimal 'OrderItems', 'Price';
EXEC dbo.FixColumnToDecimal 'IssueOperations', 'TotalAmount';
GO

PRINT '==================================================';
PRINT 'Done. Please restart the application.';
PRINT '==================================================';
GO
