USE BeverageWebsiteDb;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @OfficialCategories TABLE
(
    CategoryName NVARCHAR(100) NOT NULL PRIMARY KEY,
    Description NVARCHAR(500) NOT NULL
);

DECLARE @OfficialProducts TABLE
(
    CategoryName NVARCHAR(100) NOT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NOT NULL,
    Price DECIMAL(12,2) NOT NULL,
    PRIMARY KEY (CategoryName, ProductName)
);

DECLARE @TemporaryProducts TABLE
(
    CategoryName NVARCHAR(100) NOT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    PRIMARY KEY (CategoryName, ProductName)
);

DECLARE @ObsoleteCategories TABLE
(
    CategoryName NVARCHAR(100) NOT NULL PRIMARY KEY
);

INSERT INTO @OfficialCategories
(
    CategoryName,
    Description
)
VALUES
    (N'Cà phê', N'Các thức uống từ cà phê, từ espresso nguyên bản đến các phiên bản lạnh và đặc trưng.'),
    (N'Matcha', N'Các thức uống matcha từ nguyên bản đến những phiên bản kết hợp sữa, trái cây và chocolate.'),
    (N'Cacao', N'Các thức uống cacao nóng, lạnh và những phiên bản kết hợp hương vị sáng tạo.'),
    (N'Trà', N'Các thức uống từ trà và trà trái cây với hương vị thanh mát.');

INSERT INTO @OfficialProducts
(
    CategoryName,
    ProductName,
    Description,
    Price
)
VALUES
    (N'Cà phê', N'Cà phê Espresso',
     N'Một shot espresso cô đọng, hương rang rõ nét, vị đậm và hậu vị kéo dài.',
     39000.00),
    (N'Cà phê', N'Cappuccino',
     N'Espresso kết hợp sữa và lớp bọt mịn, cân bằng giữa vị cà phê đậm và độ béo dịu.',
     49000.00),
    (N'Cà phê', N'Cold Brew Chanh',
     N'Cold brew ủ lạnh hòa cùng chanh, thanh mát, chua nhẹ và sảng khoái.',
     55000.00),
    (N'Cà phê', N'Cà phê Pour Over',
     N'Cà phê pha thủ công giúp thể hiện rõ hương thơm và đặc tính riêng của từng loại hạt.',
     65000.00),
    (N'Cà phê', N'Cà phê Muối',
     N'Cà phê đậm kết hợp kem muối béo mịn, cân bằng giữa đắng, ngọt, béo và mặn nhẹ.',
     49000.00),
    (N'Matcha', N'Matcha Latte',
     N'Matcha thơm trà hòa cùng sữa, mịn, béo dịu và dễ uống.',
     55000.00),
    (N'Matcha', N'Matcha Dừa',
     N'Matcha kết hợp dừa nhiệt đới, thơm trà, béo nhẹ và thanh mát.',
     59000.00),
    (N'Matcha', N'Matcha Kem Dâu',
     N'Matcha kết hợp dâu chua ngọt và lớp cream mềm mịn, nhiều tầng hương vị.',
     65000.00),
    (N'Matcha', N'Matcha Caramel',
     N'Matcha kết hợp caramel thơm ngọt, béo dịu và dễ tiếp cận.',
     59000.00),
    (N'Matcha', N'Matcha Latte Xoài',
     N'Matcha và xoài nhiệt đới tạo vị trà thanh, trái cây ngọt dịu và màu sắc nổi bật.',
     65000.00),
    (N'Matcha', N'Matcha Chocolate',
     N'Matcha kết hợp chocolate, cân bằng giữa vị trà xanh, cacao và độ béo của sữa.',
     59000.00),
    (N'Matcha', N'Matcha Classic',
     N'Phiên bản tập trung vào vị matcha nguyên bản, thanh, umami và hơi đắng nhẹ.',
     49000.00),
    (N'Cacao', N'Cacao Nóng',
     N'Cacao nóng thơm đậm, mịn và ấm, kết hợp cùng sữa tạo vị béo dịu.',
     49000.00),
    (N'Cacao', N'Cacao Đá',
     N'Cacao lạnh đậm vị chocolate, béo nhẹ và mát lạnh.',
     49000.00),
    (N'Cacao', N'Cacao Chocolate Bạc Hà',
     N'Cacao chocolate kết hợp bạc hà mát, tạo hậu vị tươi và khác biệt.',
     55000.00),
    (N'Trà', N'Hồng Trà Đào Cam Sả',
     N'Hồng trà kết hợp đào, cam và sả, thơm trái cây, chua ngọt nhẹ và rất sảng khoái.',
     55000.00),
    (N'Trà', N'Trà Nhài Ổi Hồng Dâu Tây',
     N'Trà nhài thanh thơm kết hợp ổi hồng và dâu tây, chua ngọt, floral và tươi mát.',
     59000.00),
    (N'Trà', N'Trà Ô Long Macchiato',
     N'Trà ô long thơm sâu kết hợp lớp macchiato béo mịn, cân bằng giữa vị trà và cream.',
     55000.00),
    (N'Trà', N'Trà Hibiscus Dừa',
     N'Hibiscus chua thanh kết hợp hương dừa nhiệt đới, tươi mát và nổi bật về màu sắc.',
     59000.00);

