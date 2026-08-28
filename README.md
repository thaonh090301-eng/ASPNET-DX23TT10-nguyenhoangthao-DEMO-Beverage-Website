# Beverage Website

Little Cloud – Beverage Website

Website bán cà phê và đồ uống được xây dựng bằng ASP.NET MVC 5, C#, .NET Framework 4.8, SQL Server 2019 và ADO.NET.

1. Giới thiệu

Little Cloud là website bán cà phê và đồ uống với hai khu vực sử dụng chính:

Khách hàng: xem menu, tìm kiếm sản phẩm, lọc theo danh mục, xem chi tiết, quản lý giỏ hàng, hồ sơ và địa chỉ giao hàng, đặt hàng và theo dõi đơn hàng.

Quản trị viên: quản lý danh mục, sản phẩm, giá bán, nhãn sản phẩm, tồn kho và xử lý đơn hàng.

Hệ thống được tổ chức theo kiến trúc phân lớp nhằm tách biệt giao diện, điều khiển, xử lý nghiệp vụ và truy cập dữ liệu.

2. Công nghệ sử dụng

ASP.NET MVC 5

C#

.NET Framework 4.8

SQL Server 2019

SQL Server Management Studio (SSMS)

ADO.NET

Razor View Engine

HTML, CSS và JavaScript

3. Kiến trúc hệ thống

Luồng xử lý chính của ứng dụng:

Người dùng
    ↓
Views / Razor
    ↓
Controllers
    ↓
BLL (Business Logic Layer)
    ↓
DAL (Data Access Layer)
    ↓
DataProvider / ADO.NET
    ↓
SQL Server 2019

Vai trò các tầng

Views: hiển thị giao diện và nhận thao tác của người dùng.

Controllers: tiếp nhận HTTP request, điều phối xử lý và trả kết quả về View.

BLL: kiểm tra và thực hiện các quy tắc nghiệp vụ của hệ thống.

DAL: thực hiện truy vấn và cập nhật dữ liệu.

DataProvider: quản lý kết nối, command, parameter và transaction bằng ADO.NET.

SQL Server: lưu trữ dữ liệu của hệ thống.

4. Cấu trúc repository

.
├── README.md
├── setup/
├── src/
│   └── BeverageWebsite/
│       ├── App_Start/
│       ├── BLL/
│       ├── Content/
│       ├── Controllers/
│       ├── DAL/
│       ├── Filters/
│       ├── Helpers/
│       ├── Models/
│       ├── ViewModels/
│       ├── Views/
│       ├── BeverageWebsite.csproj
│       └── Web.config
├── progress-report/
├── thesis/
├── soft/
└── docker/

Một số thành phần quan trọng

BLL/: các lớp xử lý nghiệp vụ như ProductBLL, CategoryBLL, CartBLL, OrderBLL, InventoryBLL, UserBLL và các lớp liên quan.

Controllers/: các Controller phục vụ phía khách hàng và quản trị, gồm AccountController, AddressController, AdminController, CartController, CategoryController, CheckoutController, HomeController, NavigationController, OrderController và ProductController.

DAL/: các lớp truy cập dữ liệu và DataProvider.

Models/: các lớp dữ liệu tương ứng với các bảng trong cơ sở dữ liệu.

ViewModels/: dữ liệu trung gian phục vụ từng màn hình/giao diện.

Views/: các giao diện Razor của website và khu vực Admin.

Content/: CSS, hình ảnh và tài nguyên giao diện.

Filters/: các bộ lọc phục vụ kiểm soát truy cập, trong đó có cơ chế bảo vệ khu vực quản trị.

Helpers/: các tiện ích dùng chung của ứng dụng.

5. Cơ sở dữ liệu

Tên cơ sở dữ liệu phát triển:

BeverageWebsiteDb

Các bảng chính

User
Address
Category
Product
Inventory
Promotion
Cart
CartItem
Order
OrderItem
Payment
Shipment
Review

Script khởi tạo

Script cơ sở dữ liệu nằm tại:

setup/Database.sql

Nếu repository có script bổ sung cho dữ liệu sản phẩm/nhãn, kiểm tra và thực thi script theo hướng dẫn trong thư mục setup/ sau khi Database.sql được tạo thành công.

