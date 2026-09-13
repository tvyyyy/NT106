# Ứng Dụng Quản Lý Bán Trà Sữa

<p align="center">
  Hệ thống quản lý bán trà sữa — từ đặt hàng, thanh toán đến quản lý menu và đơn hàng theo thời gian thực.
</p>

---

## Mục lục

| STT | Nội dung chính |
| :---: | :--- |
| 1 | [Giới thiệu](#giới-thiệu) |
| 2 | [Đối tượng người dùng](#đối-tượng-người-dùng) |
| 3 | [Tính năng chính](#tính-năng-chính) |
| 4 | [Công nghệ sử dụng](#công-nghệ-sử-dụng) |
| 5 | [Cấu trúc thư mục](#cấu-trúc-thư-mục) |
| 6 | [Kiến trúc tổng quan](#kiến-trúc-tổng-quan) |
| 7 | [Yêu cầu công cụ và môi trường](#yêu-cầu-công-cụ-và-môi-trường) |
| 8 | [Hướng dẫn cài đặt và cấu hình môi trường](#hướng-dẫn-cài-đặt-và-cấu-hình-môi-trường) |
| 9 | [Nhóm thực hiện](#nhóm-thực-hiện) |

---

## Giới thiệu

Ứng dụng Quản lý Bán Trà Sữa là hệ thống hỗ trợ số hóa quy trình vận hành của một cửa hàng trà sữa, từ tiếp nhận và xử lý đơn hàng đến quản lý menu, thanh toán và theo dõi trạng thái đơn hàng theo thời gian thực.

Hệ thống phục vụ hai nhóm người dùng chính: **khách hàng** và **nhân viên / quản lý cửa hàng**. Ứng dụng hướng đến việc giảm thời gian xử lý đơn hàng, hạn chế sai sót trong quá trình vận hành và mang lại trải nghiệm đặt hàng thuận tiện cho khách hàng.

---

## Đối tượng người dùng

| Vai trò | Mô tả |
|---|---|
| **Khách hàng** | Đăng ký tài khoản, đăng nhập, xem menu, đặt món, thanh toán, theo dõi đơn hàng, đánh giá và quản lý hồ sơ cá nhân. |
| **Nhân viên / Quản lý** | Quản lý menu, tiếp nhận và xử lý đơn hàng, cập nhật trạng thái đơn hàng và theo dõi hoạt động của cửa hàng. |

---

## Tính năng chính

### Tài khoản và bảo mật

- Đăng ký tài khoản
- Đăng nhập
- Quên mật khẩu
- Thay đổi mật khẩu
- Chỉnh sửa thông tin cá nhân
- Xác thực và phân quyền người dùng

### Quản lý menu

- Xem danh sách đồ uống
- Xem chi tiết đồ uống
- Tìm kiếm và lọc đồ uống
- Thêm đồ uống
- Chỉnh sửa thông tin đồ uống
- Quản lý menu dành cho tài khoản nhân viên / quản lý

### Đặt hàng và thanh toán

- Tạo đơn hàng
- Thêm, sửa và hủy món trong đơn hàng
- Chọn hình thức **tại chỗ hoặc mang về**
- Chọn chỗ ngồi khi sử dụng dịch vụ tại cửa hàng
- Áp dụng mã giảm giá và khuyến mãi
- Xử lý thông tin thanh toán
- Gửi hóa đơn / biên nhận qua email

### Quản lý và theo dõi đơn hàng

- Tiếp nhận và xử lý đơn hàng
- Cập nhật trạng thái đơn hàng
- Theo dõi trạng thái đơn hàng theo thời gian thực
- Xem lịch sử đặt hàng

### Tương tác khách hàng

- Đánh giá sản phẩm / đơn hàng
- Liên hệ với cửa hàng
- Gửi góp ý

---

## Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Ngôn ngữ lập trình | `C#` |
| Ứng dụng khách hàng | `WinForms` |
| Backend | `ASP.NET Core Web API` (`.NET 8.0`) |
| Cơ sở dữ liệu | `MySQL` |
| ORM | `Entity Framework Core` |
| Giao tiếp thời gian thực | `ASP.NET Core SignalR` |
| Gửi email | `MailKit` / `MimeKit` |
| Xác thực | `JWT (JSON Web Token)` |
| Băm mật khẩu | `BCrypt` |
| Kiểm thử API | `Postman` |
| Quản lý mã nguồn | `Git / GitHub` |

---

## Cấu trúc thư mục

```text
├── src/
│   ├── Shared/
│   │
│   ├── Server.Api/
│   │   ├── Controllers/
│   │   ├── Services/
│   │   ├── Models/
│   │   ├── Data/
│   │   └── Hubs/
│   │
│   ├── Client.Customer/
│   │
│   └── Client.Staff/
│
├── docs/
│
└── database/
```

### Mô tả các thư mục chính

| Thư mục | Vai trò |
|---|---|
| `src/Shared/` | Chứa các thành phần dùng chung giữa các project trong hệ thống. |
| `src/Server.Api/` | Backend chính của hệ thống, cung cấp REST API và SignalR Hub. |
| `src/Server.Api/Controllers/` | Xử lý các HTTP request và định nghĩa các API endpoint. |
| `src/Server.Api/Services/` | Chứa logic nghiệp vụ và các service của hệ thống. |
| `src/Server.Api/Models/` | Chứa các model / DTO được sử dụng trong backend. |
| `src/Server.Api/Data/` | Quản lý DbContext và kết nối với cơ sở dữ liệu MySQL. |
| `src/Server.Api/Hubs/` | Chứa SignalR Hub phục vụ giao tiếp thời gian thực. |
| `src/Client.Customer/` | Ứng dụng WinForms dành cho khách hàng. |
| `src/Client.Staff/` | Ứng dụng WinForms dành cho nhân viên / quản lý. |
| `docs/` | Tài liệu mô tả, thiết kế và hướng dẫn của dự án. |
| `database/` | Chứa script khởi tạo và cấu hình cơ sở dữ liệu. |

---

## Kiến trúc tổng quan

Hệ thống được tổ chức theo mô hình **client-server**, trong đó các ứng dụng WinForms giao tiếp với `Server.Api` thông qua REST API và SignalR.

```text
Client.Customer (WinForms)
        │
        ├── REST API ──────► Server.Api
        │                       │
        │                       ├── Controllers
        │                       ├── Services
        │                       ├── JWT / BCrypt
        │                       ├── MailKit
        │                       └── Data ──────► MySQL
        │
        └── SignalR ──────► Hubs
                             
Client.Staff (WinForms)
        │
        ├── REST API ──────► Server.Api
        └── SignalR ───────► Hubs

```

### Luồng giao tiếp chính

**REST API**

Các ứng dụng `Client.Customer` và `Client.Staff` gửi HTTP request đến `Server.Api`. Backend tiếp nhận request thông qua `Controllers`, xử lý logic nghiệp vụ tại `Services` và truy xuất dữ liệu thông qua `Entity Framework Core`.

**SignalR**

`Server.Api` cung cấp SignalR Hub tại thư mục `Hubs/`. SignalR được sử dụng để đẩy các cập nhật trạng thái đơn hàng đến client theo thời gian thực mà không cần client liên tục gửi request để kiểm tra trạng thái mới.

**MySQL**

Backend sử dụng `Entity Framework Core` để giao tiếp với `MySQL Database`, thực hiện các thao tác đọc, thêm, sửa và xóa dữ liệu.

**MailKit / MimeKit**

Backend sử dụng `MailKit` / `MimeKit` để gửi email, chẳng hạn như hóa đơn hoặc biên nhận sau khi đơn hàng được xử lý.

---

## Yêu cầu công cụ và môi trường

| Công cụ | Phiên bản đề xuất | Ghi chú |
|---|---|---|
| .NET SDK | `8.0+` | Dùng để build và chạy backend |
| Visual Studio | `2022 (17.8+)` | Hỗ trợ phát triển ASP.NET Core và WinForms |
| MySQL Server | `8.0+` | Cơ sở dữ liệu chính |
| MySQL Workbench | Bản mới nhất | Quản trị và thiết kế cơ sở dữ liệu |
| Git | Bản mới nhất | Quản lý mã nguồn |
| Postman | Bản mới nhất | Kiểm thử REST API |

### Workload Visual Studio

Khi cài đặt Visual Studio, cần đảm bảo có các workload:

- **.NET desktop development**
- **ASP.NET and web development**

### Các gói NuGet chính

| Gói | Vai trò |
|---|---|
| `Microsoft.EntityFrameworkCore` | ORM và truy vấn dữ liệu |
| `Pomelo.EntityFrameworkCore.MySql` | Provider kết nối EF Core với MySQL |
| `MailKit` | Gửi email thông qua SMTP |
| `MimeKit` | Tạo nội dung email và file đính kèm |
| `System.IdentityModel.Tokens.Jwt` | Tạo và xác thực JWT |
| `BCrypt.Net-Next` | Băm và kiểm tra mật khẩu |

> **Lưu ý:** SignalR server được tích hợp trong ASP.NET Core. Không cần triển khai một `SignalR Server` riêng ngoài `Server.Api`.

---

## Hướng dẫn cài đặt và cấu hình môi trường

### 1. Sao chép mã nguồn

Clone repository của dự án:

```bash
git clone <repository-url>
cd <project-folder>
```

Mở solution của dự án bằng **Visual Studio 2022**.

---

### 2. Khôi phục các gói NuGet

Tại thư mục chứa solution, chạy:

```bash
dotnet restore
```

---

### 3. Tạo cơ sở dữ liệu MySQL

Mở **MySQL Workbench** và tạo database theo tên đã thống nhất trong dự án.

Ví dụ:

```sql
CREATE DATABASE tra_sua_db;
```

Sau đó chạy các script SQL trong thư mục:

```text
database/
```

> Nếu dự án sử dụng **Entity Framework Core Migration**, có thể bỏ qua bước tạo bảng thủ công và sử dụng migration để tạo / cập nhật database.

---

### 4. Cấu hình chuỗi kết nối và thông tin bảo mật

Cấu hình các giá trị cần thiết cho `Server.Api`.

Ví dụ:

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

> **Lưu ý bảo mật:** Không commit mật khẩu database, JWT secret, SMTP password hoặc các thông tin nhạy cảm khác lên GitHub. Nên sử dụng **Environment Variables**, **User Secrets** hoặc file cấu hình local không được commit.

---

### 5. Áp dụng Entity Framework Core Migration

Nếu dự án sử dụng EF Core Migration, cài đặt `dotnet-ef` nếu máy chưa có:

```bash
dotnet tool install --global dotnet-ef
```

Sau đó chạy:

```bash
dotnet ef database update
```

Lệnh này sẽ áp dụng các migration hiện có vào MySQL Database.

---

### 6. Chạy `Server.Api`

Di chuyển đến thư mục backend:

```bash
cd src/Server.Api
```

Sau đó chạy:

```bash
dotnet run
```

Backend sẽ khởi động và cung cấp các REST API endpoint cùng SignalR Hub.

Có thể sử dụng **Postman** để kiểm tra các API endpoint.

---

### 7. Chạy ứng dụng khách hàng

Sau khi `Server.Api` đã chạy, cấu hình **Base URL** của API trong các project client:

```text
src/Client.Customer/
src/Client.Staff/
```

Sau đó build và chạy ứng dụng WinForms tương ứng bằng Visual Studio.

---

### 8. Kiểm tra hệ thống

Sau khi toàn bộ thành phần được khởi động, kiểm tra các chức năng chính:

- Đăng ký và đăng nhập tài khoản
- Xem và tìm kiếm menu
- Tạo đơn hàng
- Cập nhật trạng thái đơn hàng
- Kiểm tra thông báo trạng thái đơn hàng qua SignalR
- Kiểm tra kết nối và thao tác với MySQL
- Gửi thử email để kiểm tra cấu hình MailKit
- Kiểm tra quyền truy cập giữa tài khoản khách hàng và nhân viên / quản lý

---

## Nhóm thực hiện

**Nhóm 18 — Đồ án môn NT106**

| STT | MSSV | Họ và tên | Vai trò / Nhiệm vụ |
| :---: | :---: | :--- | :--- |
| 1 | 24522071 | Nguyễn Thúy Vy | Leader / Backend & CSDL |
| 2 | 25521179 | Nguyễn Ngọc Kim Ngân | Frontend & Báo cáo |

---