INSERT INTO @TemporaryProducts
(
    CategoryName,
    ProductName
)
VALUES
    (N'Cà phê', N'Cà phê sữa đá'),
    (N'Cà phê', N'Cà phê đen đá'),
    (N'Trà', N'Trà đào cam sả'),
    (N'Trà', N'Trà sen vàng'),
    (N'Nước ép', N'Nước ép cam'),
    (N'Nước ép', N'Nước ép dưa hấu'),
    (N'Nước ngọt', N'Nước ngọt cola'),
    (N'Nước ngọt', N'Nước ngọt chanh'),
    (N'Sinh tố', N'Sinh tố bơ'),
    (N'Sinh tố', N'Sinh tố xoài');

INSERT INTO @ObsoleteCategories
(
    CategoryName
)
VALUES
    (N'Nước ép'),
    (N'Nước ngọt'),
    (N'Sinh tố');

BEGIN TRY
    BEGIN TRANSACTION;

    UPDATE Category
    SET Description = Official.Description,
        IsActive = 1
    FROM dbo.Category AS Category WITH (UPDLOCK, HOLDLOCK)
    INNER JOIN @OfficialCategories AS Official
        ON Official.CategoryName = Category.CategoryName;

    INSERT INTO dbo.Category
    (
        CategoryName,
        Description,
        IsActive
    )
    SELECT
        Official.CategoryName,
        Official.Description,
        1
    FROM @OfficialCategories AS Official
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Category AS Category WITH (UPDLOCK, HOLDLOCK)
        WHERE Category.CategoryName = Official.CategoryName
    );

    -- Validate the product natural key before choosing any existing row to update.
    IF EXISTS
    (
        SELECT 1
        FROM @OfficialProducts AS Official
        INNER JOIN dbo.Category AS Category
            ON Category.CategoryName = Official.CategoryName
        INNER JOIN dbo.Product AS Product WITH (UPDLOCK, HOLDLOCK)
            ON Product.CategoryId = Category.CategoryId
           AND Product.ProductName = Official.ProductName
        GROUP BY Official.CategoryName, Official.ProductName
        HAVING COUNT_BIG(*) > 1
    )
    BEGIN
        THROW 50001, 'Duplicate rows match an official CategoryName and ProductName before the upsert.', 1;
    END;

    -- ImageUrl and CreatedAt are intentionally absent so existing values are preserved.
    UPDATE Product
    SET Description = Official.Description,
        Price = Official.Price,
        IsActive = 1
    FROM dbo.Product AS Product
    INNER JOIN dbo.Category AS Category
        ON Category.CategoryId = Product.CategoryId
    INNER JOIN @OfficialProducts AS Official
        ON Official.CategoryName = Category.CategoryName
       AND Official.ProductName = Product.ProductName;

    -- New products use the CreatedAt default and have no image until the UI stage.
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
        Official.ProductName,
        Official.Description,
        Official.Price,
        NULL,
        1
    FROM @OfficialProducts AS Official
    INNER JOIN dbo.Category AS Category
        ON Category.CategoryName = Official.CategoryName
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Product AS Product WITH (UPDLOCK, HOLDLOCK)
        WHERE Product.CategoryId = Category.CategoryId
          AND Product.ProductName = Official.ProductName
    );

    UPDATE Product
    SET IsActive = 0
    FROM dbo.Product AS Product
    INNER JOIN dbo.Category AS Category
        ON Category.CategoryId = Product.CategoryId
    INNER JOIN @TemporaryProducts AS Temporary
        ON Temporary.CategoryName = Category.CategoryName
       AND Temporary.ProductName = Product.ProductName;

    UPDATE Category
    SET IsActive = 0
    FROM dbo.Category AS Category
    INNER JOIN @ObsoleteCategories AS Obsolete
        ON Obsolete.CategoryName = Category.CategoryName
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Product AS Product
        WHERE Product.CategoryId = Category.CategoryId
          AND Product.IsActive = 1
    );

    INSERT INTO dbo.Inventory
    (
        ProductId,
        StockQuantity,
        ReorderLevel,
        LastUpdatedAt
    )
    SELECT
        Product.ProductId,
        50,
        10,
        SYSUTCDATETIME()
    FROM @OfficialProducts AS Official
    INNER JOIN dbo.Category AS Category
        ON Category.CategoryName = Official.CategoryName
    INNER JOIN dbo.Product AS Product
        ON Product.CategoryId = Category.CategoryId
       AND Product.ProductName = Official.ProductName
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Inventory AS Inventory WITH (UPDLOCK, HOLDLOCK)
        WHERE Inventory.ProductId = Product.ProductId
    );

    UPDATE Inventory
    SET StockQuantity =
            CASE WHEN Inventory.StockQuantity = 0 THEN 50 ELSE Inventory.StockQuantity END,
        ReorderLevel =
            CASE WHEN Inventory.ReorderLevel = 0 THEN 10 ELSE Inventory.ReorderLevel END,
        LastUpdatedAt =
            CASE
                WHEN Inventory.StockQuantity = 0 THEN SYSUTCDATETIME()
                ELSE Inventory.LastUpdatedAt
            END
    FROM dbo.Inventory AS Inventory
    INNER JOIN dbo.Product AS Product
        ON Product.ProductId = Inventory.ProductId
    INNER JOIN dbo.Category AS Category
        ON Category.CategoryId = Product.CategoryId
    INNER JOIN @OfficialProducts AS Official
        ON Official.CategoryName = Category.CategoryName
       AND Official.ProductName = Product.ProductName
    WHERE Inventory.StockQuantity = 0
       OR Inventory.ReorderLevel = 0;

    IF
    (
        SELECT COUNT_BIG(*)
        FROM @OfficialCategories AS Official
        INNER JOIN dbo.Category AS Category
            ON Category.CategoryName = Official.CategoryName
           AND Category.IsActive = 1
    ) <> 4
    BEGIN
        THROW 50002, 'Official category validation failed: four active categories are required.', 1;
    END;

    IF
    (
        SELECT COUNT_BIG(*)
        FROM @OfficialProducts AS Official
        INNER JOIN dbo.Category AS Category
            ON Category.CategoryName = Official.CategoryName
        INNER JOIN dbo.Product AS Product
            ON Product.CategoryId = Category.CategoryId
           AND Product.ProductName = Official.ProductName
    ) <> 19
    BEGIN
        THROW 50003, 'Official product validation failed: exactly 19 natural-key matches are required.', 1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM @OfficialProducts AS Official
        INNER JOIN dbo.Category AS Category
            ON Category.CategoryName = Official.CategoryName
        INNER JOIN dbo.Product AS Product
            ON Product.CategoryId = Category.CategoryId
           AND Product.ProductName = Official.ProductName
        WHERE Product.IsActive = 0
    )
    BEGIN
        THROW 50004, 'Official product validation failed: an official product is inactive.', 1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM @OfficialProducts AS Official
        INNER JOIN dbo.Category AS Category
            ON Category.CategoryName = Official.CategoryName
        INNER JOIN dbo.Product AS Product
            ON Product.CategoryId = Category.CategoryId
           AND Product.ProductName = Official.ProductName
        WHERE Product.Price <= 0
    )
    BEGIN
        THROW 50005, 'Official product validation failed: an official product has a nonpositive price.', 1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM @OfficialProducts AS Official
        INNER JOIN dbo.Category AS Category
            ON Category.CategoryName = Official.CategoryName
        INNER JOIN dbo.Product AS Product
            ON Product.CategoryId = Category.CategoryId
           AND Product.ProductName = Official.ProductName
        LEFT JOIN dbo.Inventory AS Inventory
            ON Inventory.ProductId = Product.ProductId
        GROUP BY Product.ProductId
        HAVING COUNT(Inventory.InventoryId) <> 1
    )
    BEGIN
        THROW 50006, 'Official inventory validation failed: every official product requires one inventory row.', 1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM @OfficialProducts AS Official
        INNER JOIN dbo.Category AS Category
            ON Category.CategoryName = Official.CategoryName
        INNER JOIN dbo.Product AS Product
            ON Product.CategoryId = Category.CategoryId
           AND Product.ProductName = Official.ProductName
        GROUP BY Official.CategoryName, Official.ProductName
        HAVING COUNT_BIG(*) > 1
    )
    BEGIN
        THROW 50007, 'Duplicate rows match an official CategoryName and ProductName after the upsert.', 1;
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;

