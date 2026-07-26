# Beverage Website

## Giới thiệu
Website bán cà phê và đồ uống được phát triển bằng ASP.NET MVC 5, C# và .NET Framework 4.8.

## Công nghệ
- ASP.NET MVC 5 (C#)
- .NET Framework 4.8
- SQL Server 2019
- ADO.NET

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

## Tiến độ thực hiện

Trạng thái được xác định sau khi kiểm tra nội dung file, khai báo trong project và kết quả build solution.

| Hạng mục | Trạng thái |
|---|---|
| Khởi tạo solution ASP.NET MVC 5 | Đã hoàn thành |
| Cấu hình .NET Framework 4.8 | Đã hoàn thành |
| Thiết kế cơ sở dữ liệu SQL Server | Đã hoàn thành |
| Xây dựng các lớp Models | Đã hoàn thành |
| DataProvider bằng ADO.NET | Đã hoàn thành |
| CategoryDAL | Đã hoàn thành |
| ProductDAL | Đã hoàn thành |
| UserDAL | Đã hoàn thành |
| InventoryDAL | Đã hoàn thành |
| CartDAL | Đã hoàn thành |
| Transaction support trong DataProvider | Đã hoàn thành |
| OrderDAL | Đã hoàn thành |
| AddressDAL | Đã hoàn thành |

### Đang thực hiện

- Bổ sung các DAL chưa có trong project: PromotionDAL, PaymentDAL, ShipmentDAL và ReviewDAL.
- Kiểm thử kết nối SQL Server và các nghiệp vụ chính ở runtime; hiện mới xác nhận solution build thành công.
- Hoàn thiện các tầng nghiệp vụ và giao diện cho toàn bộ chức năng website.

### Công việc tiếp theo

- PromotionDAL
- PaymentDAL
- ShipmentDAL
- ReviewDAL
- Hoàn thiện các DAL còn lại theo Database.sql
- Xây dựng tầng BLL
- Xây dựng Controllers
- Xây dựng Views
- Kiểm thử kết nối SQL Server và các nghiệp vụ chính
