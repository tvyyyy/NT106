# Ứng Dụng Quản Lý Bán Trà Sữa


<p align="center">
  Hệ thống quản lý bán trà sữa — từ đặt hàng, thanh toán đến quản lý menu và đơn hàng theo thời gian thực.
</p>

---

## Mục lục

| STT | Nội dung chính |
| :---: | :--- |
| 1 | [Giới thiệu](#gioi-thieu) |
| 2 | [Đối tượng người dùng](#doi-tuong-nguoi-dung) |
| 3 | [Tính năng chính](#tinh-nang-chinh) |
| 4 | [Công nghệ sử dụng](#cong-nghe-su-dung) |
| 5 | [Kiến trúc tổng quan](#kien-truc-tong-quan) |
| 6 | [Yêu cầu công cụ và môi trường](#yeu-cau-cong-cu-va-moi-truong) |
| 7 | [Hướng dẫn cài đặt và cấu hình môi trường](#huong-dan-cai-dat-va-cau-hinh-moi-truong) |
| 8 | [Nhóm thực hiện](#nhom-thuc-hien) |

---

## Giới thiệu

Ứng dụng Quản lý Bán Trà Sữa là hệ thống hỗ trợ vận hành một cửa hàng trà sữa theo hướng số hóa toàn bộ quy trình bán hàng, từ khâu tiếp nhận đơn, xử lý thanh toán, cho đến quản lý menu và theo dõi trạng thái đơn hàng theo thời gian thực. Hệ thống được xây dựng nhằm phục vụ đồng thời hai nhóm đối tượng: khách hàng sử dụng dịch vụ và nhân viên hoặc quản lý vận hành cửa hàng, với mục tiêu rút ngắn thời gian xử lý đơn, giảm sai sót thủ công và mang lại trải nghiệm đặt hàng liền mạch.

---

## Đối tượng người dùng

| Vai trò | Mô tả |
|---|---|
| **Khách hàng** | Đặt món, thanh toán, theo dõi đơn hàng, đánh giá và quản lý hồ sơ cá nhân |
| **Nhân viên / Quản lý** *(tài khoản đặc biệt)* | Quản lý menu, xử lý đơn hàng, cập nhật trạng thái, theo dõi hoạt động cửa hàng |

---

## Tính năng chính

**Tài khoản và bảo mật:** 
Tạo tài khoản, đăng nhập, quên mật khẩu, sửa mật khẩu 
Chỉnh sửa hồ sơ cá nhân 

**Quản lý menu:** 
Tạo menu, chỉnh sửa menu *(dành riêng cho tài khoản đặc biệt)* 
Tìm kiếm và lọc thông tin đồ uống 
Xem chi tiết đồ uống 

**Đặt hàng và thanh toán:** 
Tạo, thêm, sửa, hủy đơn hàng 
Chọn chỗ ngồi hoặc mang về 
Áp mã giảm giá và khuyến mãi 
Thanh toán, gửi hóa đơn 

**Quản lý và theo dõi:** 
Quản lý trạng thái đơn hàng 
Lịch sử đặt hàng 

**Tương tác khách hàng:** 
Đánh giá, liên hệ, góp ý 

---

## Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Ngôn ngữ lập trình | `C#` |
| Giao diện ứng dụng | `WinForms` (Desktop / Mobile) |
| Backend Framework | `ASP.NET Core Web API` (.NET 8.0) |
| Cơ sở dữ liệu | `MySQL` |
| Giao tiếp thời gian thực | `ASP.NET Core SignalR` |
| Dịch vụ gửi email | `MailKit` / `MimeKit` |
| Xác thực và bảo mật | `JWT` (JSON Web Token), `BCrypt` |

---
## Cấu trúc thư mục 
```text

├── src/
│   ├── Shared/
│   ├── Server.Api/
│   │   ├── Controllers/
│   │   ├── Services/
│   │   ├── Models/
│   │   ├── Data/
│   │   └── Hubs/
│   ├── Client.Customer/
│   └── Client.Staff/
├── docs/
└── database/
              
```

## Kiến trúc tổng quan

Hệ thống được tổ chức theo mô hình client-server, trong đó ứng dụng phía người dùng đóng vai trò client giao tiếp với backend thông qua các endpoint RESTful:

```text
client/ (WinForms Desktop/Mobile)
│
├── HTTP REST ────► backend/ (ASP.NET Core Web API .NET 8.0)
│                      ├── Xác thực (JWT / BCrypt)
│                      └── MailKit Service (Email)
│                      │
└── SignalR ──────► server/ (ASP.NET Core SignalR - Thông báo đơn hàng)
                       │
                       ▼
                 MySQL Database
```

Toàn bộ logic nghiệp vụ được xử lý tập trung tại tầng API, đảm bảo tính nhất quán dữ liệu và khả năng mở rộng khi ứng dụng phát triển thêm các kênh giao diện khác trong tương lai. SignalR đảm nhiệm việc đẩy thông báo trạng thái đơn hàng theo thời gian thực đến người dùng mà không cần làm mới thủ công, trong khi MailKit chịu trách nhiệm gửi hóa đơn điện tử sau khi giao dịch hoàn tất.

---

## Yêu cầu công cụ và môi trường

| Công cụ | Phiên bản đề xuất | Ghi chú |
|---|---|---|
| .NET SDK | `8.0+` | Bắt buộc để build và chạy ASP.NET Core Web API |
| Visual Studio | `2022 (17.8+)` | Cài kèm 2 workload: **.NET desktop development** và **ASP.NET and web development** |
| MySQL Server | `8.0+` | Cơ sở dữ liệu chính của hệ thống |
| MySQL Workbench | Bản mới nhất | Công cụ quản trị và thiết kế cơ sở dữ liệu trực quan |
| Git | Bản mới nhất | Quản lý mã nguồn và làm việc nhóm |
| Postman *(hoặc tương đương)* | Bản mới nhất | Kiểm thử các endpoint của Web API |

**Các gói NuGet chính:**

| Gói | Vai trò |
|---|---|
| `Microsoft.EntityFrameworkCore` + `Pomelo.EntityFrameworkCore.MySql` | Ánh xạ và truy vấn dữ liệu MySQL |
| `Microsoft.AspNetCore.SignalR` | Thông báo đơn hàng theo thời gian thực |
| `MailKit` + `MimeKit` | Soạn và gửi hóa đơn qua email |
| `System.IdentityModel.Tokens.Jwt` | Phát hành và xác thực JWT |
| `BCrypt.Net-Next` | Băm và kiểm tra mật khẩu người dùng |

---

## Hướng dẫn cài đặt và cấu hình môi trường

1. **Sao chép mã nguồn**
   Clone repository của dự án về máy và mở solution bằng Visual Studio.

2. **Khôi phục các gói NuGet**
   ```bash
   dotnet restore
   ```

3. **Tạo cơ sở dữ liệu MySQL**
   Mở MySQL Workbench, tạo database mới theo tên đã thống nhất trong nhóm, sau đó chạy script tạo bảng đã chuẩn bị ở Giai đoạn 2.

4. **Cấu hình chuỗi kết nối và các khóa bảo mật**
   Trong `appsettings.json` của dự án API:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "server=localhost;port=3306;database=tra_sua_db;user=root;password=your_password"
     },
     "Jwt": {
       "Key": "your_secret_key",
       "Issuer": "TraSuaApp",
       "Audience": "TraSuaAppUsers",
       "ExpireMinutes": 60
     },
     "MailSettings": {
       "SmtpServer": "smtp.gmail.com",
       "Port": 587,
       "SenderEmail": "your_email@gmail.com",
       "SenderPassword": "your_app_password"
     }
   }
   ```

   > **Lưu ý:** không đưa các thông tin nhạy cảm này lên hệ thống quản lý mã nguồn dùng chung; nên dùng `appsettings.Development.json` hoặc biến môi trường cho phát triển cục bộ.

5. **Áp dụng migration** *(nếu dùng Entity Framework Core)*
   ```bash
   dotnet ef database update
   ```

6. **Chạy dự án API**
   ```bash
   dotnet run
   ```
   Sau đó kiểm tra các endpoint bằng Postman.

7. **Chạy ứng dụng WinForms**
   Cấu hình địa chỉ base URL của API trong ứng dụng client, sau đó build và chạy dự án WinForms để kết nối thử với backend.

8. **Kiểm tra các thành phần phụ trợ**
   Xác nhận SignalR Hub nhận và phát được thông báo trạng thái đơn hàng, đồng thời gửi thử một email hóa đơn để xác minh cấu hình MailKit hoạt động đúng.

---

## Nhóm thực hiện

**Nhóm 18 — Đồ án môn NT106**

| STT | MSSV | Họ và tên | Vai trò / Nhiệm vụ |
| :---: | :---: | :--- | :--- |
| 1 | 24522071 | Nguyễn Thúy Vy | Leader / Backend & CSDL |
| 2 | 25521179 | Nguyễn Ngọc Kim Ngân | Frontend & Báo cáo |
---
