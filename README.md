# Beverage Website

## Giới thiệu

Website bán cà phê và đồ uống được phát triển bằng ASP.NET MVC 5, C#, .NET Framework 4.8, SQL Server 2019 và ADO.NET.

## Công nghệ

- ASP.NET MVC 5
- C#
- .NET Framework 4.8
- SQL Server 2019
- ADO.NET

## Kiến trúc

Kiến trúc mục tiêu của dự án:

```text
Controllers -> BLL -> DAL -> DataProvider -> SQL Server
```

- Models là các lớp POCO được xây dựng phù hợp với lược đồ cơ sở dữ liệu.
- DAL sử dụng các câu lệnh ADO.NET có tham số để truy cập SQL Server.
- BLL kiểm tra dữ liệu nghiệp vụ và chuyển việc lưu trữ dữ liệu cho DAL.
- Controllers và Views là giai đoạn triển khai tiếp theo.

## Cấu trúc repository

```text
.
├── README.md
├── setup/
├── src/
├── progress-report/
├── thesis/
├── soft/
└── docker/
```

## Thiết lập cơ sở dữ liệu

1. Cài đặt SQL Server 2019.
2. Thực thi `setup/Database.sql`.
3. Cấu hình connection string `BeverageWebsiteDbConnection` trong `src/BeverageWebsite/Web.config` cho môi trường sử dụng.
4. Build `BeverageWebsite.sln`.

Không lưu thông tin xác thực hoặc cấu hình kết nối production trong tài liệu hay mã nguồn được chia sẻ.

## Trạng thái triển khai hiện tại

| Hạng mục | Trạng thái |
|---|---|
| Solution ASP.NET MVC 5 và .NET Framework 4.8 | Đã hoàn thành |
| Lược đồ cơ sở dữ liệu | Đã hoàn thành |
| Models | Đã hoàn thành |
| DataProvider và DAL | Đã hoàn thành, đã rà soát |
| BLL | Đã hoàn thành, đã rà soát |
| Controllers và Views nghiệp vụ | Giai đoạn tiếp theo |
| Xác thực và phân quyền | Chưa thực hiện |

## Models đã hoàn thành

- `User`
- `Address`
- `Category`
- `Product`
- `Inventory`
- `Promotion`
- `Cart`
- `CartItem`
- `Order`
- `OrderItem`
- `Payment`
- `Shipment`
- `Review`

## Tầng DAL đã hoàn thành

- `DataProvider`
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

Kết quả rà soát DAL cuối cùng:

- Critical: 0
- High: 0
- Medium: 0
- Low: 0

Các biện pháp đã hoàn thành trong DAL:

- SQL có tham số.
- Danh sách cột tường minh.
- Kiểm tra định danh.
- Độ dài chuỗi phù hợp với lược đồ.
- Precision và scale cho dữ liệu thập phân.
- Scale phù hợp cho tham số `DateTime2`.
- Kiểm tra chính xác số dòng bị ảnh hưởng của thao tác thay đổi dữ liệu.
- Kiểm tra quyền sở hữu dữ liệu.
- Thông báo ngoại lệ bên ngoài ở mức tổng quát.
- Checkout trong một transaction.
- Giá sản phẩm được đọc từ cơ sở dữ liệu.
- Cập nhật tồn kho và dọn giỏ hàng trong checkout.
- Lưu cache ordinal khi ánh xạ dữ liệu.

## Tầng BLL đã hoàn thành

- `AddressBLL`
- `CartBLL`
- `CategoryBLL`
- `InventoryBLL`
- `OrderBLL`
- `PaymentBLL`
- `ProductBLL`
- `PromotionBLL`
- `ReviewBLL`
- `ShipmentBLL`
- `UserBLL`

Toàn bộ 11 lớp BLL được khai báo đúng một lần trong `BeverageWebsite.csproj`.

- BLL không chứa SQL và không sử dụng trực tiếp `DataProvider`.
- Kiểm tra dữ liệu trong BLL phù hợp với `Database.sql` và hợp đồng của DAL.
- Các chữ ký phương thức bảo vệ quyền sở hữu được giữ nguyên.
- Checkout đơn hàng vẫn được chuyển cho một lời gọi DAL có transaction.
- Các truy vấn người dùng thông thường không trả về `PasswordHash`.
- Các lớp thanh toán và giao hàng chỉ quản lý bản ghi đã lưu, không tự động tích hợp cổng thanh toán hoặc đơn vị vận chuyển.

Kết quả rà soát BLL cuối cùng:

- Critical: 0
- High: 0
- Medium: 0
- Low: 0

## Trạng thái build

Kết quả đã xác minh ngày 28/07/2026:

- Build warnings: 0
- Build errors: 0

Kết quả này xác nhận solution build thành công; chưa thay thế cho kiểm thử runtime, kiểm thử tích hợp cơ sở dữ liệu hoặc kiểm thử đầu cuối.

## Giai đoạn tiếp theo

Giai đoạn tiếp theo là xây dựng Controllers và Views. Công việc đầu tiên dự kiến:

- Tạo `CategoryController` công khai, chỉ đọc.
- Thêm các action `Index` và `Details`.
- Chỉ sử dụng `CategoryBLL`.
- Trả về HTTP 400 khi thiếu định danh bắt buộc.
- Trả về HTTP 404 khi danh mục không tồn tại.

Controllers và Views nghiệp vụ chưa được xem là hoàn thành tại thời điểm cập nhật tài liệu này.

## Kế hoạch ảnh sản phẩm

`Product.ImageUrl` hiện hỗ trợ một ảnh chính cho mỗi sản phẩm. Công việc ở giai đoạn MVC sau sẽ bổ sung:

- Tải ảnh sản phẩm lên ứng dụng ASP.NET MVC.
- Lưu trữ ảnh trong ứng dụng ASP.NET MVC.
- Ảnh thu nhỏ của sản phẩm.
- Ảnh trong trang chi tiết sản phẩm.
- Ảnh sản phẩm trong giỏ hàng.

Chưa triển khai hoặc xác nhận hỗ trợ nhiều ảnh cho một sản phẩm ở giai đoạn này.

## Hướng dẫn build

Từ thư mục gốc của repository, chạy:

```powershell
dotnet build BeverageWebsite.sln
```