-- Query 1: official categories.
SELECT
    Category.CategoryId,
    Category.CategoryName,
    Category.Description,
    Category.IsActive
FROM dbo.Category AS Category
INNER JOIN @OfficialCategories AS Official
    ON Official.CategoryName = Category.CategoryName
ORDER BY Category.CategoryName;

-- Query 2: all 19 official products.
SELECT
    Product.ProductId,
    Category.CategoryName,
    Product.ProductName,
    Product.Description,
    Product.Price,
    Product.ImageUrl,
    Product.IsActive
FROM dbo.Product AS Product
INNER JOIN dbo.Category AS Category
    ON Category.CategoryId = Product.CategoryId
INNER JOIN @OfficialProducts AS Official
    ON Official.CategoryName = Category.CategoryName
   AND Official.ProductName = Product.ProductName
ORDER BY Category.CategoryName, Product.ProductName;

-- Query 3: inventory for all 19 official products.
SELECT
    Product.ProductId,
    Product.ProductName,
    Inventory.StockQuantity,
    Inventory.ReorderLevel,
    Inventory.LastUpdatedAt
FROM dbo.Product AS Product
INNER JOIN dbo.Category AS Category
    ON Category.CategoryId = Product.CategoryId
INNER JOIN @OfficialProducts AS Official
    ON Official.CategoryName = Category.CategoryName
   AND Official.ProductName = Product.ProductName
