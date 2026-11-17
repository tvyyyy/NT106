# Backend khuyến nghị — phần của Vy

Triển khai các việc 1, 2, 10–13 trong `phan_cong_v3.xlsx`. Seeder, client và giao diện do Tài và Ngân triển khai theo phân công.

## Hợp đồng API

Các route bắt đầu bằng `/api/recommendations`, trả envelope `ApiResponse<IReadOnlyList<RecommendedProductDto>>` như các API hiện tại (`success`, `message`, `data`). Danh sách rỗng là kết quả hợp lệ khi chưa có dữ liệu đủ điều kiện.

| GET | Tham số | Quyền |
| --- | --- | --- |
| `/popular` | `days` mặc định 7, dùng 30 cho tháng; `topN` mặc định 8 | Công khai |
| `/top-rated` | `topN` | Công khai |
| `/new` | `topN` | Công khai |
| `/for-you` | `userId` tùy chọn, `topN` | JWT; `userId` phải trùng `sub` nếu truyền |

`topN` nhận 1–50, `days` nhận 1–365. Tham số không hợp lệ trả 400; thiếu JWT trả 401; yêu cầu lịch sử của người khác trả 403. Client gửi JWT qua ApiClient hiện có. Không cần truyền `userId` nếu chỉ lấy gợi ý cho tài khoản đang đăng nhập.

DTO dùng chung ở `Application/Recommendations/Dtos`: `ProductID`, `Name`, `Price` (giá hiện tại của món, chưa cộng phụ thu size), `ImageURL`, `Score`, `Reason`, `PreferredSize`, `PreferredSugarLevel`, `PreferredIceLevel`. Các trường cấu hình là enum nullable, có giá trị khi đủ lịch sử cá nhân. Serializer hiện tại biểu diễn enum bằng số theo enum Domain. Không sao chép DTO sang client.

`Score` là giá trị xếp hạng trong từng nhóm, không phải xác suất: số ly cho bán chạy, trung bình sao cho đánh giá, và giá trị xếp hạng riêng cho các nhóm còn lại. Dùng thứ tự API trả về; không so sánh score giữa các endpoint.

## Quy tắc

Chỉ gợi ý món `IsAvailable`. Bán chạy chỉ tính Quantity từ đơn Completed trong cửa sổ UTC, loại thời gian tương lai. Đánh giá phải có 1–5 sao, thuộc đơn Completed của chính người đánh giá và đơn có món đó; đủ MinReviews mới được xếp hạng. Món mới dựa trên Products.CreatedAt trong 14 ngày.

Cá nhân hóa chỉ dùng đơn Completed. Dưới MinOrdersForPersonal (mặc định 3) dùng danh sách Cấp 1, gộp bán chạy, đánh giá cao và món mới, bỏ món trùng. Đủ lịch sử thì xen kẽ món hay đặt với món chưa mua trong danh mục ưa thích. Size, đường và đá được chọn riêng theo tổng số ly của món đó; món chưa mua dùng lịch sử cùng danh mục. Các trường `preferredSize`, `preferredSugarLevel`, `preferredIceLevel` là enum nullable, trả null cho Cấp 1 và fallback; JSON dùng giá trị số theo cấu hình mặc định hiện tại. Không dùng collaborative filtering hoặc dữ liệu bên ngoài.

## Cấu hình và chạy

Section cấu hình tùy chọn `Recommendations` có `TopN=8`, `PopularDays=7`, `NewArrivalDays=14`, `MinReviews=5`, `MinOrdersForPersonal=3`. Có thể dùng biến môi trường như `Recommendations__MinReviews=5`; cấu hình sai khiến server dừng ngay khi khởi động.

```bash
dotnet ef database update \
  --project src/Infrastructure/BobaShop.Persistence \
  --startup-project src/Presentation/BobaShop.WebAPI
bash scripts/check-db.sh
dotnet run --project src/Presentation/BobaShop.WebAPI
```

Trong Development, Program.cs hiện tự migrate khi khởi động. Chạy kiểm tra DB với Docker/PostgreSQL và cấu hình local của nhóm. Request mẫu nằm trong `BobaShop.WebAPI.http`; lấy JWT từ API login rồi thay biến `token`.

Migration chỉ thêm Products.CreatedAt, mặc định SQL NOW(). Các món đã tồn tại nhận thời điểm migrate nên có thể xuất hiện trong nhóm món mới trong 14 ngày đầu; Tài cần gán CreatedAt lịch sử cho catalog seed theo phân công. Không suy đoán ngày tạo cũ vì schema trước đây chưa lưu thông tin đó.

Không có weatherforecast mẫu trong Program.cs hiện tại nên không cần xóa. Giữ nguyên controller và luồng auth đang có; endpoint khuyến nghị dùng MapGroup theo phân công v3.

## Kết quả kiểm tra ngày 17/11/2026

- `dotnet build Boba.slnx --no-restore`: thành công, 0 lỗi; có 4 cảnh báo NU1902/NU1903 từ package System.Security.Cryptography.Pkcs 10.0.0 đã có trước thay đổi này.
- Build lại WebAPI sau khi hoàn thiện đăng ký options: thành công.
- `dotnet ef migrations has-pending-model-changes ... --no-build`: model khớp migration.
- 9 request HTTP thực tế: kiểm tra giới hạn topN/days, kiểu tham số, thiếu JWT và userId khác tài khoản; đều trả đúng 400/401/403.
- Harness tạm dùng provider Npgsql và interceptor: cả 14 lệnh truy vấn được dịch thành SQL; kiểm tra fixture lịch sử đủ/thiếu đơn, món mua lại và món chưa mua, loại món hết hàng và cấu hình theo số ly đều đạt. Interceptor cung cấp dữ liệu giả, không thay thế kiểm thử PostgreSQL thật.
- Chưa chạy được migration trên DB: kết nối localhost:5432 bị từ chối; Docker daemon chưa chạy. Chưa chạy scripts/check-db.sh và kiểm thử end-to-end với dữ liệu seed của Tài.
