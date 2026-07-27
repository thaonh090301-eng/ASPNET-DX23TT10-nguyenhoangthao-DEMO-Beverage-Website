# Beverage Website

## Giới thiệu

Website bán cà phê và đồ uống được phát triển bằng ASP.NET MVC 5, C# và .NET Framework 4.8. Dự án sử dụng SQL Server để lưu trữ dữ liệu và ADO.NET cho tầng truy cập dữ liệu.

## Công nghệ

- ASP.NET MVC 5 (C#)
- .NET Framework 4.8
- SQL Server 2019
- ADO.NET
- SQL Server Management Studio

## Kiến trúc

Kiến trúc mục tiêu của dự án:

```text
Controllers -> BLL -> DAL -> DataProvider -> SQL Server
```

Tầng Models, DataProvider và các lớp DAL đã có trong project. Tầng BLL, các Controllers nghiệp vụ, Views và quy trình xác thực vẫn thuộc phạm vi công việc tiếp theo.

## Cấu trúc Repository

```text
.
├── README.md
├── .gitignore
├── setup/
├── src/
├── progress-report/
├── thesis/
│   ├── doc/
│   ├── pdf/
│   ├── html/
│   ├── abs/
│   └── refs/
├── soft/
└── docker/
```

## Thiết lập cơ sở dữ liệu

Môi trường phát triển cục bộ đã được chuẩn bị với SQL Server 2019 Express, instance `SQLEXPRESS`, cơ sở dữ liệu `BeverageWebsiteDb`, Windows Authentication và SQL Server Management Studio. Script `setup/Database.sql` đã được thực thi thành công để tạo cơ sở dữ liệu.

Các bước thiết lập:

1. Cài đặt SQL Server 2019 Express.
2. Kết nối đến `.\SQLEXPRESS` bằng Windows Authentication.
3. Thực thi `setup/Database.sql`.
4. Cấu hình connection string tên `BeverageWebsiteDbConnection` trong `src/BeverageWebsite/Web.config`.
5. Build `BeverageWebsite.sln`.

### Cấu hình kết nối cục bộ

Ví dụ connection string dành cho môi trường phát triển:

```text
Data Source=.\SQLEXPRESS;
Initial Catalog=BeverageWebsiteDb;
Integrated Security=True;
TrustServerCertificate=True;
```

Cấu hình này chỉ là ví dụ cho môi trường phát triển cục bộ, không phải cấu hình production.

## Trạng thái triển khai hiện tại

Trạng thái được xác định từ mã nguồn, lịch sử thay đổi, lược đồ cơ sở dữ liệu và kết quả build solution.

| Hạng mục | Trạng thái |
|---|---|
| Khởi tạo solution ASP.NET MVC 5 | Đã hoàn thành |
| Cấu hình .NET Framework 4.8 | Đã hoàn thành |
| Lược đồ cơ sở dữ liệu | Đã hoàn thành |
| Models | Đã hoàn thành |
| DataProvider bằng ADO.NET và hỗ trợ transaction | Đã hoàn thành |
| Các lớp DAL | Đã hoàn thành |
| Thiết lập SQL Server cục bộ | Đã hoàn thành |
| Cấu hình kết nối cơ sở dữ liệu | Đã hoàn thành |
| Các hiệu chỉnh kiểm tra DAL đã phát hiện đến hiện tại | Đã hoàn thành |
| Tầng BLL | Chưa thực hiện |
| Controllers và Views nghiệp vụ | Chưa thực hiện |
| Quy trình xác thực và phân quyền | Chưa thực hiện |

### Các thành phần DAL đã hoàn thành

- `AddressDAL`
- `CartDAL`
- `CategoryDAL`
- `InventoryDAL`
- `OrderDAL`
- `PaymentDAL`
- `ProductDAL`
- `PromotionDAL`
- `ReviewDAL`
- `ShipmentDAL`
- `UserDAL`

Các truy vấn DAL sử dụng ADO.NET, DataProvider dùng chung, câu lệnh có tham số và danh sách cột tường minh.

## Cải thiện an toàn và toàn vẹn dữ liệu

- Checkout xác thực `AddressId` và quyền sở hữu địa chỉ của người đặt hàng trong cùng transaction, sử dụng khóa phù hợp và giữ nguyên cơ chế rollback. Khi tạo đơn hàng, SQL Server áp dụng giá trị mặc định của `OrderStatus`.
- `InventoryDAL` không cho phép chuyển một bản ghi tồn kho sang sản phẩm khác trong phương thức cập nhật tổng quát; các thao tác cập nhật kiểm tra định danh, số lượng không âm, số dòng bị ảnh hưởng và thang `DATETIME2(7)`.
- `setup/Database.sql` chứa ràng buộc duy nhất có thể chạy lại `UQ_CartItem_Cart_Product` trên `CartId` và `ProductId`.
- `CartDAL.AddItem` chạy trong một transaction, xác thực quyền sở hữu giỏ hàng, yêu cầu sản phẩm đang hoạt động, đọc giá hiện tại từ bảng Product, dùng khóa để tránh dòng CartItem trùng lặp, cập nhật hoặc thêm đúng một dòng và dùng `DECIMAL(12,2)` cho `UnitPrice`.
- Các thao tác `AddItem`, `UpdateQuantity`, `RemoveItem` và `ClearCart` nhận thông tin người dùng và kiểm tra quyền sở hữu giỏ hàng trước khi thay đổi dữ liệu.
- `UserDAL` không trả về `PasswordHash` trong các truy vấn hồ sơ thông thường; dữ liệu xác thực chỉ được đọc qua phương thức chuyên biệt. Cập nhật hồ sơ và cập nhật mật khẩu được tách riêng, đồng thời bảo toàn `CreatedAt` và `PasswordHash`.
- `UserDAL` chuẩn hóa email, kiểm tra các chuỗi theo độ dài của lược đồ, chuyển chuỗi tùy chọn rỗng thành `NULL` và kiểm tra vai trò theo ràng buộc `CHECK`.
- `CategoryDAL` và `ProductDAL` kiểm tra định danh, chuẩn hóa chuỗi, kiểm tra độ dài, kiểm tra số dòng bị ảnh hưởng và lưu cache ordinal khi ánh xạ dữ liệu.
- `ProductDAL` bảo toàn `CreatedAt`, từ chối giá âm và định nghĩa tham số giá theo `DECIMAL(12,2)`.

Thuộc tính `Product.ImageUrl` hiện được dùng để lưu một đường dẫn ảnh chính của sản phẩm. Chức năng tải lên và hiển thị ảnh sản phẩm sẽ được thực hiện trong bước MVC sau.

## Công việc còn lại

- Hoàn thành các phát hiện còn lại trong đợt rà soát DAL.
- Chuẩn hóa các thông báo ngoại lệ còn lại.
- Rà soát `ProductDAL.Search`.
- Rà soát thang của các tham số `DateTime2` còn lại.
- Xây dựng tầng BLL.
- Xây dựng Controllers và Views nghiệp vụ.
- Xây dựng quy trình xác thực.
- Xây dựng phân quyền.
- Xây dựng chức năng tải lên và hiển thị ảnh sản phẩm.
- Kiểm thử tích hợp với cơ sở dữ liệu.
- Kiểm thử toàn bộ quy trình checkout.
- Hoàn thiện tài liệu dự án.

Chưa có kết luận về kiểm thử runtime hoặc kiểm thử đầu cuối; kết quả hiện tại chỉ xác nhận solution build thành công.

## Hướng dẫn build

Từ thư mục gốc của repository, chạy:

```powershell
dotnet build BeverageWebsite.sln
```

Kết quả kiểm tra ngày 27/07/2026: build thành công, 0 cảnh báo và 0 lỗi.