INNER JOIN dbo.Inventory AS Inventory
    ON Inventory.ProductId = Product.ProductId
ORDER BY Product.ProductName;

-- Query 4: expected counts are 4, 19 and 19.
SELECT
    (
        SELECT COUNT_BIG(*)
        FROM dbo.Category AS Category
        INNER JOIN @OfficialCategories AS Official
            ON Official.CategoryName = Category.CategoryName
    ) AS OfficialCategoryCount,
    (
        SELECT COUNT_BIG(*)
        FROM dbo.Product AS Product
        INNER JOIN dbo.Category AS Category
            ON Category.CategoryId = Product.CategoryId
        INNER JOIN @OfficialProducts AS Official
            ON Official.CategoryName = Category.CategoryName
           AND Official.ProductName = Product.ProductName
    ) AS OfficialProductCount,
    (
        SELECT COUNT_BIG(*)
        FROM dbo.Inventory AS Inventory
        INNER JOIN dbo.Product AS Product
            ON Product.ProductId = Inventory.ProductId
        INNER JOIN dbo.Category AS Category
            ON Category.CategoryId = Product.CategoryId
        INNER JOIN @OfficialProducts AS Official
            ON Official.CategoryName = Category.CategoryName
           AND Official.ProductName = Product.ProductName
    ) AS OfficialInventoryCount;

-- Query 5: duplicate official products; expected zero rows.
SELECT
    Category.CategoryName,
    Product.ProductName,
    COUNT_BIG(*) AS DuplicateCount
FROM dbo.Product AS Product
INNER JOIN dbo.Category AS Category
    ON Category.CategoryId = Product.CategoryId
INNER JOIN @OfficialProducts AS Official
    ON Official.CategoryName = Category.CategoryName
   AND Official.ProductName = Product.ProductName
GROUP BY Category.CategoryName, Product.ProductName
HAVING COUNT_BIG(*) > 1
ORDER BY Category.CategoryName, Product.ProductName;

-- Query 6: official products missing inventory; expected zero rows.
SELECT
    Product.ProductId,
    Category.CategoryName,
    Product.ProductName
FROM dbo.Product AS Product
INNER JOIN dbo.Category AS Category
    ON Category.CategoryId = Product.CategoryId
INNER JOIN @OfficialProducts AS Official
    ON Official.CategoryName = Category.CategoryName
   AND Official.ProductName = Product.ProductName
LEFT JOIN dbo.Inventory AS Inventory
    ON Inventory.ProductId = Product.ProductId
WHERE Inventory.InventoryId IS NULL
ORDER BY Category.CategoryName, Product.ProductName;

-- Query 7: invalid official product state; expected zero rows.
SELECT
    Product.ProductId,
    Category.CategoryName,
    Product.ProductName,
    Product.Price,
    Product.IsActive
FROM dbo.Product AS Product
INNER JOIN dbo.Category AS Category
    ON Category.CategoryId = Product.CategoryId
INNER JOIN @OfficialProducts AS Official
    ON Official.CategoryName = Category.CategoryName
   AND Official.ProductName = Product.ProductName
WHERE Product.IsActive = 0
   OR Product.Price <= 0
ORDER BY Category.CategoryName, Product.ProductName;

-- Query 8: old temporary products still active; expected zero rows.
SELECT
    Product.ProductId,
    Category.CategoryName,
    Product.ProductName,
    Product.IsActive
FROM dbo.Product AS Product
INNER JOIN dbo.Category AS Category
    ON Category.CategoryId = Product.CategoryId
INNER JOIN @TemporaryProducts AS Temporary
    ON Temporary.CategoryName = Category.CategoryName
   AND Temporary.ProductName = Product.ProductName
WHERE Product.IsActive = 1
ORDER BY Category.CategoryName, Product.ProductName;
