USE master;
GO

IF DB_ID(N'BeverageWebsiteDb') IS NULL
BEGIN
    CREATE DATABASE BeverageWebsiteDb;
END;
GO

USE BeverageWebsiteDb;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.[User]', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.[User]
    (
        UserId INT IDENTITY(1,1) NOT NULL,
        UserName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        PasswordHash NVARCHAR(255) NOT NULL,
        FullName NVARCHAR(200) NULL,
        Phone NVARCHAR(20) NULL,
        Role NVARCHAR(20) NOT NULL CONSTRAINT DF_User_Role DEFAULT 'Customer',
        IsActive BIT NOT NULL CONSTRAINT DF_User_IsActive DEFAULT 1,
        CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_User_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_User PRIMARY KEY CLUSTERED (UserId ASC),
        CONSTRAINT CK_User_Role CHECK (Role IN ('Admin', 'Customer', 'Staff')),
        CONSTRAINT UQ_User_UserName UNIQUE (UserName),
        CONSTRAINT UQ_User_Email UNIQUE (Email)
    );

    EXEC sys.sp_addextendedproperty
        @name = N'MS_Description',
        @value = N'Quản lý tài khoản người dùng, phân biệt khách hàng và quản trị viên.',
        @level0type = N'SCHEMA',
        @level0name = N'dbo',
        @level1type = N'TABLE',
        @level1name = N'User';
END;
GO

