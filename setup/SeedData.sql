USE BeverageWebsiteDb;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

-- All seed writes commit together and roll back automatically when an error occurs.
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @SeedCategories TABLE
    (
        CategoryName NVARCHAR(100) NOT NULL PRIMARY KEY,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL
    );

    DECLARE @SeedProducts TABLE
    (
        CategoryName NVARCHAR(100) NOT NULL,
        ProductName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,
        Price DECIMAL(12,2) NOT NULL,
        ImageUrl NVARCHAR(500) NULL,
        IsActive BIT NOT NULL,
        StockQuantity INT NOT NULL,
        ReorderLevel INT NOT NULL,
        PRIMARY KEY (CategoryName, ProductName)
    );

    -- Category seeding uses CategoryName as the natural key and inserts only missing rows.
    INSERT INTO @SeedCategories
    (
        CategoryName,
        Description,
        IsActive
    )
    VALUES
        (N'Cà phê', N'Các loại cà phê pha theo phong cách Việt Nam.', 1),
        (N'Trà', N'Các loại trà trái cây và trà hương vị thanh mát.', 1),
        (N'Nước ép', N'Nước ép trái cây tươi dùng để giải khát.', 1),
        (N'Nước ngọt', N'Các loại nước giải khát có ga phổ biến.', 1),
        (N'Sinh tố', N'Sinh tố trái cây xay mịn và giàu dinh dưỡng.', 1);

    INSERT INTO dbo.Category
    (
        CategoryName,
        Description,
        IsActive
    )
    SELECT
        Seed.CategoryName,
        Seed.Description,
        Seed.IsActive
    FROM @SeedCategories AS Seed
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Category AS Existing WITH (UPDLOCK, HOLDLOCK)
        WHERE Existing.CategoryName = Seed.CategoryName
    );

    IF EXISTS
    (
        SELECT 1
        FROM @SeedCategories AS Seed
        INNER JOIN dbo.Category AS Existing
            ON Existing.CategoryName = Seed.CategoryName
        WHERE Existing.IsActive = 0
    )
    BEGIN
        THROW 50001, 'A seed category already exists but is inactive.', 1;
    END;

    -- Product seeding uses CategoryName plus ProductName and leaves ImageUrl null.
    INSERT INTO @SeedProducts
    (
        CategoryName,
        ProductName,
        Description,
        Price,
        ImageUrl,
        IsActive,
        StockQuantity,
        ReorderLevel
    )
    VALUES
        (N'Cà phê', N'Cà phê sữa đá',
         N'Cà phê đậm vị kết hợp sữa đặc và đá lạnh.',
         30000.00, NULL, 1, 80, 15),
        (N'Cà phê', N'Cà phê đen đá',
         N'Cà phê rang xay nguyên chất phục vụ cùng đá lạnh.',
         25000.00, NULL, 1, 70, 15),
        (N'Trà', N'Trà đào cam sả',
         N'Trà thơm kết hợp đào, cam tươi và sả.',
         45000.00, NULL, 1, 60, 12),
        (N'Trà', N'Trà sen vàng',
         N'Trà sen thanh nhẹ với hương vị dịu và hậu vị thơm.',
         42000.00, NULL, 1, 55, 12),
        (N'Nước ép', N'Nước ép cam',
         N'Nước cam tươi có vị chua ngọt tự nhiên.',
         40000.00, NULL, 1, 50, 10),
        (N'Nước ép', N'Nước ép dưa hấu',
         N'Nước ép dưa hấu tươi mát, phù hợp cho ngày nóng.',
         38000.00, NULL, 1, 45, 10),
        (N'Nước ngọt', N'Nước ngọt cola',
         N'Nước giải khát có ga vị cola dùng lạnh.',
         25000.00, NULL, 1, 90, 20),
        (N'Nước ngọt', N'Nước ngọt chanh',
         N'Nước giải khát có ga với hương chanh tươi mát.',
         24000.00, NULL, 1, 85, 20),
        (N'Sinh tố', N'Sinh tố bơ',
         N'Sinh tố bơ béo mịn pha cùng sữa.',
         55000.00, NULL, 1, 40, 8),
        (N'Sinh tố', N'Sinh tố xoài',
         N'Sinh tố xoài chín có vị ngọt thơm tự nhiên.',
         48000.00, NULL, 1, 35, 8);

    INSERT INTO dbo.Product
    (
        CategoryId,
        ProductName,
        Description,
        Price,
        ImageUrl,
        IsActive
    )
    SELECT
        Category.CategoryId,
        Seed.ProductName,
        Seed.Description,
        Seed.Price,
        Seed.ImageUrl,
        Seed.IsActive
    FROM @SeedProducts AS Seed
    INNER JOIN dbo.Category AS Category
        ON Category.CategoryName = Seed.CategoryName
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Product AS Existing WITH (UPDLOCK, HOLDLOCK)
        WHERE Existing.CategoryId = Category.CategoryId
          AND Existing.ProductName = Seed.ProductName
    );

    IF EXISTS
    (
        SELECT 1
        FROM @SeedProducts AS Seed
        INNER JOIN dbo.Category AS Category
            ON Category.CategoryName = Seed.CategoryName
        INNER JOIN dbo.Product AS Existing
            ON Existing.CategoryId = Category.CategoryId
           AND Existing.ProductName = Seed.ProductName
        WHERE Existing.IsActive = 0
           OR Existing.Price <= 0
    )
    BEGIN
        THROW 50002, 'A seed product already exists but is inactive or has a nonpositive price.', 1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM @SeedProducts AS Seed
        INNER JOIN dbo.Category AS Category
            ON Category.CategoryName = Seed.CategoryName
        INNER JOIN dbo.Product AS Existing
            ON Existing.CategoryId = Category.CategoryId
           AND Existing.ProductName = Seed.ProductName
        GROUP BY Existing.CategoryId, Existing.ProductName
        HAVING COUNT_BIG(*) > 1
    )
    BEGIN
        THROW 50003, 'Duplicate seed product names exist within a category.', 1;
    END;

    -- Inventory seeding resolves ProductId by query and inserts one row when absent.
    INSERT INTO dbo.Inventory
    (
        ProductId,
        StockQuantity,
        ReorderLevel,
        LastUpdatedAt
    )
    SELECT
        Product.ProductId,
        Seed.StockQuantity,
        Seed.ReorderLevel,
        SYSUTCDATETIME()
    FROM @SeedProducts AS Seed
    INNER JOIN dbo.Category AS Category
        ON Category.CategoryName = Seed.CategoryName
    INNER JOIN dbo.Product AS Product
        ON Product.CategoryId = Category.CategoryId
       AND Product.ProductName = Seed.ProductName
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Inventory AS Existing WITH (UPDLOCK, HOLDLOCK)
        WHERE Existing.ProductId = Product.ProductId
    );

    -- Preserve positive existing stock; make only zero-stock seed products testable.
    UPDATE Existing
    SET Existing.StockQuantity = Seed.StockQuantity,
        Existing.LastUpdatedAt = SYSUTCDATETIME()
    FROM dbo.Inventory AS Existing
    INNER JOIN dbo.Product AS Product
        ON Product.ProductId = Existing.ProductId
    INNER JOIN dbo.Category AS Category
        ON Category.CategoryId = Product.CategoryId
    INNER JOIN @SeedProducts AS Seed
        ON Seed.CategoryName = Category.CategoryName
       AND Seed.ProductName = Product.ProductName
    WHERE Existing.StockQuantity = 0;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;