6. Cài đặt và chạy project

Bước 1 – Chuẩn bị môi trường

Cài đặt:

SQL Server 2019 hoặc phiên bản tương thích với project.

SQL Server Management Studio.

.NET Framework 4.8.

Visual Studio có hỗ trợ ASP.NET MVC 5/.NET Framework hoặc môi trường build tương ứng.

Bước 2 – Tạo cơ sở dữ liệu

Mở SQL Server Management Studio và thực thi:

setup/Database.sql

Đảm bảo database BeverageWebsiteDb được tạo thành công.

Bước 3 – Cấu hình kết nối

Mở:

src/BeverageWebsite/Web.config

Kiểm tra connection string có tên:

BeverageWebsiteDbConnection

và điều chỉnh Server, Database hoặc phương thức xác thực theo môi trường máy local.

Không đưa mật khẩu SQL Server, API key hoặc thông tin production vào repository.

Bước 4 – Build

Từ thư mục gốc repository:

dotnet build BeverageWebsite.sln --no-incremental

Hoặc mở solution bằng Visual Studio và Build Solution.

Bước 5 – Chạy website

Chạy project bằng Visual Studio/IIS Express theo cấu hình hiện tại của solution.

Trong môi trường phát triển hiện tại, website được kiểm thử qua địa chỉ local dạng:

http://localhost:51158/

Port có thể khác tùy cấu hình máy.

7. Tài khoản demo

Các tài khoản dưới đây dùng cho môi trường demo/local của đồ án.

7.1. Tài khoản quản trị viên

Email:    admindemo@gmail.com
Password: admin
Role:     Admin

Tài khoản này dùng để kiểm thử:

Dashboard quản trị.

Quản lý danh mục.

Quản lý sản phẩm.

Thay đổi giá sản phẩm.

Quản lý nhãn sản phẩm.

Quản lý tồn kho.

Xem và xử lý đơn hàng.

7.2. Tài khoản khách hàng

Email:    customdemo@gmail.com
Password: custom
Role:     Customer

Tài khoản này dùng để kiểm thử:

Xem menu.

Tìm kiếm và lọc sản phẩm.

Xem chi tiết sản phẩm.

Thêm/cập nhật/xóa sản phẩm trong giỏ hàng.

Quản lý hồ sơ.

Quản lý và cập nhật địa chỉ giao hàng.

Đặt hàng.

Xem danh sách và chi tiết đơn hàng.

Lưu ý: Hai tài khoản trên chỉ dành cho đồ án và môi trường local/demo. Không sử dụng các mật khẩu này cho môi trường production.

8. Các chức năng chính

8.1. Khách chưa đăng nhập

Truy cập trang chủ.

Xem menu đồ uống.

Xem danh mục sản phẩm.

Tìm kiếm sản phẩm.

Xem chi tiết sản phẩm.

Nhận biết sản phẩm còn món hoặc tạm hết món.

Đăng ký tài khoản.

Đăng nhập.

8.2. Khách hàng

Ngoài các chức năng công khai, khách hàng có thể:

Xem và chỉnh sửa thông tin hồ sơ.

Thêm và cập nhật địa chỉ giao hàng.

Thêm sản phẩm vào giỏ hàng.

Cập nhật số lượng trong giỏ hàng.

Xóa sản phẩm khỏi giỏ hàng.

Đặt hàng.

Xem lịch sử đơn hàng.

Xem chi tiết đơn hàng.

8.3. Quản trị viên

Xem Dashboard.

Quản lý danh mục đồ uống.

Thêm, chỉnh sửa và xóa dữ liệu danh mục theo quyền được cấp.

Thêm, chỉnh sửa và xóa sản phẩm theo quyền được cấp.

Cập nhật giá sản phẩm.

Quản lý nhãn Best seller, Món mới, Món nổi bật.

Quản lý tồn kho và ngưỡng nhập lại.

Theo dõi sản phẩm tạm hết món.

Xem danh sách đơn hàng.

Xem chi tiết đơn hàng.

Cập nhật trạng thái đơn hàng.

Hủy đơn theo quy tắc nghiệp vụ.

9. Quy tắc nghiệp vụ quan trọng