IF OBJECT_ID(N'dbo.Address', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Address
    (
        AddressId INT IDENTITY(1,1) NOT NULL,
        UserId INT NOT NULL,
        RecipientName NVARCHAR(200) NOT NULL,
        Phone NVARCHAR(20) NOT NULL,
        Street NVARCHAR(255) NOT NULL,
        Ward NVARCHAR(100) NULL,
        District NVARCHAR(100) NULL,
        City NVARCHAR(100) NOT NULL,
        IsDefault BIT NOT NULL CONSTRAINT DF_Address_IsDefault DEFAULT 0,
        CONSTRAINT PK_Address PRIMARY KEY CLUSTERED (AddressId ASC),
        CONSTRAINT FK_Address_User FOREIGN KEY (UserId) REFERENCES dbo.[User](UserId) ON DELETE NO ACTION ON UPDATE NO ACTION
    );

    EXEC sys.sp_addextendedproperty
        @name = N'MS_Description',
        @value = N'Lưu trữ địa chỉ nhận hàng của người dùng.',
        @level0type = N'SCHEMA',
        @level0name = N'dbo',
        @level1type = N'TABLE',
        @level1name = N'Address';
END;
GO

IF OBJECT_ID(N'dbo.Category', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Category
    (
        CategoryId INT IDENTITY(1,1) NOT NULL,
        CategoryName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Category_IsActive DEFAULT 1,
        CONSTRAINT PK_Category PRIMARY KEY CLUSTERED (CategoryId ASC),
        CONSTRAINT UQ_Category_CategoryName UNIQUE (CategoryName)
    );

    EXEC sys.sp_addextendedproperty
        @name = N'MS_Description',
        @value = N'Phân nhóm các sản phẩm như cà phê, trà, nước ép và đồ uống khác.',
        @level0type = N'SCHEMA',
        @level0name = N'dbo',
        @level1type = N'TABLE',
        @level1name = N'Category';
END;
GO

IF OBJECT_ID(N'dbo.Product', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Product
    (
        ProductId INT IDENTITY(1,1) NOT NULL,
        CategoryId INT NOT NULL,
        ProductName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,
        Price DECIMAL(12,2) NOT NULL,
        ImageUrl NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Product_IsActive DEFAULT 1,
        CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_Product_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Product PRIMARY KEY CLUSTERED (ProductId ASC),
        CONSTRAINT FK_Product_Category FOREIGN KEY (CategoryId) REFERENCES dbo.Category(CategoryId) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT CK_Product_Price CHECK (Price >= 0)
    );

    EXEC sys.sp_addextendedproperty
        @name = N'MS_Description',
        @value = N'Quản lý thông tin sản phẩm được bán trên website.',
        @level0type = N'SCHEMA',
        @level0name = N'dbo',
        @level1type = N'TABLE',
        @level1name = N'Product';
END;
GO

IF OBJECT_ID(N'dbo.Promotion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Promotion
    (
        PromotionId INT IDENTITY(1,1) NOT NULL,
        PromotionCode NVARCHAR(50) NOT NULL,
        PromotionName NVARCHAR(200) NOT NULL,
        DiscountType NVARCHAR(50) NOT NULL,
        DiscountValue DECIMAL(12,2) NOT NULL,
        StartDate DATETIME2(7) NOT NULL,
        EndDate DATETIME2(7) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Promotion_IsActive DEFAULT 1,
        CONSTRAINT PK_Promotion PRIMARY KEY CLUSTERED (PromotionId ASC),
        CONSTRAINT CK_Promotion_DiscountValue CHECK (DiscountValue >= 0),
        CONSTRAINT CK_Promotion_DateRange CHECK (EndDate >= StartDate),
        CONSTRAINT UQ_Promotion_Code UNIQUE (PromotionCode)
    );

    EXEC sys.sp_addextendedproperty
        @name = N'MS_Description',
        @value = N'Quản lý chương trình khuyến mãi và mã giảm giá.',
        @level0type = N'SCHEMA',
        @level0name = N'dbo',
        @level1type = N'TABLE',
        @level1name = N'Promotion';
END;
GO

IF OBJECT_ID(N'dbo.Inventory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Inventory
    (
        InventoryId INT IDENTITY(1,1) NOT NULL,
        ProductId INT NOT NULL,
        StockQuantity INT NOT NULL CONSTRAINT DF_Inventory_StockQuantity DEFAULT 0,
        ReorderLevel INT NOT NULL CONSTRAINT DF_Inventory_ReorderLevel DEFAULT 0,
        LastUpdatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_Inventory_LastUpdatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Inventory PRIMARY KEY CLUSTERED (InventoryId ASC),
        CONSTRAINT FK_Inventory_Product FOREIGN KEY (ProductId) REFERENCES dbo.Product(ProductId) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT UQ_Inventory_ProductId UNIQUE (ProductId),
        CONSTRAINT CK_Inventory_StockQuantity CHECK (StockQuantity >= 0),
        CONSTRAINT CK_Inventory_ReorderLevel CHECK (ReorderLevel >= 0)
    );

    EXEC sys.sp_addextendedproperty
        @name = N'MS_Description',
        @value = N'Theo dõi số lượng tồn kho cho từng sản phẩm.',
        @level0type = N'SCHEMA',
        @level0name = N'dbo',
        @level1type = N'TABLE',
        @level1name = N'Inventory';
END;
GO

IF OBJECT_ID(N'dbo.Cart', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cart
    (
        CartId INT IDENTITY(1,1) NOT NULL,
        UserId INT NOT NULL,
        CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_Cart_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_Cart_UpdatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Cart PRIMARY KEY CLUSTERED (CartId ASC),
        CONSTRAINT FK_Cart_User FOREIGN KEY (UserId) REFERENCES dbo.[User](UserId) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT UQ_Cart_UserId UNIQUE (UserId)
    );

    EXEC sys.sp_addextendedproperty
        @name = N'MS_Description',
        @value = N'Giữ sản phẩm khách hàng chọn trước khi thanh toán.',
        @level0type = N'SCHEMA',
        @level0name = N'dbo',
        @level1type = N'TABLE',
        @level1name = N'Cart';
END;
GO

IF OBJECT_ID(N'dbo.CartItem', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CartItem
    (
        CartItemId INT IDENTITY(1,1) NOT NULL,
        CartId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(12,2) NOT NULL,
        CONSTRAINT PK_CartItem PRIMARY KEY CLUSTERED (CartItemId ASC),
        CONSTRAINT FK_CartItem_Cart FOREIGN KEY (CartId) REFERENCES dbo.Cart(CartId) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT FK_CartItem_Product FOREIGN KEY (ProductId) REFERENCES dbo.Product(ProductId) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT CK_CartItem_Quantity CHECK (Quantity > 0),
        CONSTRAINT CK_CartItem_UnitPrice CHECK (UnitPrice >= 0)
    );

    EXEC sys.sp_addextendedproperty
        @name = N'MS_Description',
        @value = N'Bảng chi tiết các sản phẩm trong giỏ hàng.',
        @level0type = N'SCHEMA',
        @level0name = N'dbo',
        @level1type = N'TABLE',
        @level1name = N'CartItem';
END;
GO

IF OBJECT_ID(N'dbo.[Order]', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.[Order]
    (
        OrderId INT IDENTITY(1,1) NOT NULL,
        UserId INT NOT NULL,
        AddressId INT NOT NULL,
        PromotionId INT NULL,
        OrderDate DATETIME2(7) NOT NULL CONSTRAINT DF_Order_OrderDate DEFAULT SYSUTCDATETIME(),
        OrderStatus NVARCHAR(50) NOT NULL CONSTRAINT DF_Order_OrderStatus DEFAULT 'Pending',
        TotalAmount DECIMAL(12,2) NOT NULL CONSTRAINT DF_Order_TotalAmount DEFAULT 0,
        ShippingFee DECIMAL(12,2) NOT NULL CONSTRAINT DF_Order_ShippingFee DEFAULT 0,
        FinalAmount DECIMAL(12,2) NOT NULL CONSTRAINT DF_Order_FinalAmount DEFAULT 0,
        CONSTRAINT PK_Order PRIMARY KEY CLUSTERED (OrderId ASC),
        CONSTRAINT FK_Order_User FOREIGN KEY (UserId) REFERENCES dbo.[User](UserId) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT FK_Order_Address FOREIGN KEY (AddressId) REFERENCES dbo.Address(AddressId) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT FK_Order_Promotion FOREIGN KEY (PromotionId) REFERENCES dbo.Promotion(PromotionId) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT CK_Order_Status CHECK (OrderStatus IN ('Pending', 'Confirmed', 'Processing', 'Completed', 'Cancelled')),
        CONSTRAINT CK_Order_Amounts CHECK (TotalAmount >= 0 AND ShippingFee >= 0 AND FinalAmount >= 0)
    );

    EXEC sys.sp_addextendedproperty
        @name = N'MS_Description',
        @value = N'Lưu trữ thông tin tổng thể của từng đơn hàng.',
        @level0type = N'SCHEMA',
        @level0name = N'dbo',
        @level1type = N'TABLE',
        @level1name = N'Order';
END;
GO

IF OBJECT_ID(N'dbo.OrderItem', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItem
    (
        OrderItemId INT IDENTITY(1,1) NOT NULL,
        OrderId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(12,2) NOT NULL,
        DiscountAmount DECIMAL(12,2) NOT NULL CONSTRAINT DF_OrderItem_DiscountAmount DEFAULT 0,
        LineTotal DECIMAL(12,2) NOT NULL,
        CONSTRAINT PK_OrderItem PRIMARY KEY CLUSTERED (OrderItemId ASC),
        CONSTRAINT FK_OrderItem_Order FOREIGN KEY (OrderId) REFERENCES dbo.[Order](OrderId) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT FK_OrderItem_Product FOREIGN KEY (ProductId) REFERENCES dbo.Product(ProductId) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT CK_OrderItem_Quantity CHECK (Quantity > 0),
        CONSTRAINT CK_OrderItem_UnitPrice CHECK (UnitPrice >= 0),
        CONSTRAINT CK_OrderItem_LineTotal CHECK (LineTotal >= 0)
    );

    EXEC sys.sp_addextendedproperty
        @name = N'MS_Description',
        @value = N'Chi tiết từng sản phẩm trong một đơn hàng.',
        @level0type = N'SCHEMA',
        @level0name = N'dbo',
        @level1type = N'TABLE',
        @level1name = N'OrderItem';
END;
GO

IF OBJECT_ID(N'dbo.Payment', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payment
    (
        PaymentId INT IDENTITY(1,1) NOT NULL,
        OrderId INT NOT NULL,
        PaymentMethod NVARCHAR(50) NOT NULL,
        PaymentStatus NVARCHAR(50) NOT NULL CONSTRAINT DF_Payment_PaymentStatus DEFAULT 'Pending',
        PaidAmount DECIMAL(12,2) NOT NULL CONSTRAINT DF_Payment_PaidAmount DEFAULT 0,
        PaidAt DATETIME2(7) NULL,
        TransactionReference NVARCHAR(255) NULL,
        CONSTRAINT PK_Payment PRIMARY KEY CLUSTERED (PaymentId ASC),
        CONSTRAINT FK_Payment_Order FOREIGN KEY (OrderId) REFERENCES dbo.[Order](OrderId) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT UQ_Payment_OrderId UNIQUE (OrderId),
        CONSTRAINT CK_Payment_Method CHECK (PaymentMethod IN ('Cash', 'Card', 'BankTransfer', 'DigitalWallet')),
        CONSTRAINT CK_Payment_Status CHECK (PaymentStatus IN ('Pending', 'Paid', 'Failed', 'Refunded'))
    );

    EXEC sys.sp_addextendedproperty
        @name = N'MS_Description',
        @value = N'Quản lý thông tin thanh toán cho từng đơn hàng.',
        @level0type = N'SCHEMA',
        @level0name = N'dbo',
        @level1type = N'TABLE',
        @level1name = N'Payment';
END;
GO

IF OBJECT_ID(N'dbo.Shipment', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Shipment
    (
        ShipmentId INT IDENTITY(1,1) NOT NULL,
        OrderId INT NOT NULL,
        ShippingProvider NVARCHAR(100) NULL,
        TrackingNumber NVARCHAR(100) NULL,
        ShipmentStatus NVARCHAR(50) NOT NULL CONSTRAINT DF_Shipment_ShipmentStatus DEFAULT 'Pending',
        ShippedAt DATETIME2(7) NULL,
        DeliveredAt DATETIME2(7) NULL,
        CONSTRAINT PK_Shipment PRIMARY KEY CLUSTERED (ShipmentId ASC),
        CONSTRAINT FK_Shipment_Order FOREIGN KEY (OrderId) REFERENCES dbo.[Order](OrderId) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT UQ_Shipment_OrderId UNIQUE (OrderId),
        CONSTRAINT CK_Shipment_Status CHECK (ShipmentStatus IN ('Pending', 'Packed', 'Shipping', 'Delivered', 'Cancelled'))
    );

    EXEC sys.sp_addextendedproperty
        @name = N'MS_Description',
        @value = N'Quản lý tiến trình giao hàng và theo dõi vận chuyển.',
        @level0type = N'SCHEMA',
        @level0name = N'dbo',
        @level1type = N'TABLE',
        @level1name = N'Shipment';
END;
GO

IF OBJECT_ID(N'dbo.Review', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Review
    (
        ReviewId INT IDENTITY(1,1) NOT NULL,
        UserId INT NOT NULL,
        ProductId INT NOT NULL,
        Rating TINYINT NOT NULL,
        Comment NVARCHAR(1000) NULL,
        CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_Review_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Review PRIMARY KEY CLUSTERED (ReviewId ASC),
        CONSTRAINT FK_Review_User FOREIGN KEY (UserId) REFERENCES dbo.[User](UserId) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT FK_Review_Product FOREIGN KEY (ProductId) REFERENCES dbo.Product(ProductId) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT UQ_Review_User_Product UNIQUE (UserId, ProductId),
        CONSTRAINT CK_Review_Rating CHECK (Rating BETWEEN 1 AND 5)
    );

    EXEC sys.sp_addextendedproperty
        @name = N'MS_Description',
        @value = N'Cho phép khách hàng đánh giá sản phẩm sau khi mua.',
        @level0type = N'SCHEMA',
        @level0name = N'dbo',
        @level1type = N'TABLE',
        @level1name = N'Review';
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Address_UserId' AND object_id = OBJECT_ID(N'dbo.Address'))
BEGIN
    CREATE INDEX IX_Address_UserId ON dbo.Address(UserId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Product_CategoryId' AND object_id = OBJECT_ID(N'dbo.Product'))
BEGIN
    CREATE INDEX IX_Product_CategoryId ON dbo.Product(CategoryId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inventory_ProductId' AND object_id = OBJECT_ID(N'dbo.Inventory'))
BEGIN
    CREATE INDEX IX_Inventory_ProductId ON dbo.Inventory(ProductId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Cart_UserId' AND object_id = OBJECT_ID(N'dbo.Cart'))
BEGIN
    CREATE INDEX IX_Cart_UserId ON dbo.Cart(UserId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CartItem_CartId' AND object_id = OBJECT_ID(N'dbo.CartItem'))
BEGIN
    CREATE INDEX IX_CartItem_CartId ON dbo.CartItem(CartId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CartItem_ProductId' AND object_id = OBJECT_ID(N'dbo.CartItem'))
BEGIN
    CREATE INDEX IX_CartItem_ProductId ON dbo.CartItem(ProductId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Order_UserId' AND object_id = OBJECT_ID(N'dbo.[Order]'))
BEGIN
    CREATE INDEX IX_Order_UserId ON dbo.[Order](UserId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Order_OrderDate' AND object_id = OBJECT_ID(N'dbo.[Order]'))
BEGIN
    CREATE INDEX IX_Order_OrderDate ON dbo.[Order](OrderDate);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OrderItem_OrderId' AND object_id = OBJECT_ID(N'dbo.OrderItem'))
BEGIN
    CREATE INDEX IX_OrderItem_OrderId ON dbo.OrderItem(OrderId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OrderItem_ProductId' AND object_id = OBJECT_ID(N'dbo.OrderItem'))
BEGIN
    CREATE INDEX IX_OrderItem_ProductId ON dbo.OrderItem(ProductId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Payment_OrderId' AND object_id = OBJECT_ID(N'dbo.Payment'))
BEGIN
    CREATE INDEX IX_Payment_OrderId ON dbo.Payment(OrderId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Shipment_OrderId' AND object_id = OBJECT_ID(N'dbo.Shipment'))
BEGIN
    CREATE INDEX IX_Shipment_OrderId ON dbo.Shipment(OrderId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Review_ProductId' AND object_id = OBJECT_ID(N'dbo.Review'))
BEGIN
    CREATE INDEX IX_Review_ProductId ON dbo.Review(ProductId);
END;
GO