-- Read-only verification queries report seeded data and consistency problems.
SELECT
    Category.CategoryId,
    Category.CategoryName,
    Category.Description,
    Category.IsActive
FROM dbo.Category AS Category
INNER JOIN @SeedCategories AS Seed
    ON Seed.CategoryName = Category.CategoryName
ORDER BY Category.CategoryId;

SELECT
    Product.ProductId,
    Product.ProductName,
    Product.Description,
    Product.Price,
    Product.ImageUrl,
    Product.IsActive,
    Category.CategoryId,
    Category.CategoryName
FROM dbo.Product AS Product
INNER JOIN dbo.Category AS Category
    ON Category.CategoryId = Product.CategoryId
INNER JOIN @SeedProducts AS Seed
    ON Seed.CategoryName = Category.CategoryName
   AND Seed.ProductName = Product.ProductName
ORDER BY Category.CategoryId, Product.ProductId;

SELECT
    Product.ProductId,
    Product.ProductName,
    Inventory.InventoryId,
    Inventory.StockQuantity,
    Inventory.ReorderLevel,
    Inventory.LastUpdatedAt
FROM dbo.Product AS Product
INNER JOIN dbo.Category AS Category
    ON Category.CategoryId = Product.CategoryId
INNER JOIN @SeedProducts AS Seed
    ON Seed.CategoryName = Category.CategoryName
   AND Seed.ProductName = Product.ProductName
INNER JOIN dbo.Inventory AS Inventory
    ON Inventory.ProductId = Product.ProductId
ORDER BY Product.ProductId;

SELECT
    COUNT_BIG(*) AS SeededCategoryCount
FROM @SeedCategories AS Seed
WHERE EXISTS
(
    SELECT 1
    FROM dbo.Category AS Category
    WHERE Category.CategoryName = Seed.CategoryName
);

SELECT
    COUNT_BIG(*) AS SeededProductCount
FROM @SeedProducts AS Seed
WHERE EXISTS
(
    SELECT 1
    FROM dbo.Category AS Category
    INNER JOIN dbo.Product AS Product
        ON Product.CategoryId = Category.CategoryId
    WHERE Category.CategoryName = Seed.CategoryName
      AND Product.ProductName = Seed.ProductName
);

SELECT
    Product.ProductId,
    Product.ProductName,
    Category.CategoryId,
    Category.CategoryName
FROM dbo.Product AS Product
INNER JOIN dbo.Category AS Category
    ON Category.CategoryId = Product.CategoryId
INNER JOIN @SeedProducts AS Seed
    ON Seed.CategoryName = Category.CategoryName
   AND Seed.ProductName = Product.ProductName
LEFT JOIN dbo.Inventory AS Inventory
    ON Inventory.ProductId = Product.ProductId
WHERE Inventory.InventoryId IS NULL
ORDER BY Product.ProductId;

SELECT
    Category.CategoryId,
    Category.CategoryName,
    Product.ProductName,
    COUNT_BIG(*) AS DuplicateCount
FROM dbo.Product AS Product
INNER JOIN dbo.Category AS Category
    ON Category.CategoryId = Product.CategoryId
INNER JOIN @SeedProducts AS Seed
    ON Seed.CategoryName = Category.CategoryName
   AND Seed.ProductName = Product.ProductName
GROUP BY
    Category.CategoryId,
    Category.CategoryName,
    Product.ProductName
HAVING COUNT_BIG(*) > 1
ORDER BY Category.CategoryId, Product.ProductName;
