# Hợp đồng API cho phạm vi giữa kỳ

Tài liệu này chốt API đăng ký, đăng nhập, đặt lại mật khẩu và trang chủ khách hàng. Các ứng dụng WinForms chỉ gọi Web API. Ví dụ dùng JSON và địa chỉ phát triển hiện tại `http://localhost:5227`.

## Cấu trúc phản hồi chung

Phản hồi thành công:

```json
{
  "success": true,
  "message": "...",
  "data": {}
}
```

Lỗi dùng cùng cấu trúc, `success: false` và `data: null`. Thời gian theo ISO 8601 UTC. OTP 6 chữ số truyền dưới dạng chuỗi để giữ được số 0 ở đầu.

## Xác thực

### `POST /api/auth/register`

Request:

```json
{
  "fullName": "Nguyen Van A",
  "phone": "0901234567",
  "email": "a@example.com",
  "password": "Example123!"
}
```

Trả `201 Created`, `data` gồm `userId`, `fullName`, `phone` và `email`. Sau khi đăng ký, client mở màn đăng nhập; đăng ký chưa tạo phiên đăng nhập.

### `POST /api/auth/login`

Request:

```json
{
  "email": "a@example.com",
  "password": "Example123!"
}
```

Trả `200 OK`, `data` gồm `accessToken`, `tokenType` (`Bearer`), `expiresAt` và `user` (`userId`, `fullName`, `email`, `role`). JWT có claim vai trò; giá trị hiện có là `Customer` và `Admin`.

### `POST /api/auth/forgot-password`

Request:

```json
{
  "email": "a@example.com"
}
```

Luôn trả `200 OK` với cùng thông báo, bất kể email có tồn tại hay không: `Nếu email tồn tại, mã xác nhận sẽ được gửi.` Với tài khoản hợp lệ, gửi OTP 6 chữ số, hết hạn sau 10 phút, chỉ lưu hash, chỉ dùng một lần và giới hạn gửi lại 60 giây. Trong Development có thể dùng LogEmailSender để ghi OTP ra log phục vụ demo.

### `POST /api/auth/reset-password`

Request:

```json
{
  "email": "a@example.com",
  "otp": "012345",
  "newPassword": "NewExample123!"
}
```

Trả `200 OK` khi thành công. Mã sai, hết hạn, đã dùng hoặc không tồn tại trả `400 Bad Request` với thông báo an toàn cho người dùng. Khi đặt lại thành công, đánh dấu token đã dùng và thay password hash.

## Trang chủ khách hàng

### `GET /api/categories`

Trả `200 OK`, `data` là mảng `{ "categoryId": 1, "categoryName": "Trà sữa" }`.

### `GET /api/products?featured=true`

Trả `200 OK`, `data` là mảng `{ "productId": 1, "categoryId": 1, "productName": "Trà sữa trân châu", "price": 35000, "imageUrl": null }`. Chỉ trả món đang bán. ERD hiện chưa có cờ đánh dấu món nổi bật; đến khi nhóm thêm cờ hoặc thống nhất quy tắc chọn món, `featured=true` trả danh sách món đang bán theo `productId` tăng dần để giao diện hiển thị ở khu vực nổi bật.

## Quy ước lỗi và client

- Lỗi kiểm tra dữ liệu trả `400 Bad Request`; email hoặc số điện thoại đăng ký bị trùng trả `409 Conflict`.
- Sai thông tin đăng nhập trả `401 Unauthorized` với thông báo chung.
- Client lưu token sau đăng nhập và gửi header `Authorization: Bearer <accessToken>` khi gọi API cần xác thực.
- Màn đăng ký và đặt lại mật khẩu không cần token. API quên mật khẩu không tiết lộ email có tài khoản hay không.