Trạng thái sản phẩm

Sản phẩm đang hoạt động và còn tồn kho: khách có thể xem và đặt mua.

Sản phẩm đang hoạt động nhưng tồn kho bằng 0: vẫn hiển thị trên menu với trạng thái Tạm hết món, nhưng không cho thêm vào giỏ.

Sản phẩm ngừng bán: không hiển thị cho khách hàng.

Nhãn sản phẩm

Các nhãn chính:

Best seller
Món mới
Món nổi bật

Sản phẩm có nhãn được ưu tiên khi hiển thị trên menu theo logic sắp xếp của ứng dụng.

Tồn kho

Không cho phép tồn kho âm.

Kiểm tra tồn kho khi thao tác với giỏ hàng.

Kiểm tra lại tồn kho trước khi tạo đơn.

Khi checkout thành công, tồn kho được giảm theo số lượng đặt.

Đơn hàng

Trạng thái chính:

Pending → Confirmed → Processing → Completed

Đơn hàng đủ điều kiện có thể chuyển sang:

Cancelled

Khi hủy đơn hợp lệ, số lượng đã trừ khỏi tồn kho được hoàn lại theo quy tắc nghiệp vụ và phải đảm bảo không hoàn tồn kho nhiều lần.

10. Quy trình demo đề xuất

Demo Customer

Đăng nhập
→ Menu
→ Tìm kiếm/lọc sản phẩm
→ Xem chi tiết
→ Thêm vào giỏ
→ Kiểm tra giỏ hàng
→ Hồ sơ / Địa chỉ giao hàng
→ Đặt hàng
→ Xem đơn hàng

Demo Admin

Đăng nhập Admin
→ Dashboard
→ Quản lý sản phẩm
→ Chỉnh sửa giá/nhãn
→ Quản lý tồn kho
→ Xem đơn hàng
→ Xem chi tiết đơn
→ Cập nhật trạng thái / Hủy đơn

11. Kiểm thử chức năng tiêu biểu

Một số trường hợp nên kiểm tra khi chạy demo:

Đăng nhập đúng/sai thông tin.

Customer không truy cập được khu vực Admin.

Tìm kiếm và lọc sản phẩm.

Thêm, cập nhật và xóa sản phẩm trong giỏ.

Sản phẩm tồn kho bằng 0 hiển thị Tạm hết món và không thể thêm vào giỏ.

Cập nhật địa chỉ trong Hồ sơ và kiểm tra địa chỉ mới khi checkout.

Đặt hàng thành công và kiểm tra tồn kho.

Admin cập nhật trạng thái đơn hàng.

Hủy đơn hợp lệ và kiểm tra hoàn tồn kho.

Giá của đơn hàng cũ không thay đổi khi giá sản phẩm hiện tại được chỉnh sửa.

12. An toàn và toàn vẹn dữ liệu

Project sử dụng các biện pháp chính:

SQL query có parameter.

Kiểm tra định danh và dữ liệu đầu vào.

Khóa chính và khóa ngoại.

Ràng buộc UNIQUE và CHECK tại database.

Kiểm tra quyền sở hữu dữ liệu.

Phân quyền Admin/Customer.

Checkout thực hiện trong transaction.

Rollback khi transaction thất bại.

Không sử dụng giá do client tự gửi để thay thế giá sản phẩm từ database trong checkout.

13. Git – các lệnh cơ bản

Kiểm tra trạng thái:

git status

Lấy code mới nhất:

git pull origin main

Kiểm tra lỗi whitespace:

git diff --check

Build trước khi commit:

dotnet build BeverageWebsite.sln --no-incremental -v:minimal

Commit:

git add -A
git diff --cached --check
git commit -m "your commit message"
git push origin main

14. Thông tin đồ án

Tên đề tài: Xây dựng website bán sản phẩm cà phê và đồ uống Little Cloud

Sinh viên: Nguyễn Hoàng Thảo

Mã sinh viên: 170123374

Lớp: DX23TT10

Nền tảng: ASP.NET MVC 5

Ngôn ngữ: C#

Framework: .NET Framework 4.8

Cơ sở dữ liệu: SQL Server 2019

Truy cập dữ liệu: ADO.NET